using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public enum ConsumableID
{
    NONE = -1,
    Clue,
    Remove,
    Cut,
    Peek,
    Highlight,
    Shotgun
}
public enum UpgradeID
{
    NONE=-1,
    ExtraLife,
    LifeProtector,
    ExtraClue,
    BetterClue,
    HealOnClear,
    ExtraRemove,
    ExtraCut,
    BetterCut,
    ExtraPeek,
    BlockMistake,
    DeathDefy,
    ExtraHighlight,
    ExtraShotgun,
    StickerMaster,
    ConsumableSlot,
}


[System.Serializable] 
public class ConsumableData
{
    public ConsumableID itemID;
    [FormerlySerializedAs("max")] public int amount;
    public string name;
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
                    price = 8,
                    name = "CLUE",
                    description = "Correctly guess the current sticker.",
                    amount = 1,
                };
                break;
            case ConsumableID.Remove:
                consumable = new ConsumableData()
                {
                    itemID = itemID,
                    price = 20,
                    name = "REMOVE",
                    description = "REMOVE the current Sticker from the match.",
                    amount = 1,
                };
                break;
            case ConsumableID.Cut:
                consumable = new ConsumableData()
                {
                    itemID = itemID,
                    price = 3,
                    name = "CUT",
                    description = "Block one incorrect option per level.",
                    amount = 1,
                };
                break;
            case ConsumableID.Peek:
                consumable = new ConsumableData()
                {
                    itemID = itemID,
                    price= 30,
                    name = "PEEK",
                    description = "See how many times each Sticker hast appeared",
                    amount = 1,
                };
                break;  
            case ConsumableID.Highlight:
                consumable = new ConsumableData()
                {
                    itemID = itemID,
                    price= 3,
                    name = "Highlight",
                    description = "If the next guess is correct, reduce Sticker options for the match.",
                    amount = 1,
                };
                break;
            case ConsumableID.Shotgun:
                consumable = new ConsumableData()
                {
                    itemID = itemID,
                    price= 5,
                    name = "Shotgun",
                    description = "If the next guess is a Small Mistake, it will count as correct.",
                    amount = 1,
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
    public int valuePerLevel;
    public int baseValue;
    public string name;
    public string description;
    public Color backgroundColor;
    public int[] levelPrices = { 100, 200, 1500 };
    public int[] userLevelRequired = new []{1};
    public Dictionary<UpgradeID, int> upgradeRequired = new();
    public int ID => (int)itemId;
    public int GetMaxLevel()
    {
        return userLevelRequired.Length;
    }
    public int GetValue(int currentLevel)
    {
        return valuePerLevel * currentLevel + baseValue;
    }
    public int GetPrice(int currentLevel)
    {
        int index = Mathf.Clamp(currentLevel, 0, levelPrices.Length - 1);
        return levelPrices[index];
    }

    public bool IsMaxLevel(int currentLevel)
    {
        return currentLevel >= userLevelRequired.Length;
    }
    public static UpgradeData GetUpgrade(UpgradeID itemId)
    {
        UpgradeData upgrade;
        switch (itemId)
        {
            case UpgradeID.ExtraClue:
                upgrade = new UpgradeData()
                {
                    itemId = itemId,
                    valuePerLevel = 1,
                    name = "EXTRA CLUE",
                    description = "Start each match with an extra Clue. \n"+ConsumableData.GetConsumable(ConsumableID.Clue).description,
                    backgroundColor = Color.blue,
                    levelPrices = new [] { 1000, 2000, 3000 },
                    userLevelRequired = new []{ 3,10,15 }
                };
                break;
            case UpgradeID.ExtraRemove:
                upgrade = new UpgradeData()
                {
                    itemId = itemId,
                    valuePerLevel = 1,
                    name = "EXTRA REMOVE",
                    description = "Start each match with an extra Remove. \n" +ConsumableData.GetConsumable(ConsumableID.Remove).description,
                    backgroundColor = Color.green,
                    userLevelRequired = new int[] { 7 },
                };
                break;
            case UpgradeID.ExtraCut:
                upgrade = new UpgradeData()
                {
                    itemId = itemId,
                    valuePerLevel = 1,
                    name = "EXTRA CUT",
                    description = "Start each match with an extra Cut. \n" +ConsumableData.GetConsumable(ConsumableID.Cut).description,
                    backgroundColor = Color.yellow,
                    userLevelRequired = new int[] { 1, 2, 6, 10 },
                };
                break;
            case UpgradeID.ExtraPeek:
                upgrade = new UpgradeData()
                {
                    itemId = itemId,
                    valuePerLevel = 1,
                    name = "EXTRA PEEK",
                    description = "Start each match with an extra Peek. \n" +ConsumableData.GetConsumable(ConsumableID.Peek).description,
                    backgroundColor = Color.magenta,
                    userLevelRequired = new int[] { 15 },

                };
                break;    
            case UpgradeID.ExtraHighlight:
                upgrade = new UpgradeData()
                {
                    itemId = itemId,
                    valuePerLevel = 1,
                    name = "EXTRA HIGHLIGHT",
                    description = "Start each match with an extra Highlight. \n" +ConsumableData.GetConsumable(ConsumableID.Highlight).description,
                    backgroundColor = Color.green,
                    levelPrices = new int[] { 1,3,6,10 },

                };
                break;     
            case UpgradeID.ExtraShotgun:
                upgrade = new UpgradeData()
                {
                    itemId = itemId,
                    valuePerLevel = 1,
                    name = "EXTRA SHOTGUN",
                    description = "Start each match with an extra Shotgun. \n" +ConsumableData.GetConsumable(ConsumableID.Shotgun).description,
                    backgroundColor = Color.cyan,
                    userLevelRequired = new int[] { 1,6 },
                };
                break;
            
            // Upgrades
            case UpgradeID.ExtraLife:
                upgrade = new UpgradeData()
                {
                    itemId = itemId,
                    valuePerLevel = 1,
                    name = "EXTRA LIFE",
                    description = "Increase the amount of lives you have.",
                    backgroundColor = Color.red,
                    userLevelRequired = new int[] { 1,5,10,15},
                };
                break;
            case UpgradeID.LifeProtector:
                upgrade = new UpgradeData()
                {
                    itemId = itemId,
                    valuePerLevel = 1,
                    name = "LIFE PROTECTOR",
                    description = "Protect your life from one mistake. Recharges on 10 Combo - lv",
                    backgroundColor = Color.red,
                    userLevelRequired = new int[] {10,12,14,16,18},
                };
                break;
            case UpgradeID.BetterClue:
                upgrade = new UpgradeData()
                {
                    itemId = itemId,
                    valuePerLevel = 1,
                    name = "BETTER CLUE",
                    description = "Clues mark the number they were used on.",
                    backgroundColor = Color.blue,
                    userLevelRequired = new int[] { 10 },
                    upgradeRequired = new Dictionary<UpgradeID, int>()
                    {
                        { UpgradeID.ExtraClue, 1 }
                    }
                };
                break;
            case UpgradeID.BetterCut:
                upgrade = new UpgradeData()
                {
                    itemId = itemId,
                    valuePerLevel = 1,
                    name = "BETTER CUT",
                    description = "Cut blocks one extra option per level.",
                    backgroundColor = Color.blue,
                    userLevelRequired = new int[] { 6,10,14,18,22,26,30,34 },
                    upgradeRequired = new Dictionary<UpgradeID, int>()
                    {
                        { UpgradeID.ExtraCut, 1 }
                    }
                };
                break;
          
            case UpgradeID.BlockMistake:
                upgrade = new UpgradeData()
                {
                    itemId = itemId,
                    valuePerLevel = 1,
                    name = "BLOCK MISTAKE",
                    description = "On mistake, add a Cut lv 1 to the sticker.",
                    backgroundColor = Color.yellow,
                    userLevelRequired = new int[] { 10,14,18 },
                };
                break;
            case UpgradeID.DeathDefy:
                upgrade = new UpgradeData()
                {
                    itemId = itemId,
                    valuePerLevel = 1,
                    name = "DEATH DEFY",
                    description = "Cheat death on a SMALL mistake once per lv",
                    backgroundColor = Color.red,
                    userLevelRequired = new int[] { 15,18,20 },
                };
                break;           
            case UpgradeID.HealOnClear:
                upgrade = new UpgradeData()
                {
                    itemId = itemId,
                    valuePerLevel = 1,
                    name = "HEAL ON CLEAR",
                    description = "Heal one heart per 4 - lv stickers cleared",
                    backgroundColor = Color.red,
                    userLevelRequired = new int[] { 0,4,8,12 },

                };
                break;            
            case UpgradeID.StickerMaster:
                upgrade = new UpgradeData()
                {
                    itemId = itemId,
                    valuePerLevel = 1,
                    name = "STICKER MASTER",
                    description = "Stickers on match come with bonuses, per sticker level",
                    backgroundColor = Color.red,
                    userLevelRequired = new int[] { 25 },
                };
                break;
            case UpgradeID.ConsumableSlot:
                upgrade = new UpgradeData()
                {
                    itemId = itemId,
                    valuePerLevel = 1,
                    name = "CONSUMABLE SLOT",
                    description = "Allows you to bring (level) consumables ",
                    backgroundColor = Color.red,
                    userLevelRequired = new int[] { 5,10 },
                };
                break;

            default:
                throw new Exception("Upgrade ID not found");
                break;
        }
        return upgrade;
    }
}
