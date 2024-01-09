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
    BetterClue,
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
    MaxPeek,
    Block,
    DeathDefy
}


[System.Serializable]
public class ConsumableData
{
    public ConsumableID itemID;
    public int max;
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
                    price = 100,
                    description = "Guess current Sticker",
                    max = 1,
                };
                break;
            case ConsumableID.Remove:
                consumable = new ConsumableData()
                {
                    itemID = itemID,
                    price = 300,
                    description = "Remove current Sticker",
                    max = 1,
                };
                break;
            case ConsumableID.Cut:
                consumable = new ConsumableData()
                {
                    itemID = itemID,
                    price = 200,
                    description = "Crop options",
                    max = 1,
                };
                break;
            case ConsumableID.Peek:
                consumable = new ConsumableData()
                {
                    itemID = itemID,
                    price= 200,
                    description = "See how many times appears each sticker",
                    max = 1,
                };
                break;
        }
        return consumable;
    }
}
[System.Serializable]
public class UpgradeData
{
    public UpgradeID itemId;
    public int valueAddToInitial;
    public int valueAddToMax;
    public string description;
    public int[] levelPrices = new int[] { 100, 200, 1500 };
    public Dictionary<UpgradeID, int> upgradeRequired = new Dictionary<UpgradeID, int>();
    public int ID => (int)itemId;

    public int GetAdditionalMax(int currentLevel)
    {
        return valueAddToMax * currentLevel;
    }
    public int GetAdditionalItem(int currentLevel)
    {
        return valueAddToInitial * currentLevel;
    }
    public int GetPrice(int currentLevel)
    {
        int index = Mathf.Clamp(currentLevel, 0, levelPrices.Length - 1);
        return levelPrices[index];
    }
    //public void LevelUp()
    //{
    //    currentLevel++;
    //    currentLevel = Mathf.Clamp(currentLevel, 0, levelPrices.Length - 1);
    //}
    public bool IsMaxLevel(int currentLevel)
    {
        return currentLevel >= levelPrices.Length - 1;
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
                    description = "Adds initial consumables at the start of the match and increases the maximum you can carry.",
                    levelPrices = new int[] { 200, 500, 1500 },
                };
                break;
            case UpgradeID.MaxRemove:
                upgrade = new UpgradeData()
                {
                    itemId = itemId,
                    valueAddToInitial = 1,
                    valueAddToMax = 2,
                    levelPrices = new int[] { 200, 500, 1500 },
                };
                break;
            case UpgradeID.MaxCut:
                upgrade = new UpgradeData()
                {
                    itemId = itemId,
                    valueAddToInitial = 1,
                    valueAddToMax = 2,

                    levelPrices = new int[] { 200, 500, 1500 },
                    upgradeRequired = new Dictionary<UpgradeID, int>()
                    {
                        { UpgradeID.MaxClue, 2 }
                    }
                };
                break;

            // Upgrades
            case UpgradeID.ExtraLife:
                upgrade = new UpgradeData()
                {
                    itemId = itemId,
                    valueAddToInitial = 1,
                    valueAddToMax = 0,

                    levelPrices = new int[] { 200},
                };
                break;
            case UpgradeID.ProtectedLife:
                upgrade = new UpgradeData()
                {
                    itemId = itemId,
                    valueAddToInitial = 1,
                    valueAddToMax = 0,

                    levelPrices = new int[] { 200},
                };
                break;
            case UpgradeID.BetterClue:
                upgrade = new UpgradeData()
                {
                    itemId = itemId,
                    valueAddToInitial = 1,
                    valueAddToMax = 0,

                    levelPrices = new int[] { 200 },
                };
                break;
            case UpgradeID.BetterCut:
                upgrade = new UpgradeData()
                {

                };
                break;
            case UpgradeID.MaxPeek:
                upgrade = new UpgradeData()
                {

                };
                break;
            case UpgradeID.Block:
                upgrade = new UpgradeData()
                {

                };
                break;
            case UpgradeID.DeathDefy:
                upgrade = new UpgradeData()
                {
                    itemId = itemId,
                    valueAddToInitial = 1,
                    valueAddToMax = 0,

                    levelPrices = new int[] { 200 },
                };
                break;

            default:
                upgrade = new UpgradeData()
                {

                };
                break;
        }
        return upgrade;
    }
}
