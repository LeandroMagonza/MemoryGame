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
    private GameText name;
    private GameText description;
    public int price;
    public int generationMinutes;
    public int initialStorage;
    public int ID => (int)itemID;
    public static ConsumableData GetConsumable(ConsumableID itemID)
    {
        ConsumableData consumable = null;
        switch (itemID)
        {
            case ConsumableID.Clue:
                consumable = new ConsumableData()
                {
                    itemID = itemID,
                    price = 8,
                    name = GameText.ItemClueName,
                    description = GameText.ItemClueDescription,
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
                    name = GameText.ItemRemoveName,
                    description = GameText.ItemRemoveDescription,
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
                    name = GameText.ItemCutName,
                    description = GameText.ItemCutDescription,
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
                    name = GameText.ItemPeekName,
                    description = GameText.ItemPeekDescription,
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
                    name = GameText.ItemHighlightName,
                    description = GameText.ItemHighlightDescription,
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
                    name = GameText.ItemBombName,
                    description = GameText.ItemBombDescription,
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
                    name = GameText.ItemEnergyPotionName,
                    description = GameText.ItemEnergyPotionDescription,
                    amount = 1,
                    generationMinutes = 480,
                    initialStorage = 10,
                };
                break;
        }
        return consumable;
    }

    public string GenerateDescription() {
        return LocalizationManager.Instance.GetGameText(description);
    }
    public string GetName() {
        return LocalizationManager.Instance.GetGameText(name);
    }
    
}
[System.Serializable]
public class UpgradeData
{
    public UpgradeID itemId;
    public int valuePerLevel;
    public int baseValue;
    public GameText name;
    public GameText description;
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

    public string GenerateDescription(bool nextLevel) {
        int upgradeLevel = PersistanceManager.Instance.userData.GetUpgradeLevel(itemId);
        if (nextLevel) { upgradeLevel++; }
        
        ConsumableData correspondingConsumable = ConsumableData.GetConsumable(
            ItemHelper.GetCorrespondingConsumable(itemId));

        string generationTimeText = "";
        string storageText = "";
        string fullFactoryDescription = "";

        string textHighlightColor;
        if (nextLevel) {
            textHighlightColor = 	"#00ff00ff"; //lime
        }
        else {
            textHighlightColor = 	"#00ffffff"; //cyan
        }
        
        
        if (correspondingConsumable != null) {
            //CustomDebugger.Log("nextLevel: "+nextLevel+" upgradeLevel:"+upgradeLevel);
            generationTimeText = "<color="+textHighlightColor+">"+ItemHelper.FormatGenerationTime(
            correspondingConsumable.generationMinutes,
            upgradeLevel)+"</color>";
            var totalStorageAmount = (ConsumableData.GetConsumable(
                ItemHelper.GetCorrespondingConsumable(itemId)).initialStorage + upgradeLevel);
            
            storageText = LocalizationManager.Instance.GetGameText(GameText.UpgradeFactoryStorage);
            string localizedName = correspondingConsumable.GetName();
            string localizedDescription = correspondingConsumable.GenerateDescription();
            
            storageText = ItemHelper.ReplacePlaceholders(storageText, new() {
                { "textHighlightColor", textHighlightColor },
                { "correspondingConsumable.name", localizedName+((totalStorageAmount == 1) ? "" : "S")},
                { "totalStorageAmount", totalStorageAmount.ToString() }
            });

            fullFactoryDescription = LocalizationManager.Instance.GetGameText(GameText.UpgradeFactoryDescription);
            
            fullFactoryDescription = ItemHelper.ReplacePlaceholders(fullFactoryDescription, new() {
                {"generationTimeText",generationTimeText},
                {"storageText",storageText},
                {"correspondingConsumable.name",localizedName},
                {"correspondingConsumable.description",localizedDescription},
                {"upgradeLevel",upgradeLevel.ToString()}, // sumar aca valor sacado del string
            });
            return fullFactoryDescription;
        }
        else {
            string descriptionText = LocalizationManager.Instance.GetGameText(this.description);
            descriptionText = ItemHelper.ReplacePlaceholders(descriptionText, new() {
                {"textHighlightColor",textHighlightColor},
                {"upgradeLevel",upgradeLevel.ToString()}, // sumar aca valor sacado del string
            },
                new () {
                {"upgradeLevel",upgradeLevel}, // sumar aca valor sacado del string
                }
            
            );
            return descriptionText;
        }
    }

    public string GetName() {
        return LocalizationManager.Instance.GetGameText(this.name);
    }
    public static UpgradeData GetUpgrade(UpgradeID upgradeID)
    {
        UpgradeData upgrade;
        switch (upgradeID)
        {
            //Factories
            case UpgradeID.FactoryClue:
                upgrade = new UpgradeData()
                {
                    itemId = upgradeID,
                    valuePerLevel = 1,
                    name = GameText.UpgradeFactoryClueName,
                    description = GameText.UpgradeFactoryDescription,
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
                    name = GameText.UpgradeFactoryRemoveName,
                    description = GameText.UpgradeFactoryDescription,
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
                    name = GameText.UpgradeFactoryCutName,
                    description = GameText.UpgradeFactoryDescription,
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
                    name = GameText.UpgradeFactoryPeekName,
                    description = GameText.UpgradeFactoryDescription,
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
                    name =GameText.UpgradeFactoryHighlightName,
                    description = GameText.UpgradeFactoryDescription,
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
                    name = GameText.UpgradeFactoryBombName,
                    description = GameText.UpgradeFactoryDescription,
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
                    name = GameText.UpgradeExtraLifeName,
                    description = GameText.UpgradeExtraLifeDescription,
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
                    name = GameText.UpgradeBetterClueName,
                    description = GameText.UpgradeBetterClueDescription,
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
                    name = GameText.UpgradeBetterCutName,
                    description = GameText.UpgradeBetterCutDescription,
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
                    name = GameText.UpgradeLifeProtectorName,
                    description = GameText.UpgradeLifeProtectorDescription,
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
                    name = GameText.UpgradeBlockMistakeName,
                    description = GameText.UpgradeBlockMistakeDescription,
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
                    name = GameText.UpgradeDeathDefyName,
                    description = GameText.UpgradeDeathDefyDescription,
                    backgroundColor = Color.red,
                    playerLevelRequired = new int[] { 12,15,18 },
                    iconMain = IconName.HEART,
                    iconSecondary = IconName.SKULL
                };
                break;           
            case UpgradeID.HealOnClear:
                upgrade = new UpgradeData()
                {
                    itemId = upgradeID,
                    valuePerLevel = 1,
                    name = GameText.UpgradeHealOnClearName,
                    description = GameText.UpgradeHealOnClearDescription,
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
                    name = GameText.UpgradeStickerMasterName,
                    description = GameText.UpgradeStickerMasterDescription,
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
                    name = GameText.UpgradeConsumableSlotName,
                    description = GameText.UpgradeConsumableSlotDescription,
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




