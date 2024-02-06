using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

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
    LifeProtector,
    ExtraClue,
    BetterClue,

    ExtraRemove,
    ExtraCut,
    BetterCut,
    ExtraPeek,
    BlockChoise,

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
                    description = "CLUE: Guess the current sticker.",
                    max = 1,
                };
                break;
            case ConsumableID.Remove:
                consumable = new ConsumableData()
                {
                    itemID = itemID,
                    price = 300,
                    description = "REMOVE: Remove the current Sticker.",
                    max = 1,
                };
                break;
            case ConsumableID.Cut:
                consumable = new ConsumableData()
                {
                    itemID = itemID,
                    price = 200,
                    description = "CUT: Reduce the number of options.",
                    max = 1,
                };
                break;
            case ConsumableID.Peek:
                consumable = new ConsumableData()
                {
                    itemID = itemID,
                    price= 200,
                    description = "PEEK: See how many times appears each sticker",
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
    public int GetMaxLevel()
    {
        return levelPrices.Length;
    }
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
        //Debug.Log(itemId.ToString()+ " IsMaxLevel: " + currentLevel + " >= " + (levelPrices.Length).ToString());
        return currentLevel >= levelPrices.Length;
    }
    public static UpgradeData GetUpgrade(UpgradeID itemId)
    {
        UpgradeData upgrade = new UpgradeData();
        switch (itemId)
        {
            case UpgradeID.ExtraClue:
                upgrade = new UpgradeData()
                {
                    itemId = itemId,
                    valueAddToInitial = 1,
                    valueAddToMax = 2,
                    description = "EXTRA CLUE: ADD extra MAX and ITEM to the match.",
                    levelPrices = new int[] { 200, 400, 1200 },
                };
                break;
            case UpgradeID.ExtraRemove:
                upgrade = new UpgradeData()
                {
                    itemId = itemId,
                    valueAddToInitial = 1,
                    valueAddToMax = 2,
                    description = "EXTRA REMOVE: ADD extra MAX and ITEM to the match.",
                    levelPrices = new int[] { 2500 },
                };
                break;
            case UpgradeID.ExtraCut:
                upgrade = new UpgradeData()
                {
                    itemId = itemId,
                    valueAddToInitial = 1,
                    valueAddToMax = 2,
                    description = "EXTRA CUT: ADD extra MAX and ITEM to the match.",
                    levelPrices = new int[] { 300, 700, 1500 },

                };
                break;

            // Upgrades
            case UpgradeID.ExtraLife:
                upgrade = new UpgradeData()
                {
                    itemId = itemId,
                    valueAddToInitial = 1,
                    valueAddToMax = 0,
                    description = "EXTRA LIFE: Increase the MAX capacity of lifes.",
                    levelPrices = new int[] { 1000,2000,3000},
                };
                break;
            case UpgradeID.LifeProtector:
                upgrade = new UpgradeData()
                {
                    itemId = itemId,
                    valueAddToInitial = 1,
                    valueAddToMax = 0,
                    description = "LIFE PROTECTOR: Protect your life from one mistake.",
                    levelPrices = new int[] {2500},
                    upgradeRequired = new Dictionary<UpgradeID, int>()
                    {
                        { UpgradeID.ExtraLife, 2 }
                    }
                };
                break;
            case UpgradeID.BetterClue:
                upgrade = new UpgradeData()
                {
                    itemId = itemId,
                    valueAddToInitial = 1,
                    valueAddToMax = 0,
                    description = "BETTER CLUE: Upgrade CLUE behaviour.",
                    levelPrices = new int[] { 1500 },
                    upgradeRequired = new Dictionary<UpgradeID, int>()
                    {
                        { UpgradeID.ExtraClue, 1 }
                    }
                };
                break;
            case UpgradeID.BetterCut:
                upgrade = new UpgradeData()
                {
                    levelPrices = new int[] { 3000 },
                };
                break;
            case UpgradeID.ExtraPeek:
                upgrade = new UpgradeData()
                {
                    levelPrices = new int[] { 3000 },
                };
                break;
            case UpgradeID.BlockChoise:
                upgrade = new UpgradeData()
                {
                    itemId = itemId,
                    valueAddToInitial = 1,
                    valueAddToMax = 0,
                    description = "BLOCK MISTAKE: BLOCK the miss for next time.",
                    levelPrices = new int[] { 3000 },
                };
                break;
            case UpgradeID.DeathDefy:
                upgrade = new UpgradeData()
                {
                    itemId = itemId,
                    valueAddToInitial = 1,
                    valueAddToMax = 0,
                    description = "DEATH DEFY: Probability of surviving a near miss.",
                    levelPrices = new int[] { 3000 },
                    upgradeRequired = new Dictionary<UpgradeID, int>()
                    {
                        { UpgradeID.LifeProtector, 1 }
                    }
                };
                break;

            default:
                upgrade = new UpgradeData()
                {
                    levelPrices = new int[] { 3000 },
                };
                break;
        }
        return upgrade;
    }
}
