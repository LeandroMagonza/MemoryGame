using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public enum ItemID
{
    NONE = -1,

    // Consumables

    Clue,
    Remove,
    Cut,
    Peek,

    /// Upgrdes

    ExtraLife,
    ProtectedLife,
    MaxClue,
    BetterClue,
    MaxRemove,
    MaxCut,
    BetterCut,
    BetterPeek,
    Block,
    DeathDefy
}

public class ItemManager : MonoBehaviour
{

    public bool validate = true;
    //public Dictionary<ItemID, int> matchConsumables = new Dictionary<ItemID, int>();
    public Consumable[] consumables;
    public Upgrade[] upgrades;
    public static ItemManager Instance;
    [Header("Consumable Settings")]
    [SerializeField] private Button buttonClue;
    [SerializeField] private Button buttonRemove;
    [SerializeField] private Button buttonCut;
    [SerializeField] private Button buttonPeek;

    [SerializeField] private TextMeshProUGUI buttonTextClue;
    [SerializeField] private TextMeshProUGUI buttonTextRemove;
    [SerializeField] private TextMeshProUGUI buttonTextPeek;
    [SerializeField] private TextMeshProUGUI buttonTextCut;

    [SerializeField] private AudioClip buttonClueAudioClip;
    [SerializeField] private AudioClip buttonRemoveAudioClip;
    [SerializeField] private AudioClip buttonCutAudioClip;
    [SerializeField] private AudioClip buttonPeekAudioClip;
    private void OnValidate()
    {
        if (!validate) return;
        int breakPoint = 4;
        List<ItemID> items = new List<ItemID>();
        upgrades = new Upgrade[items.Count];
        consumables = new Consumable[breakPoint];
        foreach (string item in Enum.GetNames(typeof(ItemID)))
        {
            ItemID itemID = (ItemID)Enum.Parse(typeof(ItemID), item);
            items.Add(itemID);
        }
        for (int i = 0; i < breakPoint; i++)
        {
            consumables[i] = Consumable.GetConsumable(items[i]);
        }
        items.RemoveRange(0, breakPoint);
        upgrades = new Upgrade[items.Count];
        for (int i = 0; i < items.Count; i++)
        {
            upgrades[i] = Upgrade.GetUpgrade(items[i]);
        }
    }
    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        ValidateItem(ItemID.NONE);
    }
    public void AddItem(ItemID item)
    {
        if ((int)item < 0) return;
        Debug.Log("Added Item:" + nameof(item));
        AddItemTodictionaries(item);
        ValidateItem(item);
    }

    private void AddItemTodictionaries(ItemID item)
    {
        if ((int)item > 3)
        {
            foreach(Upgrade upgrade in upgrades)
            {
                if (upgrade.ItemID.Equals(item))
                {
                    upgrade.LevelUp();
                }
            }
        }
        else
        {
            AddConsumable(item);
        }
    }
    public Dictionary<ItemID, int> MatchInventory()
    {
        Dictionary<ItemID, int> temp_inventory = new Dictionary<ItemID, int>();
        foreach (Upgrade upgrade in upgrades)
        {
            temp_inventory.Add(upgrade.ItemID, upgrade.GetAdditionalItem());
        }
        foreach (Consumable item in consumables)
        {
            if (temp_inventory.ContainsKey(item.ItemID))
            {
                temp_inventory[item.ItemID] += item.Amount;
            }
            else
            {
                temp_inventory.Add(item.ItemID, item.Amount);
            }
        }
        return temp_inventory;
    }

    private void AddConsumable(ItemID item)
    {
        foreach (Consumable c in consumables)
        {
            if (c.ItemID.Equals(item))
                c.AddCurrent(1);
        }
        /*
        if (matchConsumables.ContainsKey(item))
        {
            matchConsumables[item] += 1;
        }
        else
        {
            matchConsumables.Add(item, 1);
        }*/
    }

    public void UseClue()
    {
        //Debug.Log("USE CLUE");
        //var turnSticker = GameManager.Instance.GetCurrentlySelectedSticker();
        //if (!matchConsumables.ContainsKey(ItemID.Clue) || matchConsumables[ItemID.Clue] <= 0) return;
        //GameManager.Instance.audioSource.PlayOneShot(buttonClueAudioClip);
        ////anim
        //matchConsumables[ItemID.Clue]--;
        //if (matchConsumables[ItemID.Clue] <= 0)
        //{
        //    matchConsumables[ItemID.Clue] = 0;
        //}
        //ValidateItem(ItemID.Clue);
        //int scoreModification = GameManager.Instance.OnCorrectGuess();

        //StartCoroutine(GameManager.Instance.FinishProcessingTurnAction(
        //    turnSticker.amountOfAppearances,
        //    TurnAction.UseClue,
        //    scoreModification,
        //    turnSticker
        //    ));
    }
    
    public void UseRemove()
    {
        //Debug.Log("USE REMOVE");
        //if (!matchConsumables.ContainsKey(ItemID.Remove) || matchConsumables[ItemID.Remove] == 0) return;
        //GameManager.Instance.audioSource.PlayOneShot(buttonRemoveAudioClip);
        ////anim
        //matchConsumables[ItemID.Remove]--;
        //if (matchConsumables[ItemID.Remove] <= 0)
        //{
        //    matchConsumables[ItemID.Remove] = 0;
        //}
        //ValidateItem(ItemID.Remove);
        //GameManager.Instance.RemoveStickerFromPool();
        //GameManager.Instance.NextTurn();
    }
    private void Cut()
    {
    }
    private void Peek()
    {
    }
   
    public void ValidateItem(ItemID itemId)
    {
        SetInteractable();
        SetButtonText();
    }
    private void SetButtonText()
    {
        if (buttonTextClue != null)
            buttonTextClue.text = consumables[(int)ItemID.Clue].Amount.ToString();
        if (buttonTextPeek != null)
            buttonTextPeek.text = consumables[(int)ItemID.Peek].Amount.ToString();
        if (buttonTextCut != null)
            buttonTextCut.text = consumables[(int)ItemID.Cut].Amount.ToString();
        if (buttonTextRemove != null)
            buttonTextRemove.text = consumables[(int)ItemID.Remove].Amount.ToString();
    }
    private void SetInteractable()
    {
        if (buttonClue != null)
            buttonClue.interactable = consumables[(int)ItemID.Clue].Amount > 0;
        if (buttonRemove != null)
            buttonRemove.interactable = consumables[(int)ItemID.Remove].Amount > 0;
        if (buttonCut != null)
            buttonCut.interactable = consumables[(int)ItemID.Cut].Amount > 0;
        if (buttonPeek != null)
            buttonPeek.interactable = consumables[(int)ItemID.Peek].Amount > 0;
    }
    public void UseItem(ItemID itemId)
    {
        switch (itemId)
        {
            case ItemID.Clue:
                UseClue();
                break;
            case ItemID.Remove:
                UseRemove();
                break;
            //case ItemID.Cut:
            //    break;
            //case ItemID.Peek:
            //    break;

            //// Upgrades
            //case ItemID.ExtraLife:
            //    break;
            //case ItemID.ProtectedLife:
            //    break;
            //case ItemID.MaxClue:
            //    break;
            //case ItemID.BetterClue:
            //    break;
            //case ItemID.MaxRemove:
            //    break;
            //case ItemID.MaxCut:
            //    break;
            //case ItemID.BetterCut:
            //    break;
            //case ItemID.BetterPeek:
            //    break;
            //case ItemID.Block:
            //    break;
            //case ItemID.DeathDefy:
            //    break;

            default:
                break;
        }
    }

}
public class ItemPrizes
{
    public static int GetItemPrice(ItemID item)
    {
        switch (item)
        {
            case ItemID.Clue:
                return 100;


            case ItemID.Remove:
                return 100;
            //case ItemID.Cut:
            //    break;
            //case ItemID.Peek:
            //    break;

            //// Upgrades
            //case ItemID.ExtraLife:
            //    break;
            //case ItemID.ProtectedLife:
            //    break;
            //case ItemID.MaxClue:
            //    break;
            //case ItemID.BetterClue:
            //    break;
            //case ItemID.MaxRemove:
            //    break;
            //case ItemID.MaxCut:
            //    break;
            //case ItemID.BetterCut:
            //    break;
            //case ItemID.BetterPeek:
            //    break;
            //case ItemID.Block:
            //    break;
            //case ItemID.DeathDefy:
            //    break;

            default:
                return 100;
        }

    }
}
[System.Serializable]
public class Consumable
{
    private int min = 0;
    [SerializeField] private ItemID itemID;
    [SerializeField] private int max;
    [Space]
    [SerializeField] private int current;

    public ItemID ItemID => itemID; 
    public int Amount => current;
    public int ID => (int)itemID;
    public int MAX => max;
    public static Consumable GetConsumable(ItemID itemID)
    {
        Consumable consumable = new Consumable();
        switch (itemID)
        {
            case ItemID.Clue:
                consumable = new Consumable()
                {
                    itemID = itemID,
                    current = 0,
                    max = 1,
                    min = 0,
                };
                break;
            case ItemID.Remove:
                consumable = new Consumable()
                {
                    itemID = itemID,
                    current = 0,
                    max = 1,
                    min = 0,
                };
                break;
            case ItemID.Cut:
                consumable = new Consumable()
                {
                    itemID = itemID,
                    current = 0,
                    max = 1,
                    min = 0,
                };
                break;
            case ItemID.Peek:
                consumable = new Consumable()
                {
                    itemID = itemID,
                    current = 0,
                    max = 1,
                    min = 0,
                };
                break;

            // Upgrades
            case ItemID.ExtraLife:
                consumable = new Consumable()
                {
                    itemID = itemID,
                    current = 0,
                    max = 1,
                    min = 0,
                };
                break;
            case ItemID.ProtectedLife:
                consumable = new Consumable()
                {
                    itemID = itemID,
                    current = 0,
                    max = 1,
                    min = 0,
                };
                break;
            case ItemID.MaxClue:
                consumable = new Consumable()
                {
                    itemID = itemID,
                    current = 0,
                    max = 1,
                    min = 0,
                };
                break;
            case ItemID.BetterClue:
                consumable = new Consumable()
                {
                    itemID = itemID,
                    current = 0,
                    max = 1,
                    min = 0,
                };
                break;
            case ItemID.MaxRemove:
                consumable = new Consumable()
                {
                    itemID = itemID,
                    current = 0,
                    max = 1,
                    min = 0,
                };
                break;
            case ItemID.MaxCut:
                consumable = new Consumable()
                {
                    itemID = itemID,
                    current = 0,
                    max = 1,
                    min = 0,
                };
                break;
            case ItemID.BetterCut:
                consumable = new Consumable()
                {
                    itemID = itemID,
                    current = 0,
                    max = 1,
                    min = 0,
                };
                break;
            case ItemID.BetterPeek:
                consumable = new Consumable()
                {
                    itemID = itemID,
                    current = 0,
                    max = 1,
                    min = 0,
                };
                break;
            case ItemID.Block:
                consumable = new Consumable()
                {
                    itemID = itemID,
                    current = 0,
                    max = 1,
                    min = 0,
                };
                break;
            case ItemID.DeathDefy:
                consumable = new Consumable()
                {
                    itemID = itemID,
                    current = 0,
                    max = 1,
                    min = 0,
                };
                break;

            default:
                consumable = new Consumable()
                {
                    itemID = ItemID.NONE,
                    current = 0,
                    max = 1,
                    min = 0,
                };
                break;
        }
        return consumable;
    }

    public void AddCurrent(int value)
    {
        current += value;
    }
}
[System.Serializable]
public class Upgrade
{
    [SerializeField] private ItemID itemId;
    [SerializeField] private int upgradeMaxLevel;
    [SerializeField] private int valueAddToInitial;
    [SerializeField] private int valueAddToMax;
    [Space]
    [SerializeField] private int upgradeCurrentLevel;
    public ItemID ItemID => itemId; 
    public int ID => (int)itemId; 
    public void LevelUp()
    {
        upgradeCurrentLevel++;
        upgradeCurrentLevel = Mathf.Clamp(upgradeCurrentLevel, 0, upgradeMaxLevel);
    }
    public int GetAdditionalMax ()
    {
        return valueAddToMax * upgradeCurrentLevel;
    }
    public int GetAdditionalItem()
    {
        return valueAddToInitial * upgradeCurrentLevel;
    }


    public static Upgrade GetUpgrade(ItemID itemId)
    {
        Upgrade upgrade = new Upgrade();
        switch (itemId)
        {
            case ItemID.Clue:
                upgrade = new Upgrade()
                {
                    itemId = itemId,
                    valueAddToInitial = 1,
                    valueAddToMax = 2,
                    upgradeMaxLevel = 3,
                    upgradeCurrentLevel = 0
                };
                break;
            case ItemID.Remove:
                upgrade = new Upgrade()
                {
                    itemId = itemId,
                    valueAddToInitial = 1,
                    valueAddToMax = 2,
                    upgradeMaxLevel = 3,
                    upgradeCurrentLevel = 0
                };
                break;
            case ItemID.Cut:
                upgrade = new Upgrade()
                {
                    itemId = itemId,
                    valueAddToInitial = 1,
                    valueAddToMax = 2,
                    upgradeMaxLevel = 3,
                    upgradeCurrentLevel = 0
                };
                break;
            case ItemID.Peek:
                upgrade = new Upgrade()
                {
                    itemId = itemId,
                    valueAddToInitial = 1,
                    valueAddToMax = 2,
                    upgradeMaxLevel = 3,
                    upgradeCurrentLevel = 0
                };
                break;

            // Upgrades
            case ItemID.ExtraLife:
                upgrade = new Upgrade()
                {
                    itemId = itemId,
                    valueAddToInitial = 1,
                    valueAddToMax = 2,
                    upgradeMaxLevel = 3,
                    upgradeCurrentLevel = 0
                };
                break;
            case ItemID.ProtectedLife:
                upgrade = new Upgrade()
                {
                    itemId = itemId,
                    valueAddToInitial = 1,
                    valueAddToMax = 2,
                    upgradeMaxLevel = 3,
                    upgradeCurrentLevel = 0
                };
                break;
            case ItemID.MaxClue:
                upgrade = new Upgrade()
                {
                    itemId = itemId,
                    valueAddToInitial = 1,
                    valueAddToMax = 2,
                    upgradeMaxLevel = 3,
                    upgradeCurrentLevel = 0
                };
                break;
            case ItemID.BetterClue:
                upgrade = new Upgrade()
                {
                    itemId = itemId,
                    valueAddToInitial = 1,
                    valueAddToMax = 2,
                    upgradeMaxLevel = 3,
                    upgradeCurrentLevel = 0
                };
                break;
            case ItemID.MaxRemove:
                upgrade = new Upgrade()
                {
                    itemId = itemId,
                    valueAddToInitial = 1,
                    valueAddToMax = 2,
                    upgradeMaxLevel = 3,
                    upgradeCurrentLevel = 0
                };
                break;
            case ItemID.MaxCut:
                upgrade = new Upgrade()
                {
                    itemId = itemId,
                    valueAddToInitial = 1,
                    valueAddToMax = 2,
                    upgradeMaxLevel = 3,
                    upgradeCurrentLevel = 0
                };
                break;
            case ItemID.BetterCut:
                upgrade = new Upgrade()
                {
                    itemId = itemId,
                    valueAddToInitial = 1,
                    valueAddToMax = 2,
                    upgradeMaxLevel = 3,
                    upgradeCurrentLevel = 0
                };
                break;
            case ItemID.BetterPeek:
                upgrade = new Upgrade()
                {
                    itemId = itemId,
                    valueAddToInitial = 1,
                    valueAddToMax = 2,
                    upgradeMaxLevel = 3,
                    upgradeCurrentLevel = 0
                };
                break;
            case ItemID.Block:
                upgrade = new Upgrade()
                {
                    itemId = itemId,
                    valueAddToInitial = 1,
                    valueAddToMax = 2,
                    upgradeMaxLevel = 3,
                    upgradeCurrentLevel = 0
                };
                break;
            case ItemID.DeathDefy:
                upgrade = new Upgrade()
                {
                    itemId = itemId,
                    valueAddToInitial = 1,
                    valueAddToMax = 2,
                    upgradeMaxLevel = 3,
                    upgradeCurrentLevel = 0
                };
                break;

            default:
                upgrade = new Upgrade()
                {
                    itemId = ItemID.NONE,
                    valueAddToInitial = 0,
                    valueAddToMax = 0,
                    upgradeMaxLevel = 0,
                    upgradeCurrentLevel = 0
                };
                break;
        }
        return upgrade;
    }
}