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

    [Header("Consumables")]
    public Button clue_Consumable_Button;
    public TextMeshProUGUI clue_consumable_price_text;
    public TextMeshProUGUI clue_consumable_description_text;
    public TextMeshProUGUI clue_consumable_amount_text;
    [Space]
    public Button remove_Consumable_Button;
    public TextMeshProUGUI remove_consumable_price_text;
    public TextMeshProUGUI remove_consumable_description_text;
    public TextMeshProUGUI remove_consumable_amount_text;
    [Space]
    public Button cut_Consumable_Button;
    public TextMeshProUGUI cut_consumable_price_text;
    public TextMeshProUGUI cut_consumable_description_text;
    public TextMeshProUGUI cut_consumable_amount_text;
    [Space]
    public Button peek_Consumable_Button;
    public TextMeshProUGUI peek_consumable_price_text;
    public TextMeshProUGUI peek_consumable_description_text;
    public TextMeshProUGUI peek_consumable_amount_text;
    [Space]
    [Header("Upgrades")]
    public Button maxClue_Upgrade_Button;
    public TextMeshProUGUI maxClue_upgrade_price_text;
    public TextMeshProUGUI maxClue_upgrade_amount_text;
    [Space]

    public Button maxRemove_Upgrade_Button;
    public TextMeshProUGUI maxRemove_upgrade_price_text;
    public TextMeshProUGUI maxRemove_upgrade_amount_text;
    [Space]

    public Button maxCut_Upgrade_Button;
    public TextMeshProUGUI maxCut_upgrade_price_text;
    public TextMeshProUGUI maxCut_upgrade_amount_text;
    [Space]

    public Button betterPeek_Upgrade_Button;
    public TextMeshProUGUI betterPeek_upgrade_price_text;
    public TextMeshProUGUI betterPeek_upgrade_amount_text;
    [Space]

    public Button betterClue_Upgrade_Button;
    public Button betterCut_Upgrade_Button;
    public Button extraLife_Upgrade_Button;
    [Space]

    public Button protectedLife;
    public Button block;
    public Button deathDefy_Upgrade_Button;
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
        int price = UpgradeData.GetUpgrade(item).GetPrice();
        if (GameManager.Instance.userData.upgrades.ContainsKey(item))
            price = GameManager.Instance.userData.upgrades[item].GetPrice();
        bool canBuy = GameManager.Instance.userData.ModifyCoins(-price);
        if (!canBuy)
        {
            Debug.Log("Not enough money");
            return;
        }
        if (itemID < 0) return;
        UpgradeID upgradeID = (UpgradeID)itemID;
        Debug.Log("Added Item:" + nameof(upgradeID));
        GameManager.Instance.userData.AddUpgradeObject(upgradeID);
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
        UserData data = GameManager.Instance.userData;
        if (clue_Consumable_Button != null)
        {
            if (data.consumables.ContainsKey(ConsumableID.Clue))
            {
                clue_consumable_amount_text.text = "Owned: " + data.consumables[ConsumableID.Clue].current.ToString();
                clue_consumable_price_text.text = "BUY\n$" + data.consumables[ConsumableID.Clue].price.ToString();
                clue_consumable_description_text.text = data.consumables[ConsumableID.Clue].description;
            }
            else
            {
                clue_consumable_amount_text.text = "Owned: 0";
                clue_consumable_price_text.text = "BUY\n$" + ConsumableData.GetConsumable(ConsumableID.Clue).price.ToString();
                clue_consumable_description_text.text = ConsumableData.GetConsumable(ConsumableID.Clue).description;
            }
        }
        if (remove_Consumable_Button != null)
        {
            if (data.consumables.ContainsKey(ConsumableID.Remove))
            {
                remove_consumable_amount_text.text = "Owned: " + data.consumables[ConsumableID.Remove].current.ToString();
                remove_consumable_price_text.text = "BUY\n$" + data.consumables[ConsumableID.Remove].price.ToString();
                remove_consumable_description_text.text = data.consumables[ConsumableID.Remove].description;
            }   
            else
            {   
                remove_consumable_amount_text.text = "Owned: 0";
                remove_consumable_price_text.text = "BUY\n$" + ConsumableData.GetConsumable(ConsumableID.Remove).price.ToString();
                remove_consumable_description_text.text = ConsumableData.GetConsumable(ConsumableID.Remove).description;

            }
        }
        if (cut_Consumable_Button != null)
        {
            if (data.consumables.ContainsKey(ConsumableID.Cut))
            {
                cut_consumable_amount_text.text = "Owned: " + data.consumables[ConsumableID.Cut].current.ToString();
                cut_consumable_price_text.text = "BUY\n$" + data.consumables[ConsumableID.Cut].price.ToString();
                cut_consumable_description_text.text = data.consumables[ConsumableID.Cut].description;
            }
            else
            {   
                cut_consumable_amount_text.text = "Owned: 0";
                cut_consumable_price_text.text = "BUY\n$" + ConsumableData.GetConsumable(ConsumableID.Cut).price.ToString();
                cut_consumable_description_text.text = ConsumableData.GetConsumable(ConsumableID.Cut).description;

            }
        }
        if (peek_Consumable_Button != null)
        {
            if (data.consumables.ContainsKey(ConsumableID.Peek))
            {
                peek_consumable_amount_text.text = "Owned: " + data.consumables[ConsumableID.Peek].current.ToString();
                peek_consumable_price_text.text = "BUY\n$" + data.consumables[ConsumableID.Peek].price.ToString();
                peek_consumable_description_text.text = data.consumables[ConsumableID.Peek].description;
            }   
            else
            {   
                peek_consumable_amount_text.text = "Owned: 0";
                peek_consumable_price_text.text = "BUY\n$" + ConsumableData.GetConsumable(ConsumableID.Peek).price.ToString();
                peek_consumable_description_text.text = ConsumableData.GetConsumable(ConsumableID.Peek).description;

            }
        }
    }
    private void UpdateUpgradeButtonsIU()
    {
        if (maxClue_Upgrade_Button != null)
        {
            SetUpgradeButton(maxClue_Upgrade_Button, maxClue_upgrade_amount_text, maxClue_upgrade_price_text, UpgradeID.MaxClue);
        }
        if (maxCut_Upgrade_Button != null)
        {
            SetUpgradeButton(maxCut_Upgrade_Button, maxClue_upgrade_amount_text, maxCut_upgrade_price_text, UpgradeID.MaxCut);
        }
    }

    private void SetUpgradeButton(Button button, TextMeshProUGUI amount, TextMeshProUGUI price, UpgradeID upgradeID)
    {
        UserData data = GameManager.Instance.userData;

        if (data.upgrades.ContainsKey(upgradeID))
        {
            if (data.upgrades[upgradeID].upgradeRequired.Count > 0)
            {
                for (int i = 0; i < data.upgrades[upgradeID].upgradeRequired.Count; i++)
                {
                    if (IsUpgradeHasRequiered(upgradeID, i))
                    {
                        if (data.upgrades[(UpgradeID)i].currentLevel >= data.upgrades[upgradeID].upgradeRequired[(UpgradeID)i])
                        {
                            Debug.Log("HAS");
                            button.interactable = true;
                        }
                    }
                }
            }
            else
            {
                            Debug.Log("Dont HAS");
                button.interactable = true;
            }
            amount.text = data.upgrades[upgradeID].currentLevel.ToString();
            price.text = data.upgrades[upgradeID].GetPrice().ToString();

            if (data.upgrades[upgradeID].IsMaxLevel())
            {
                button.interactable = false;
            }
        }
        else
        {
            amount.text = "0";
            price.text = UpgradeData.GetUpgrade(upgradeID).GetPrice().ToString();
        }
    }

    private bool IsUpgradeHasRequiered(UpgradeID upgrade, int i)
    {
        return GameManager.Instance.userData.upgrades.ContainsKey((UpgradeID)GameManager.Instance.userData.upgrades[upgrade].upgradeRequired[(UpgradeID)i]);
    }
}



