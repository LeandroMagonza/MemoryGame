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
                    description = "EXTRA CLUE\n",
                    levelPrices = new int[] { 200, 400, 1200 },
                };
                break;
            case UpgradeID.ExtraRemove:
                upgrade = new UpgradeData()
                {
                    itemId = itemId,
                    valueAddToInitial = 1,
                    valueAddToMax = 2,
                    description = "EXTRA REMOVE\n",
                    levelPrices = new int[] { 2500 },
                };
                break;
            case UpgradeID.ExtraCut:
                upgrade = new UpgradeData()
                {
                    itemId = itemId,
                    valueAddToInitial = 1,
                    valueAddToMax = 2,
                    description = "EXTRA CUT\n",
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
                    description = "EXTRA LIFE\n",
                    levelPrices = new int[] { 1000,2000,3000},
                };
                break;
            case UpgradeID.LifeProtector:
                upgrade = new UpgradeData()
                {
                    itemId = itemId,
                    valueAddToInitial = 1,
                    valueAddToMax = 0,
                    description = "LIFE PROTECTOR\n",
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
                    description = "BETTER CLUE\n",
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
                    description = "BLOCK CHOISE\n",
                    levelPrices = new int[] { 3000 },
                };
                break;
            case UpgradeID.DeathDefy:
                upgrade = new UpgradeData()
                {
                    itemId = itemId,
                    valueAddToInitial = 1,
                    valueAddToMax = 0,
                    description = "DEATH DEFY\n",
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
