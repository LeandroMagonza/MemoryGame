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
    public void BuyItem(int itemID)
    {
        ItemID item = (ItemID)itemID;
        Debug.Log("Added Item:" + item.ToString());
        int price = ItemManager.Instance.GetItemPrice(item);
        bool canBuy = GameManager.Instance.userData.ModifyCoins(-price);
        if (!canBuy)
        {   
            Debug.Log("Not enough money");        
            return;
        }
        ProcessItem(item);
    }
    [ContextMenu("Add Money")]
    public void AddMoney()
    {
        GameManager.Instance.userData.ModifyCoins(1000);
    }
    private void ProcessItem(ItemID itemId)
    {
        Debug.Log($"{itemId} Purchase Successfully");
        ItemManager.Instance.AddItem(itemId);
    }
}



