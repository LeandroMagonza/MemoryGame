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
    Bomb,
    EnergyPotion
}
public enum UpgradeID
{
    NONE=-1,
    FactoryClue,
    FactoryRemove,
    FactoryPeek,
    FactoryHighlight,
    FactoryBomb,
    
    ExtraLife,
    LifeProtector,
    BetterClue,
    HealOnClear,
    FactoryCut,
    BetterCut,
    BlockMistake,
    DeathDefy,
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
    public int generationMinutes;
    public int initialStorage;
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
                    generationMinutes = 180,
                    initialStorage =  1
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
                    generationMinutes = 480,
                    initialStorage =  0
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
                    generationMinutes = 60,
                    initialStorage =  2
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
                    generationMinutes = 480,
                    initialStorage =  0
                };
                break;  
            case ConsumableID.Highlight:
                consumable = new ConsumableData()
                {
                    itemID = itemID,
                    price= 3,
                    name = "HIGHLIGHT",
                    description = "If the next guess is correct, reduce Sticker options for the match.",
                    amount = 1,
                    generationMinutes = 60,
                    initialStorage =  2
                };
                break;
            case ConsumableID.Bomb:
                consumable = new ConsumableData()
                {
                    itemID = itemID,
                    price= 5,
                    name = "BOMB",
                    description = "If the next guess is a Small Mistake, it will count as correct.",
                    amount = 1,
                    generationMinutes = 120,
                    initialStorage =  1
                };
                break;  
            case ConsumableID.EnergyPotion:
                consumable = new ConsumableData()
                {
                    itemID = itemID,
                    price= 5,
                    name = "Energy Potion",
                    description = "Recharges your energy to the MAX!",
                    amount = 1,
                    generationMinutes = 480,
                    initialStorage = 10,
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
    public int[] playerLevelRequired = new []{1};
    public Dictionary<UpgradeID, int> upgradeRequired = new();
    public IconName iconMain;
    public IconName iconSecondary;
    public int ID => (int)itemId;
    public int GetMaxLevel()
    {
        return playerLevelRequired.Length;
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
        return currentLevel >= playerLevelRequired.Length;
    }
    public static UpgradeData GetUpgrade(UpgradeID upgradeID, bool nextLevel = false)
    {
        UpgradeData upgrade;
        int upgradeLevel = PersistanceManager.Instance.userData.GetUpgradeLevel(upgradeID);
        if (nextLevel) { upgradeLevel++; }
        
        ConsumableData correspondingConsumable = ConsumableData.GetConsumable(
            ItemHelper.GetCorrespondingConsumable(upgradeID));

        string generationTimeText = "";
        string storageText = "";
        string fullFactoryDescription = "";

        string textHighlighColor;
        if (nextLevel) {
            textHighlighColor = 	"#00ff00ff"; //lime
        }
        else {
            textHighlighColor = 	"#00ffffff"; //cyan
        }
        if (correspondingConsumable.itemID != ConsumableID.NONE) {
            //CustomDebugger.Log("nextLevel: "+nextLevel+" upgradeLevel:"+upgradeLevel);
            generationTimeText = "<color="+textHighlighColor+">"+ItemHelper.FormatGenerationTime(
            correspondingConsumable.generationMinutes,
            upgradeLevel)+"</color>";
            var totalStorageAmount = (ConsumableData.GetConsumable(
                ItemHelper.GetCorrespondingConsumable(upgradeID)).initialStorage + upgradeLevel);
            
            storageText = "<color="+textHighlighColor+">"+totalStorageAmount +"</color> "+correspondingConsumable.name+ ((totalStorageAmount == 1) ? "" : "S");


            fullFactoryDescription = "Generates a " + correspondingConsumable.name + " every " + generationTimeText +
                                     ", stores " + storageText + ". \n"+ 
                                     "<color=white>"+correspondingConsumable.name+"</color>: "+correspondingConsumable.description;
        }
        switch (upgradeID)
        {
            //Factories
            case UpgradeID.FactoryClue:
                upgrade = new UpgradeData()
                {
                    itemId = upgradeID,
                    valuePerLevel = 1,
                    name = "CLUE FACTORY",
                    description = fullFactoryDescription,
                    backgroundColor = Color.blue,
                    playerLevelRequired = new []{ 3,8,15 },
                    iconMain = IconName.CLUE,
                    iconSecondary = IconName.FACTORY
                    
                };
                break;
            case UpgradeID.FactoryRemove:

                upgrade = new UpgradeData()
                {
                    itemId = upgradeID,
                    valuePerLevel = 1,
                    name = "REMOVE FACTORY",
                    description = fullFactoryDescription,
                    backgroundColor = Color.green,
                    playerLevelRequired = new int[] { 7 },
                    iconMain = IconName.REMOVE,
                    iconSecondary = IconName.FACTORY
                };
                break;
            case UpgradeID.FactoryCut:
                upgrade = new UpgradeData()
                {
                    itemId = upgradeID,
                    valuePerLevel = 1,
                    name = "CUT FACTORY",
                    description = fullFactoryDescription,
                    backgroundColor = Color.yellow,
                    playerLevelRequired = new int[] { 1, 2, 6, 8 },
                    iconMain = IconName.CUT,
                    iconSecondary = IconName.FACTORY
                };
                break;
            case UpgradeID.FactoryPeek:
                upgrade = new UpgradeData()
                {
                    itemId = upgradeID,
                    valuePerLevel = 1,
                    name = "PEEK FACTORY",
                    description = fullFactoryDescription,
                    backgroundColor = Color.magenta,
                    playerLevelRequired = new int[] { 12 },
                    iconMain = IconName.PEEK,
                    iconSecondary = IconName.FACTORY

                };
                break;    
            case UpgradeID.FactoryHighlight:
                upgrade = new UpgradeData()
                {
                    itemId = upgradeID,
                    valuePerLevel = 1,
                    name = "HIGHLIGHT FACTORY",
                    description = fullFactoryDescription,
                    backgroundColor = Color.green,
                    levelPrices = new int[] { 1,3,6,8 },
                    iconMain = IconName.HIGHLIGHT,
                    iconSecondary = IconName.FACTORY

                };
                break;     
            case UpgradeID.FactoryBomb:
                upgrade = new UpgradeData()
                {
                    itemId = upgradeID,
                    valuePerLevel = 1,
                    name = "BOMB FACTORY",
                    description = fullFactoryDescription,
                    backgroundColor = Color.cyan,
                    playerLevelRequired = new int[] { 1,6 },
                    iconMain = IconName.BOMB,
                    iconSecondary = IconName.FACTORY
                };
                break;
            
            // Upgrades
            case UpgradeID.ExtraLife:
                upgrade = new UpgradeData()
                {
                    itemId = upgradeID,
                    valuePerLevel = 1,
                    name = "EXTRA LIFE",
                    description = "Increase the amount of lives you start each game with to <color="+textHighlighColor+">"+ (upgradeLevel + 3)+"</color>.",
                    backgroundColor = Color.red,
                    playerLevelRequired = new int[] { 1,5,9,12},
                    iconMain = IconName.HEART,
                    iconSecondary = IconName.PLUS
                };
                break;
           
            case UpgradeID.BetterClue:
                upgrade = new UpgradeData()
                {
                    itemId = upgradeID,
                    valuePerLevel = 1,
                    name = "BETTER CLUE",
                    description = "Clues mark the number they were used on.",
                    backgroundColor = Color.blue,
                    playerLevelRequired = new int[] { 10 },
                    upgradeRequired = new Dictionary<UpgradeID, int>()
                    {
                        { UpgradeID.FactoryClue, 1 }
                    },
                    iconMain = IconName.CLUE,
                    iconSecondary = IconName.PLUS

                };
                break;
            case UpgradeID.BetterCut:
                upgrade = new UpgradeData()
                {
                    itemId = upgradeID,
                    valuePerLevel = 1,
                    name = "BETTER CUT",
                    description = "Cut blocks <color="+textHighlighColor+">"+ (upgradeLevel + 1)+" options.",
                    backgroundColor = Color.blue,
                    playerLevelRequired = new int[] { 6,9,12,15,18,20,22,24 },
                    upgradeRequired = new Dictionary<UpgradeID, int>()
                    {
                        { UpgradeID.FactoryCut, 1 }
                    },
                    iconMain = IconName.CUT,
                    iconSecondary = IconName.PLUS
                };
                break;
            case UpgradeID.LifeProtector:
                upgrade = new UpgradeData()
                {
                    itemId = upgradeID,
                    valuePerLevel = 1,
                    name = "LIFE PROTECTOR",
                    description = "Protect your life from one mistake. Recharges on <color="+textHighlighColor+">"+(11 - upgradeLevel)+" COMBO",
                    backgroundColor = Color.red,
                    playerLevelRequired = new int[] {9,11,13,15,17},
                    iconMain = IconName.HEART,
                    iconSecondary = IconName.SHIELD
                };
                break;
            case UpgradeID.BlockMistake:
                upgrade = new UpgradeData()
                {
                    itemId = upgradeID,
                    valuePerLevel = 1,
                    name = "BLOCK MISTAKE",
                    description = "On mistake, add a <color="+textHighlighColor+">Cut lv. "+upgradeLevel+" </color> to the sticker.",
                    backgroundColor = Color.yellow,
                    playerLevelRequired = new int[] { 9,12,15 },
                    iconMain = IconName.CUT,
                    iconSecondary = IconName.SHIELD
                };
                break;
            case UpgradeID.DeathDefy:
                upgrade = new UpgradeData()
                {
                    itemId = upgradeID,
                    valuePerLevel = 1,
                    name = "DEATH DEFY",
                    description = "Cheat death on a SMALL mistake once per lv",
                    backgroundColor = Color.red,
                    playerLevelRequired = new int[] { 12,15,18 },
                    iconMain = IconName.HEART,
                    iconSecondary = IconName.SKULL
                };
                break;           
            case UpgradeID.HealOnClear:
                string descriptionHealOnClear;
                if (5 - upgradeLevel != 1) {
                    descriptionHealOnClear = "Heal one heart per <color="+textHighlighColor+">"+  (5 - upgradeLevel) + " stickers cleared</color>.";
                }
                else {
                    descriptionHealOnClear = "Heal one heart per <color="+textHighlighColor+">sticker cleared</color>.";
                }
                upgrade = new UpgradeData()
                {
                    itemId = upgradeID,
                    valuePerLevel = 1,
                    name = "HEAL ON CLEAR",
                    description = descriptionHealOnClear,
                    backgroundColor = Color.red,
                    playerLevelRequired = new int[] { 0,4,8,12 },
                    iconMain = IconName.HEART,
                    iconSecondary = IconName.STICKER

                };
                break;            
            case UpgradeID.StickerMaster:
                upgrade = new UpgradeData()
                {
                    itemId = upgradeID,
                    valuePerLevel = 1,
                    name = "STICKER MASTER",
                    description = "Stickers on match come with bonuses, per sticker level",
                    backgroundColor = Color.red,
                    playerLevelRequired = new int[] { 20 },
                    iconMain = IconName.STICKER,
                    iconSecondary = IconName.PLUS
                };
                break;
            case UpgradeID.ConsumableSlot:
                upgrade = new UpgradeData()
                {
                    itemId = upgradeID,
                    valuePerLevel = 1,
                    name = "CONSUMABLE SLOT",
                    description = "Allows you to bring <color="+textHighlighColor+">"+ upgradeLevel+" extra consumables</color> to the match, and store <color="+textHighlighColor+">"+ 3*(upgradeLevel)+" extra consumables</color> on each factory.",
                    backgroundColor = Color.red,
                    playerLevelRequired = new int[] { 5,10 },
                    iconMain = IconName.BAG,
                    iconSecondary = IconName.PLUS
                };
                break;

            default:
                throw new Exception("Upgrade ID not found");
                break;
        }
        return upgrade;
    }
}




