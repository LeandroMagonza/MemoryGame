using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public TextMeshProUGUI moneyDisplay;

    public Button clue_Consumable_Button;
    public Button remove_Consumable_Button;
    public Button cut_Consumable_Button;
    public Button peek_Consumable_Button;


    public Button maxClue_Upgrade_Button;
    public Button maxRemove_Upgrade_Button;
    public Button maxCut_Upgrade_Button;
    public Button betterPeek_Upgrade_Button;

    public Button betterClue_Upgrade_Button;
    public Button betterCut_Upgrade_Button;
    public Button extraLife_Upgrade_Button;

    public Button protectedLife;
    public Button block;
    public Button deathDefy_Upgrade_Button;
    private void OnEnable()
    {
        UpdateMoneyDisplay();
    }
    public void BuyItemConsumable(int itemID)
    {
        ConsumableID item = (ConsumableID)itemID;
        Debug.Log("Added Item:" + item.ToString());
        int price = ItemPrizes.GetItemPrice(item);
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
    }
    public void BuyItemUpgrade(int itemID)
    {
        UpgradeID item = (UpgradeID)itemID;
        Debug.Log("Added Item:" + item.ToString());
        //int price = ItemPrizes.GetItemPrice(item);
        bool canBuy = GameManager.Instance.userData.ModifyCoins(-100);
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


    private void UpdateButtonsIU()
    {
        UserData data = GameManager.Instance.userData;
        if (maxClue_Upgrade_Button != null)
        {
            if (data.upgrades.ContainsKey(UpgradeID.MaxClue))
            {
                if (data.upgrades[UpgradeID.MaxClue].upgradeRequired.Count > 0) 
                {
                    for (int i = 0; i < data.upgrades[UpgradeID.MaxClue].upgradeRequired.Count; i++)
                    {

                    }
                }
                else
                {
                    maxClue_Upgrade_Button.interactable = true;
                }
            }
        }
    }
}



