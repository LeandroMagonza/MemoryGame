using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public Button clue_Consumable_Button;
    public Button betterClue_Consumable_Button;
    public Button remove_Consumable_Button;
    public Button cut_Consumable_Button;
    public Button peek_Consumable_Button;

    public Button extraLife_Upgrade_Button;
    public Button protectedLife;
    public Button maxClue_Upgrade_Button;
    public Button maxRemove_Upgrade_Button;
    public Button maxCut_Upgrade_Button;
    public Button betterCut_Upgrade_Button;
    public Button betterPeek_Upgrade_Button;
    public Button block;
    public Button deathDefy_Upgrade_Button;

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
        ProcessConsumable(item);
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
        ProcessUpgrade(item);
    }
    public void AddMoney()
    {
        GameManager.Instance.userData.ModifyCoins(1000);
    }
    private void ProcessConsumable(ConsumableID consumableID)
    {
        Debug.Log($"{consumableID} Purchase Successfully");
        ItemManager.Instance.ProcessAddConsumable(consumableID);
    }
    private void ProcessUpgrade(UpgradeID upgradeID)
    {
        Debug.Log($"{upgradeID} Purchase Successfully");
        ItemManager.Instance.ProcessAddUpgrade(upgradeID);
    }
}



