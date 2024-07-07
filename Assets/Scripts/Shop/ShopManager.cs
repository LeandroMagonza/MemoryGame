using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public enum OrderItemBy
{
    Price,
    Active
}
public class ShopManager : MonoBehaviour
{

    public GameObject consumablePanel;
    public GameObject consumableTittle;
    public GameObject upgradePanel;
    public GameObject upgradeTitle;
    public TextMeshProUGUI moneyDisplay;
    public ShopButton[] shopButtons;
    public Animator animatorMessage;
    private Dictionary<ConsumableID, (ConsumableButton button, Transform panel, ConsumableData data)> shopConsumableButtons = new Dictionary<ConsumableID, (ConsumableButton,Transform,ConsumableData)>();
    private Dictionary<UpgradeID, (UpgradeButton button, Transform panel, UpgradeData data)> shopUpgradeButtons = new Dictionary<UpgradeID, (UpgradeButton,Transform,UpgradeData)>();

    public UserData userData => PersistanceManager.Instance.userData;
    private const string _lock = "Ω\n▀";
    private const string _max = "MAX";
    private void Awake()
    {
        foreach (ShopButton button in shopButtons)
        {
            if (button is ConsumableButton)
            {
                ConsumableButton consumable = (ConsumableButton)button;

                if (!shopConsumableButtons.ContainsKey(consumable.ID))
                {
                    ConsumableData data = ConsumableData.GetConsumable(consumable.ID);
                    shopConsumableButtons.Add(consumable.ID, (consumable, consumable.transform.parent, data));
                }
            }
            else if (button is UpgradeButton) 
            {
                UpgradeButton upgrade = (UpgradeButton)button;

                if (!shopUpgradeButtons.ContainsKey(upgrade.ID))
                {
                    UpgradeData data = UpgradeData.GetUpgrade(upgrade.ID);
                    shopUpgradeButtons.Add(upgrade.ID, (upgrade, upgrade.transform.parent, data));
                }
            }
        }
    }
    private void Start()
    {
        OrderUpgradePanels();
    }
    private void OnEnable()
    {
        EnableShopCanvas();
        UpdateMoneyDisplay();
        UpdateConsumableButtonsUI();
        UpdateUpgradeButtonsIU();
    }

    private void EnableShopCanvas()
    {
        consumablePanel.SetActive(false);
        consumableTittle.SetActive(false);
        upgradePanel.SetActive(true);
        upgradeTitle.SetActive(true);
    }

    public void BuyItemConsumable(int itemID)
    {
        ConsumableID item = (ConsumableID)itemID;
        CustomDebugger.Log("Added Item:" + item.ToString());
        int price = ConsumableData.GetConsumable(ConsumableID.Clue).price;
        bool canBuy = GameManager.Instance.userData.ModifyCoins(-price);
        if (!canBuy)
        {
            string message = "Not enough money";
            animatorMessage.GetComponentInChildren<TextMeshProUGUI>().text = message;
            animatorMessage.SetTrigger("play");
            CustomDebugger.Log(message);
            return;
        }
        if (itemID < 0) return;
        ConsumableID consumableID = (ConsumableID)itemID;
        CustomDebugger.Log("Added Item:" + nameof(consumableID));
        PersistanceManager.Instance.userConsumableData.ModifyConsumable(consumableID,1);
        PersistanceManager.Instance.SaveUserData();
        UpdateMoneyDisplay();
        UpdateConsumableButtonsUI();
        UpdateUpgradeButtonsIU();

    }

    public void OrderUpgradePanels()
    {
        var upgradesOrder = shopUpgradeButtons.Values
            .OrderBy(tuple => tuple.data.levelPrices[0])
            .OrderByDescending(tuple => tuple.button.button.interactable)
            .ToList();
        foreach (var upgrade in upgradesOrder)
        {
            upgrade.panel.SetAsLastSibling();
        }
    }
    public void BuyItemUpgrade(int itemID)
    {
        UpgradeID item = (UpgradeID)itemID;
        CustomDebugger.Log("Added Item:" + item.ToString());
        int currentLevel = 0;
        if (userData.unlockedUpgrades.ContainsKey(item))
        {
            currentLevel = userData.unlockedUpgrades[item];
        }

        int price = UpgradeData.GetUpgrade(item).GetPrice(currentLevel);
        
        bool canBuy = GameManager.Instance.userData.ModifyCoins(-price);
        if (!canBuy)
        {
            string message = "Not enough money";
            animatorMessage.GetComponentInChildren<TextMeshProUGUI>().text = message;
            animatorMessage.SetTrigger("play");
            CustomDebugger.Log(message);
            return;
        }

        CustomDebugger.Log("Added Item:" + nameof(itemID));
        GameManager.Instance.userData.AddUpgradeToUser(item);
        PersistanceManager.Instance.SaveUserData();
        UpdateMoneyDisplay();
        UpdateConsumableButtonsUI();
        UpdateUpgradeButtonsIU();

    }
    [ContextMenu("AddMoney")]
    public void AddMoney()
    {
        GameManager.Instance.userData.ModifyCoins(1000000);
        UpdateMoneyDisplay();
    }
    private void UpdateMoneyDisplay()
    {
        moneyDisplay.text = GameManager.Instance.userData.coins.ToString();
    }

    private void UpdateConsumableButtonsUI()
    {
        foreach (var item in shopConsumableButtons)
        {
            SetConsumableButton(item.Key);
        }
    }

    private void SetConsumableButton(ConsumableID consumableID)
    {
        ShopButton shopButton = shopConsumableButtons[consumableID].button;
            shopButton.currentText.text = 
                "Owned: " +
            PersistanceManager.Instance.userConsumableData.GetConsumableEntry(consumableID).amount;

        shopButton.priceText.text = ConsumableData.GetConsumable(consumableID).price.ToString();
        shopButton.descriptionText.text  = ConsumableData.GetConsumable(consumableID).GenerateDescription();

    }

    [ContextMenu("UpdateUpgrade")]
    private void UpdateUpgradeButtonsIU()
    {
        foreach (var item in shopUpgradeButtons)
        {
            SetUpgradeButton(item.Key);
        }
       
    }

    private void SetUpgradeButton(UpgradeID upgradeID)
    {

        int currentLevel = 0;

        if (userData.unlockedUpgrades.ContainsKey(upgradeID))
        {
            currentLevel = userData.unlockedUpgrades[upgradeID];       
        }
        bool isMaxLevel = UpgradeData.GetUpgrade(upgradeID).IsMaxLevel(currentLevel);

        int max = UpgradeData.GetUpgrade(upgradeID).GetMaxLevel();

        bool requirementsMet = true;
        
        Dictionary<string, string> requirementList = new Dictionary<string, string>();
        foreach (var requirement in UpgradeData.GetUpgrade(upgradeID).upgradeRequired)
        {
            if (!requirementList.ContainsKey(requirement.Key.ToString()))
            {
                requirementList.Add(requirement.Key.ToString(), requirement.Value.ToString());
            }
        }
        foreach (var requirement in UpgradeData.GetUpgrade(upgradeID).upgradeRequired)
        {
            UpgradeID requirementID = requirement.Key;
            int requirementLevel = requirement.Value;

            if (!userData.unlockedUpgrades.ContainsKey(requirementID) || userData.unlockedUpgrades[requirementID] < requirementLevel)
            {
                requirementsMet = false;
                break;
            }
        }
        string price = UpgradeData.GetUpgrade(upgradeID).GetPrice(currentLevel).ToString();
        price = !requirementsMet ? _lock : price;
        price = isMaxLevel ? _max : price;
        
        string description = UpgradeData.GetUpgrade(upgradeID).GenerateDescription(false);
        if (!requirementsMet)
        {
            string requirementData = "\n YOU NEED: ";
            foreach(string upgrade in requirementList.Keys)
            {
                string upgradeNameFormatted = Regex.Replace(upgrade.ToString(), "([a-z])([A-Z])", "$1 $2");
                requirementData += upgradeNameFormatted + " " + requirementList[upgrade].ToString() + "\n";
            }
            description += requirementData; 
        }

        var upgradeButton = shopUpgradeButtons[upgradeID].button;
        upgradeButton.button.interactable = requirementsMet && !isMaxLevel;
        upgradeButton.currentText.text = currentLevel.ToString() + "/" + max;
        upgradeButton.priceText.text = price;
        upgradeButton.priceText.color = shopUpgradeButtons[upgradeID].button.button.interactable? Color.white : Color.gray;
        upgradeButton.descriptionText.text = description;
    }
}



