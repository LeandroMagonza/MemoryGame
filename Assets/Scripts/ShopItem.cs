using UnityEngine;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour
{
    public int price = 200;
    public ShopItemType type;
    public int maxLevel = 2;
    public ShopItem shopItemNeeded;
    public int level  {get; private set;}
    
    private Button button;
    private void Awake()
    {
        button = GetComponent<Button>();
    }
    private void OnEnable()
    {
        button.interactable = CanBuyItem();
    }

    private bool CanBuyItem()
    {
        if (shopItemNeeded == null)
            return GameManager.Instance.score < price || level >= maxLevel;
        else
            return GameManager.Instance.score < price || level >= maxLevel && shopItemNeeded?.level > 0;
    }

    public void BuyItem()
    {
        GameManager.Instance.ModifyScore(-price);
        level++;
        SetItem();
    }

    private void SetItem()
    {
        switch (type)
        {
            case ShopItemType.Consumible_Clue:
                GameManager.Instance.AddItemToInventary(ShopItemType.Consumible_Clue);
                break;
            case ShopItemType.Consumible_Cut:
                GameManager.Instance.AddItemToInventary(ShopItemType.Consumible_Cut);
                break;
            case ShopItemType.Consumible_Remove:
                GameManager.Instance.AddItemToInventary(ShopItemType.Consumible_Remove);
                break;
            case ShopItemType.Consumible_Peek:
                GameManager.Instance.AddItemToInventary(ShopItemType.Consumible_Peek);
                break;
            case ShopItemType.Upgrade_ExtraLife:
                GameManager.Instance.AddUpgrade(ShopItemType.Upgrade_ExtraLife);
                break;
            case ShopItemType.Upgrade_ProtectedLife:
                GameManager.Instance.AddUpgrade(ShopItemType.Upgrade_ProtectedLife);
                break;
            case ShopItemType.Upgrade_MaxClue:
                GameManager.Instance.AddUpgrade(ShopItemType.Upgrade_MaxClue);
                break;
            case ShopItemType.Upgrade_BetterClue:
                GameManager.Instance.AddUpgrade(ShopItemType.Upgrade_BetterClue);
                break;
            case ShopItemType.Upgrade_MaxRemove:
                GameManager.Instance.AddUpgrade(ShopItemType.Upgrade_MaxRemove);
                break;
            case ShopItemType.Upgrade_MaxCut:
                GameManager.Instance.AddUpgrade(ShopItemType.Upgrade_MaxCut);
                break;
            case ShopItemType.Upgrade_BetterCut:
                GameManager.Instance.AddUpgrade(ShopItemType.Upgrade_BetterCut);
                break;
            case ShopItemType.Upgrade_BetterPeek:
                GameManager.Instance.AddUpgrade(ShopItemType.Upgrade_BetterPeek);
                break;
            case ShopItemType.Upgrade_Block:
                GameManager.Instance.AddUpgrade(ShopItemType.Upgrade_Block);
                break;
            case ShopItemType.Upgrade_DeathDefy:
                GameManager.Instance.AddUpgrade(ShopItemType.Upgrade_DeathDefy);
                break;
        }
    }
}
