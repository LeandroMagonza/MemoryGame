using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public enum ConsumableID
{
    NONE = -1,
    Clue,
    Remove,
    Cut,
    Peek,
}
public enum UpgradeID
{
    NONE=-1,
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
    public Dictionary<ConsumableID, ConsumableData> consumables = new Dictionary<ConsumableID, ConsumableData>();
    public Dictionary<UpgradeID, UpgradeData> upgrades = new Dictionary<UpgradeID, UpgradeData>();
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
    private void Awake()
    {
        Instance = this;
    }
   
    public void ProcessAddConsumable(ConsumableID consumableID)
    {
        if ((int)consumableID < 0) return;
        Debug.Log("Added Item:" + nameof(consumableID));
        AddConsumableObject(consumableID);
        ValidateItem(consumableID);
    }
    public void ProcessAddUpgrade(UpgradeID upgradeID)
    {
        if ((int)upgradeID < 0) return;
        Debug.Log("Added Item:" + nameof(upgradeID));
       
        
    }
    private void AddUpgradeObject(UpgradeID upgradeID)
    {
        if (upgrades.ContainsKey(upgradeID))
        {
            upgrades[upgradeID].LevelUp();
        }
        else
        {
            //upgrades.Add(upgradeID,updateda);
        }
    }
    private void AddConsumableObject(ConsumableID consumableID)
    {
        if (consumables.ContainsKey(consumableID))
        {
            consumables[consumableID].AddCurrent(1);
        }
        else
        {
            consumables.Add(consumableID, ConsumableData.GetConsumable(consumableID));
        }
    }
    public Dictionary<ConsumableData, int> MatchInventory()
    {
        Dictionary<ConsumableData, int> temp_inventory = new Dictionary<ConsumableData, int>();
        //foreach (UpgradeData upgrade in upgrades)
        //{
        //    //temp_inventory.Add(upgrade.ItemID, upgrade.GetAdditionalItem());
        //}
        //foreach (ConsumableData item in consumables)
        //{
        //    if (temp_inventory.ContainsKey(item.ItemID))
        //    {
        //        temp_inventory[item.ItemID] += item.Amount;
        //    }
        //    else
        //    {
        //        temp_inventory.Add(item.ItemID, item.Amount);
        //    }
        //}
        return temp_inventory;
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
   
    public void ValidateItem(ConsumableID itemId)
    {
        SetInteractable();
        SetButtonText();
    }
    private void SetButtonText()
    {
        if (buttonTextClue != null)
            buttonTextClue.text = consumables[ConsumableID.Clue].Amount.ToString();
        if (buttonTextPeek != null)
            buttonTextPeek.text = consumables[ConsumableID.Peek].Amount.ToString();
        if (buttonTextCut != null)
            buttonTextCut.text = consumables[ConsumableID.Cut].Amount.ToString();
        if (buttonTextRemove != null)
            buttonTextRemove.text = consumables[ConsumableID.Remove].Amount.ToString();
    }
    private void SetInteractable()
    {
        if (buttonClue != null)
            buttonClue.interactable = consumables[ConsumableID.Clue].Amount > 0;
        if (buttonRemove != null)
            buttonRemove.interactable = consumables[ConsumableID.Remove].Amount > 0;
        if (buttonCut != null)
            buttonCut.interactable = consumables[ConsumableID.Cut].Amount > 0;
        if (buttonPeek != null)
            buttonPeek.interactable = consumables[ConsumableID.Peek].Amount > 0;
    }
    public void UseItem(ConsumableID itemId)
    {
        switch (itemId)
        {
            case ConsumableID.Clue:
                UseClue();
                break;
            case ConsumableID.Remove:
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
    public static int GetItemPrice(ConsumableID item)
    {
        switch (item)
        {
            case ConsumableID.Clue:
                return 100;


            case ConsumableID.Remove:
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
public class ConsumableData
{
    private int min = 0;
    [SerializeField] private ConsumableID itemID;
    [SerializeField] private int max;
    [Space]
    [SerializeField] private int current;

    public ConsumableID ItemID => itemID; 
    public int Amount => current;
    public int ID => (int)itemID;
    public int MAX => max;
    public static ConsumableData GetConsumable(ConsumableID itemID)
    {
        ConsumableData consumable = new ConsumableData();
        switch (itemID)
        {
            case ConsumableID.Clue:
                consumable = new ConsumableData()
                {
                    itemID = itemID,
                    current = 0,
                    max = 1,
                    min = 0,
                };
                break;
            case ConsumableID.Remove:
                consumable = new ConsumableData()
                {
                    itemID = itemID,
                    current = 0,
                    max = 1,
                    min = 0,
                };
                break;
            case ConsumableID.Cut:
                consumable = new ConsumableData()
                {
                    itemID = itemID,
                    current = 0,
                    max = 1,
                    min = 0,
                };
                break;
            case ConsumableID.Peek:
                consumable = new ConsumableData()
                {
                    itemID = itemID,
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
    public bool ModifyCurrent(int value)
    {
        int newValue = current + value;
        if (newValue < 0)
        {
            return false;
        }
        else if (newValue > max)
        {
            current = max;
            return true;
        }
        else
        {
            current += value;
            return true;
        }
    }
}
[System.Serializable]
public class UpgradeData
{
    [SerializeField] protected UpgradeID itemId;
    [SerializeField] protected int valueAddToInitial;
    [SerializeField] protected int valueAddToMax;
    [SerializeField] protected int upgradeCurrentLevel;
    [Space]
    [SerializeField] protected int[] levelPrizes = new int[] {100,200,1500};
    [SerializeField] protected List<UpgradeData> nextUpgrades;
    public UpgradeID ItemID => itemId;
    public int ID => (int)itemId;

    public int GetAdditionalMax()
    {
        return valueAddToMax * upgradeCurrentLevel;
    }
    public int GetAdditionalItem()
    {
        return valueAddToInitial * upgradeCurrentLevel;
    }
    public int GetUpdateCurrentPrice()
    {
        int index = Mathf.Clamp(upgradeCurrentLevel,0, levelPrizes.Length - 1);
        return levelPrizes[index]; 
    }
    public void LevelUp()
    {
        upgradeCurrentLevel++;
        upgradeCurrentLevel = Mathf.Clamp(upgradeCurrentLevel, 0, levelPrizes.Length-1);
    }

    public static UpgradeData GetUpgrade(UpgradeID itemId)
    {
        UpgradeData upgrade = new UpgradeData();
        switch (itemId)
        {
            case UpgradeID.MaxClue:
                upgrade = new UpgradeData()
                {
                    itemId = itemId,
                    valueAddToInitial = 1,
                    valueAddToMax = 2,
                    upgradeCurrentLevel = 0
                };
                break;
            case UpgradeID.MaxRemove:
                upgrade = new UpgradeData()
                {
                    itemId = itemId,
                    valueAddToInitial = 1,
                    valueAddToMax = 2,
                    upgradeCurrentLevel = 0
                };
                break;
            case UpgradeID.MaxCut:
                upgrade = new UpgradeData()
                {
                    itemId = itemId,
                    valueAddToInitial = 1,
                    valueAddToMax = 2,
                    upgradeCurrentLevel = 0
                };
                break;

            // Upgrades
            case UpgradeID.ExtraLife:
                upgrade = new UpgradeData()
                {
                    upgradeCurrentLevel = 0
                };
                break;
            case UpgradeID.ProtectedLife:
                upgrade = new UpgradeData()
                {
                    upgradeCurrentLevel = 0
                };
                break;
            case UpgradeID.BetterClue:
                upgrade = new UpgradeData()
                {
                    upgradeCurrentLevel = 0
                };
                break;
            case UpgradeID.BetterCut:
                upgrade = new UpgradeData()
                {
                    upgradeCurrentLevel = 0
                };
                break;
            case UpgradeID.BetterPeek:
                upgrade = new UpgradeData()
                {
                    upgradeCurrentLevel = 0
                };
                break;
            case UpgradeID.Block:
                upgrade = new UpgradeData()
                {
                    upgradeCurrentLevel = 0
                };
                break;
            case UpgradeID.DeathDefy:
                upgrade = new UpgradeData()
                {
                    upgradeCurrentLevel = 0
                };
                break;

            default:
                upgrade = new UpgradeData()
                {
                    upgradeCurrentLevel = 0
                };
                break;
        }
        return upgrade;
    }
}
