using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public GameObject consumablePanel;
    public GameObject consumableTittle;
    public GameObject upgradePanel;
    public GameObject upgradeTittle;
    public TextMeshProUGUI moneyDisplay;
    public ShopButton[] shopButtons;

    private Dictionary<ConsumableID, ConsumableButton> shopConsumableButtons = new Dictionary<ConsumableID, ConsumableButton>();
    private Dictionary<UpgradeID, UpgradeButton> shopUpgradeButtons = new Dictionary<UpgradeID, UpgradeButton>();

    public UserData userData => PersistanceManager.Instance.userData;

    private void Awake()
    {
        foreach (ShopButton button in shopButtons)
        {
            if (button is ConsumableButton)
            {
                ConsumableButton consumable = (ConsumableButton)button;
                if (!shopConsumableButtons.ContainsKey(consumable.ID))
                    shopConsumableButtons.Add(consumable.ID, consumable);
            }
            else if (button is UpgradeButton) 
            {
                UpgradeButton upgrade = (UpgradeButton)button;
                if (!shopUpgradeButtons.ContainsKey(upgrade.ID)) 
                    shopUpgradeButtons.Add(upgrade.ID, upgrade);
            }
        }
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
        consumablePanel.SetActive(true);
        consumableTittle.SetActive(true);
        upgradePanel.SetActive(false);
        upgradeTittle.SetActive(false);
    }

    public void BuyItemConsumable(int itemID)
    {
        ConsumableID item = (ConsumableID)itemID;
        Debug.Log("Added Item:" + item.ToString());
        int price = ConsumableData.GetConsumable(ConsumableID.Clue).price;
        bool canBuy = GameManager.Instance.userData.ModifyCoins(-price);
        if (!canBuy)
        {
            Debug.Log("Not enough money");
            return;
        }
        if (itemID < 0) return;
        ConsumableID consumableID = (ConsumableID)itemID;
        Debug.Log("Added Item:" + nameof(consumableID));
        GameManager.Instance.userData.AddConsumableObject(consumableID);
        PersistanceManager.Instance.SaveUserData();
        UpdateMoneyDisplay();
        UpdateConsumableButtonsUI();
        UpdateUpgradeButtonsIU();

    }
    public void BuyItemUpgrade(int itemID)
    {
        UpgradeID item = (UpgradeID)itemID;
        Debug.Log("Added Item:" + item.ToString());
        int currentLevel = 0;
        if (userData.upgrades.ContainsKey(item))
        {
            currentLevel = userData.upgrades[item];
        }

        int price = UpgradeData.GetUpgrade(item).GetPrice(currentLevel);
        
        bool canBuy = GameManager.Instance.userData.ModifyCoins(-price);
        if (!canBuy)
        {
            Debug.Log("Not enough money");
            return;
        }

        Debug.Log("Added Item:" + nameof(itemID));
        GameManager.Instance.userData.AddUpgradeObject(item);
        PersistanceManager.Instance.SaveUserData();
        UpdateMoneyDisplay();
        UpdateConsumableButtonsUI();
        UpdateUpgradeButtonsIU();

    }
    [ContextMenu("AddMoney")]
    public void AddMoney()
    {
        GameManager.Instance.userData.ModifyCoins(10000);
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
        ShopButton shopButton = shopConsumableButtons[consumableID];
        if (userData.consumables.ContainsKey(consumableID))
        {
            shopButton.currentText.text = "Owned: " + userData.consumables[consumableID].ToString();
        }
        else
        {
            shopButton.currentText.text = "Owned: 0";
        }

        shopButton.priceText.text = "BUY\n$" + ConsumableData.GetConsumable(consumableID).price.ToString();
        shopButton.descriptionText.text  = ConsumableData.GetConsumable(consumableID).description;

    }

    
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

        if (userData.upgrades.ContainsKey(upgradeID))
        {
            currentLevel = userData.upgrades[upgradeID];       
        }
        bool isMaxLevel = UpgradeData.GetUpgrade(upgradeID).IsMaxLevel(currentLevel);

        bool requirementsMet = true;

        foreach (var requirement in UpgradeData.GetUpgrade(upgradeID).upgradeRequired)
        {
            UpgradeID requirementID = requirement.Key;
            int requirementLevel = requirement.Value;

            if (!userData.upgrades.ContainsKey(requirementID) || userData.upgrades[requirementID] < requirementLevel)
            {
                requirementsMet = false;
                break;
            }
        }
        shopUpgradeButtons[upgradeID].button.interactable = requirementsMet && !isMaxLevel;

        shopUpgradeButtons[upgradeID].currentText.text = currentLevel.ToString();
        shopUpgradeButtons[upgradeID].priceText.text = UpgradeData.GetUpgrade(upgradeID).GetPrice(currentLevel).ToString();
        shopUpgradeButtons[upgradeID].descriptionText.text = UpgradeData.GetUpgrade(upgradeID).description;
    }
}



