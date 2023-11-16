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
    public Button beathDefy_Upgrade_Button;
    public void BuyItem(int itemID)
    {
        ItemID item = (ItemID)itemID;
        Debug.Log("Added Item:" + item.ToString());
        int price = ItemManager.Instance.GetItemPrice(item);
        int money = GameManager.Instance.currentBuyScore;
        if (money < price)
        {   
            Debug.Log("Not enought money");        
            return; 
        }

        money -= price;
        GameManager.Instance.currentBuyScore = money;
        ProcessItem(item);
    }
    [ContextMenu("Add Money")]
    public void AddMoney()
    {
        GameManager.Instance.ModifyScore(1000);
    }
    private void ProcessItem(ItemID itemId)
    {
        Debug.Log($"{itemId} Purchase Successfully");
        ItemManager.Instance.AddItem(itemId);
    }
}



