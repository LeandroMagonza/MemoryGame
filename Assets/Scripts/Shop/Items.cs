using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
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


[System.Serializable]
public class ConsumableData
{
    public ConsumableID itemID;
    public int max;
    public int current;
    public string description;
    public int price;
    public int ID => (int)itemID;
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
                    price = 100,
                    description = "Guess current Sticker",
                    max = 1,
                };
                break;
            case ConsumableID.Remove:
                consumable = new ConsumableData()
                {
                    itemID = itemID,
                    current = 0,
                    price = 300,
                    description = "Remove current Sticker",
                    max = 1,
                };
                break;
            case ConsumableID.Cut:
                consumable = new ConsumableData()
                {
                    itemID = itemID,
                    current = 0,
                    price = 200,
                    description = "Crop options",
                    max = 1,
                };
                break;
            case ConsumableID.Peek:
                consumable = new ConsumableData()
                {
                    itemID = itemID,
                    current = 0,
                    price= 200,
                    description = "See how many times appears each sticker",
                    max = 1,
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
    public UpgradeID itemId;
    public int valueAddToInitial;
    public int valueAddToMax;
    public int currentLevel;
    public string description;
    public int[] levelPrizes = new int[] { 100, 200, 1500 };
    public Dictionary<UpgradeID, int> upgradeRequired = new Dictionary<UpgradeID, int>();
    public int ID => (int)itemId;

    public int GetAdditionalMax()
    {
        return valueAddToMax * currentLevel;
    }
    public int GetAdditionalItem()
    {
        return valueAddToInitial * currentLevel;
    }
    public int GetPrice()
    {
        int index = Mathf.Clamp(currentLevel, 0, levelPrizes.Length - 1);
        return levelPrizes[index];
    }
    public void LevelUp()
    {
        currentLevel++;
        currentLevel = Mathf.Clamp(currentLevel, 0, levelPrizes.Length - 1);
    }
    public bool IsMaxLevel()
    {
        return currentLevel >= levelPrizes.Length - 1;
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
                    currentLevel = 0,
                };
                break;
            case UpgradeID.MaxRemove:
                upgrade = new UpgradeData()
                {
                    itemId = itemId,
                    valueAddToInitial = 1,
                    valueAddToMax = 2,
                    currentLevel = 0,
                };
                break;
            case UpgradeID.MaxCut:
                upgrade = new UpgradeData()
                {
                    itemId = itemId,
                    valueAddToInitial = 1,
                    valueAddToMax = 2,
                    currentLevel = 0,
                };
                break;

            // Upgrades
            case UpgradeID.ExtraLife:
                upgrade = new UpgradeData()
                {
                    currentLevel = 0
                };
                break;
            case UpgradeID.ProtectedLife:
                upgrade = new UpgradeData()
                {
                    currentLevel = 0
                };
                break;
            case UpgradeID.BetterClue:
                upgrade = new UpgradeData()
                {
                    currentLevel = 0
                };
                break;
            case UpgradeID.BetterCut:
                upgrade = new UpgradeData()
                {
                    currentLevel = 0
                };
                break;
            case UpgradeID.BetterPeek:
                upgrade = new UpgradeData()
                {
                    currentLevel = 0
                };
                break;
            case UpgradeID.Block:
                upgrade = new UpgradeData()
                {
                    currentLevel = 0
                };
                break;
            case UpgradeID.DeathDefy:
                upgrade = new UpgradeData()
                {
                    currentLevel = 0
                };
                break;

            default:
                upgrade = new UpgradeData()
                {
                    currentLevel = 0
                };
                break;
        }
        return upgrade;
    }
}
