using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ColorUtility = UnityEngine.ColorUtility;

public class Stage : MonoBehaviour
{
    public int stage;
    public TextMeshProUGUI titleText; 
    public TextMeshProUGUI amountStickersTotalText; 
    public TextMeshProUGUI amountStickersCurrentText; 
    public List<DifficultyButton> difficultyButtons; 
    public void SetTitle(string title) {
        titleText.text = title;
    }

    public void SetColor(Color color) {
        GetComponent<Image>().color = color;
    }

    public void SetAmountOfStickersTotal(int imageListCount) {
        amountStickersTotalText.text = imageListCount.ToString();
    }
    public void SetAmountOfStickersCurrent(int imageListCount) {
        amountStickersCurrentText.text = imageListCount.ToString();
    }

    public void SetScore(int difficulty, int score) {
        difficultyButtons[difficulty].SetScore(score);
    }
    public void SetStage(int stage)
    {
        this.stage = stage;
        int difficulty = 0;
        foreach (var difficultyButton in difficultyButtons)
        {
            difficultyButton.SetStage(stage);
            difficulty++;
        }
        UpdateDifficultyUnlockedAndAmountOfStickersUnlocked();
    }
    public void UpdateDifficultyUnlockedAndAmountOfStickersUnlocked()
    {
        List<int> unlockedStickers = new List<int>();
        
        foreach (var stickerFromStage in GameManager.Instance.stages[stage].stickers) {
            //tiene por lo menos una vez la figurita del stage, en sus imageduplicates
            if (GameManager.Instance.userData.imageDuplicates.ContainsKey(stickerFromStage)
                &&
                GameManager.Instance.userData.imageDuplicates[stickerFromStage] > 0) {
                unlockedStickers.Add(stickerFromStage);
            }
        }
        SetAmountOfStickersCurrent(unlockedStickers.Count);
        SetAmountOfStickersTotal(GameManager.Instance.stages[stage].stickers.Count);
        foreach (var difficultyButton in difficultyButtons)
        {
            difficultyButton.UpdateDifficultyUnlocked();
        }
    }
    
}

[Serializable]
public class StageData
{
    public int stageID;
    public string title;
    public int basePoints;
    [JsonProperty("color")]
    public string color; // Almacenar como string en formato hexadecimal
    public List<int> stickers;
    public Stage stageObject;
    //int = stageID, int = chance de una carta de ese stage, la suma de todos los floats tiene que dar 100
    public Dictionary<int, int> packOdds = new Dictionary<int, int>(); 
    [NonSerialized]
    public Color ColorValue; // Propiedad para acceder al valor de color

    // Constructor sin parámetros
    public StageData()
    {
    }

    // Constructor con parámetros
    public StageData(int stageID, string title, int basePoints, Color color, List<int> stickers)
    {
        this.stageID = stageID;
        this.title = title;
        this.basePoints = basePoints;
        this.ColorValue = color;
        this.color = ColorUtility.ToHtmlStringRGBA(color);
        this.stickers = stickers;
    }

    // Método para convertir el string hexadecimal a Color después de la deserialización
    public void ConvertColorStringToColorValue()
    {
        if (ColorUtility.TryParseHtmlString("#" + color, out Color colorValue))
        {
            ColorValue = colorValue;
        }
    }
}

[Serializable]
public enum Achievement {
    ClearedEveryImage,
    ClearedStage,
    ClearedStageNoMistakes,
    ClearedStageNoUpgrades,
    ClearedStageNoMistakesNoUpgrades,
}
[Serializable]
public class Match
{
    //public List<Updgrades> upgrades;
    public List<(int id, int level)> imageDuplicates = new List<(int id, int level)>();
    public int stage;
    public int difficulty;
    public DateTime date;
    public int score;
    public int amountOfTurns;
    public bool hardcore;
    public List<TurnData> turnHistory = new ();

    public Match(int stage, int difficulty, bool hardcore) {
        this.stage = stage;
        this.difficulty = difficulty;
        this.hardcore = hardcore;
    }

    public void AddTurn(int imageID, int amountOfAppearences, float turnDuration, int guess, TurnAction action, int
        remainingLives, int streak, int scoreModification) {
        turnHistory.Add(new TurnData(turnHistory.Count, imageID, amountOfAppearences, turnDuration, guess,action,remainingLives, streak, scoreModification));
    }

    public (List<int> clearedImages, List<Achievement> achievementsFullfilled) EndMatch() {
        List<int> clearedImages = new();
        int maxLives = turnHistory[0].remainingLives;
        bool lostLife = false;
        foreach (var turn in turnHistory) {
            score += turn.scoreModification;
            amountOfTurns ++;
            if (turn.amountOfAppearences == GameManager.Instance.DifficultyToAmountOfAppearences(difficulty) && (turn.action == TurnAction.GuessCorrect ||turn.action == TurnAction.UseClue)) {
                clearedImages.Add(turn.imageID);
                Debug.Log("Ädding clearedImages");
            }

            if (turn.remainingLives < maxLives)
            {
                lostLife = true;
            }
        }
        
        Debug.Log("clearedImages " +clearedImages.Count);
        List<Achievement> achievementsFullFilled = new List<Achievement>();
        date = DateTime.Now;
        if (!lostLife) achievementsFullFilled.Add(Achievement.ClearedStageNoMistakes);
        return (clearedImages, achievementsFullFilled);
    }
}

[Serializable]
public class TurnData
{
    public int turn;
    public int imageID;
    public int amountOfAppearences;
    public float turnDuration;
    public int guess;
    public TurnAction action;
    public int remainingLives;
    public int combo;
    public int scoreModification;
    public TurnData(int turn, int imageID, int amountOfAppearences, float turnDuration, int guess, TurnAction action,
        int remainingLives, int combo, int scoreModification)
    {
        this.turn = turn;
        this.imageID = imageID;
        this.amountOfAppearences = amountOfAppearences;
        this.turnDuration = turnDuration;
        this.guess = guess;
        this.action = action;
        this.remainingLives = remainingLives;
        this.combo = combo;
        this.scoreModification = scoreModification;
    }
}
[Serializable]
public class UserData
{
    public string name;
    public int id;
    public int coins;
    public List<UserStageData> stages;
    public Dictionary<int,int> imageDuplicates = new Dictionary<int, int>();
    public Dictionary<ConsumableID, ConsumableData> consumables = new Dictionary<ConsumableID, ConsumableData>();
    public Dictionary<UpgradeID, UpgradeData> upgrades = new Dictionary<UpgradeID, UpgradeData>();
    //historial de compras

    public UserStageData GetUserStageData(int stage,int difficulty)
    {
        foreach (var VARIABLE in stages)
        {
            if (VARIABLE.stage == stage && VARIABLE.difficulty == difficulty)
            {
                return VARIABLE;
            }
        }
        return null;
    }

    public bool ModifyCoins(int modificationAmount)
    {
        modificationAmount += coins;
        if (modificationAmount >= 0 )
        {
            coins = modificationAmount;
            return true;
        }
        return false;
    }
    public void AddUpgradeObject(UpgradeID upgradeID)
    {
        if (upgrades.ContainsKey(upgradeID))
        {
            upgrades[upgradeID].LevelUp();
        }
        else
        {
            upgrades.Add(upgradeID, UpgradeData.GetUpgrade(upgradeID));
            upgrades[upgradeID].LevelUp();
        }
    }
    public void AddConsumableObject(ConsumableID consumableID)
    {
        if (consumables.ContainsKey(consumableID))
        {
            consumables[consumableID].AddCurrent(1);
        }
        else
        {
            consumables.Add(consumableID, ConsumableData.GetConsumable(consumableID));
            consumables[consumableID].AddCurrent(1);
        }
    }
    public void modifyConsumableObject(ConsumableID consumableID, int amount)
    {
        if (consumables.ContainsKey(consumableID))
        {
            consumables[consumableID].AddCurrent(amount);
        }
    }
   

    public Dictionary<ConsumableID, int> GetMatchInventory()
    {
        Dictionary<ConsumableID, int> temp_inventory = new Dictionary<ConsumableID, int>();
        ConsumableID clue = ConsumableID.Clue;
        ConsumableID remove = ConsumableID.Remove;
        ConsumableID cut = ConsumableID.Cut;
        ConsumableID peek = ConsumableID.Peek;

        int maxClue = 0;
        int maxRemove = 0;
        int maxCut = 0;
        int maxPeek = 0;

        foreach (UpgradeData upgrade in upgrades.Values)
        {
            switch (upgrade.itemId)
            {
                case UpgradeID.MaxClue:
                    if (!temp_inventory.ContainsKey(clue))
                    {
                        temp_inventory.Add(clue, upgrade.GetAdditionalItem());
                    }
                    else
                    {
                        temp_inventory[clue] += upgrade.GetAdditionalItem();
                    }
                    maxClue += upgrade.GetAdditionalMax();
                    break;
                case UpgradeID.MaxRemove:
                    if (!temp_inventory.ContainsKey(remove))
                    {
                        temp_inventory.Add(remove, upgrade.GetAdditionalItem());
                    }
                    else
                    {
                        temp_inventory[remove] += upgrade.GetAdditionalItem();
                    }
                    maxRemove += upgrade.GetAdditionalMax();
                    break;
                case UpgradeID.MaxCut:
                    if (!temp_inventory.ContainsKey(cut))
                    {
                        temp_inventory.Add(cut, upgrade.GetAdditionalItem());
                    }
                    else
                    {
                        temp_inventory[cut] += upgrade.GetAdditionalItem();
                    }
                    maxCut += upgrade.GetAdditionalMax();
                    break;
                case UpgradeID.BetterPeek:
                    if (!temp_inventory.ContainsKey(peek))
                    {
                        temp_inventory.Add(peek, upgrade.GetAdditionalItem());
                    }
                    else
                    {
                        temp_inventory[peek] += upgrade.GetAdditionalItem();
                    }
                    maxPeek += upgrade.GetAdditionalMax();
                    break;
                case UpgradeID.BetterClue:

                    break;
                case UpgradeID.BetterCut:
                    break;
                case UpgradeID.Block:
                    break;
                case UpgradeID.DeathDefy:
                    break;
                case UpgradeID.ExtraLife:
                    break;

                case UpgradeID.ProtectedLife:
                    break;
                case UpgradeID.NONE:
                    break;
                default:
                    break;
            }
        }
        foreach (ConsumableData consumableData in consumables.Values)
        {
            switch (consumableData.itemID)
            {
                case ConsumableID.Clue:
                    if (!temp_inventory.ContainsKey(clue))
                    {
                        temp_inventory.Add(consumableData.itemID, consumableData.current);
                    }
                    else
                    {
                        temp_inventory[consumableData.itemID] += consumableData.current;
                    }
                    maxClue += consumableData.max;
                    break;
                case ConsumableID.Remove:
                    if (!temp_inventory.ContainsKey(remove))
                    {
                        temp_inventory.Add(consumableData.itemID, consumableData.current);
                    }
                    else
                    {
                        temp_inventory[consumableData.itemID] += consumableData.current;
                    }
                    maxRemove += consumableData.max;
                    break;
                case ConsumableID.Cut:
                    if (!temp_inventory.ContainsKey(cut))
                    {
                        temp_inventory.Add(consumableData.itemID, consumableData.current);
                    }
                    else
                    {
                        temp_inventory[consumableData.itemID] += consumableData.current;
                    }
                    maxCut += consumableData.max;
                    break;
                case ConsumableID.Peek:
                    if (!temp_inventory.ContainsKey(peek))
                    {
                        temp_inventory.Add(consumableData.itemID, consumableData.current);
                    }
                    else
                    {
                        temp_inventory[consumableData.itemID] += consumableData.current;
                    }
                    break;
                case ConsumableID.NONE:
                    break;
                default:
                    break;
            }


        }

        if (temp_inventory.Count > 0)
        {
            if (temp_inventory.ContainsKey(clue))
            {
                temp_inventory[clue] = Mathf.Clamp(temp_inventory[clue], 0, maxClue);
            }
            if (temp_inventory.ContainsKey(remove))
            {
                temp_inventory[remove] = Mathf.Clamp(temp_inventory[remove], 0, maxRemove);
            }
            if (temp_inventory.ContainsKey(cut))
            {
                temp_inventory[cut] = Mathf.Clamp(temp_inventory[cut], 0, maxCut);
            }
            if (temp_inventory.ContainsKey(peek))
            {
                temp_inventory[peek] = Mathf.Clamp(temp_inventory[peek], 0, maxPeek);
            }
        }
        Debug.Log("tempCount" + temp_inventory.Count);
        return temp_inventory;
    }

    public Dictionary<ConsumableID, int> GetAditionalValueData()
    {
        Dictionary<ConsumableID, int> temp_inventory = new Dictionary<ConsumableID, int>();
        ConsumableID clue = ConsumableID.Clue;
        ConsumableID remove = ConsumableID.Remove;
        ConsumableID cut = ConsumableID.Cut;
        ConsumableID peek = ConsumableID.Peek;

        foreach (UpgradeData upgrade in upgrades.Values)
        {
            switch (upgrade.itemId)
            {
                case UpgradeID.MaxClue:
                    if (!temp_inventory.ContainsKey(clue))
                    {
                        temp_inventory.Add(clue, upgrade.GetAdditionalItem());
                    }
                    else
                    {
                        temp_inventory[clue] += upgrade.GetAdditionalItem();
                    }
                    break;
                case UpgradeID.MaxRemove:
                    if (!temp_inventory.ContainsKey(remove))
                    {
                        temp_inventory.Add(remove, upgrade.GetAdditionalItem());
                    }
                    else
                    {
                        temp_inventory[remove] += upgrade.GetAdditionalItem();
                    }
                    break;
                case UpgradeID.MaxCut:
                    if (!temp_inventory.ContainsKey(cut))
                    {
                        temp_inventory.Add(cut, upgrade.GetAdditionalItem());
                    }
                    else
                    {
                        temp_inventory[cut] += upgrade.GetAdditionalItem();
                    }
                    break;
                case UpgradeID.BetterPeek:
                    if (!temp_inventory.ContainsKey(peek))
                    {
                        temp_inventory.Add(peek, upgrade.GetAdditionalItem());
                    }
                    else
                    {
                        temp_inventory[peek] += upgrade.GetAdditionalItem();
                    }
                    break;
                case UpgradeID.BetterClue:

                    break;
                case UpgradeID.BetterCut:
                    break;
                case UpgradeID.Block:
                    break;
                case UpgradeID.DeathDefy:
                    break;
                case UpgradeID.ExtraLife:
                    break;

                case UpgradeID.ProtectedLife:
                    break;
                case UpgradeID.NONE:
                    break;
                default:
                    break;
            }
        }

        Debug.Log("tempCount"+temp_inventory.Count);
        return temp_inventory;
    }

}

[Serializable]
public class UserStageData
{
    public int stage;
    public int difficulty;
    public List<int> clearedImages;
    public int highScore;
    [JsonProperty (ItemConverterType = typeof(StringEnumConverter))]
    public List<Achievement> achievements;
    
    public List<Match> matches;

    public List<Achievement> AddMatch(Match currentMatch)
    {
        List<Achievement> firstTimeAchievements = new List<Achievement>();
        matches.Add(currentMatch);
        var matchResult = currentMatch.EndMatch();
        int amountOfImagesInStage = GameManager.Instance.stages[stage].stickers.Count;
        foreach (var clearedImageID in matchResult.clearedImages) {
            if ( !clearedImages.Contains(clearedImageID)) clearedImages.Add(clearedImageID); 
        }
        Debug.Log("-------------------Achievement ClearedStage----------------------");
        Debug.Log("amountOfImagesInStage == matchResult.clearedImages.Count");
        Debug.Log(amountOfImagesInStage +" == "+ matchResult.clearedImages.Count);
        Debug.Log(amountOfImagesInStage == matchResult.clearedImages.Count);
        Debug.Log("---------------------------------------------------------");
        
        if (amountOfImagesInStage == matchResult.clearedImages.Count)
        {
            matchResult.achievementsFullfilled.Add(Achievement.ClearedStage);
        }
        Debug.Log("-------------------Achievement ClearedEveryImage----------------------");
        Debug.Log("amountOfImagesInStage == clearedImages.Count");
        Debug.Log(amountOfImagesInStage +" == "+ clearedImages.Count);
        Debug.Log(amountOfImagesInStage == clearedImages.Count);
        Debug.Log("---------------------------------------------------------");
        if (amountOfImagesInStage == clearedImages.Count)
        {
            matchResult.achievementsFullfilled.Add(Achievement.ClearedEveryImage);
        }
        Debug.Log("achievements fullfilled count"+matchResult.achievementsFullfilled.Count);
        foreach (var fullfilledAchievement in matchResult.achievementsFullfilled) {
            Debug.Log("achievement fullfilled "+fullfilledAchievement);
            if (!achievements.Contains(fullfilledAchievement))
            {
                achievements.Add(fullfilledAchievement);
                firstTimeAchievements.Add(fullfilledAchievement);
            } 
        }
        
        Debug.Log("Added Match with turns "+currentMatch.turnHistory.Count);
        return firstTimeAchievements;
    }
    public bool HasUnlockedStage()
    {
        foreach (var imageFromStage in GameManager.Instance.stages[stage].stickers) {
            //tiene por lo menos una vez la figurita del stage, en sus imageduplicates
            if (!GameManager.Instance.userData.imageDuplicates.ContainsKey(imageFromStage)
                ||
                GameManager.Instance.userData.imageDuplicates[imageFromStage] <= 0) {
                return  false;
            }
        }
        return true;
    }
}

// Otras clases...

[JsonConverter(typeof(StringEnumConverter))]
public enum TurnAction {
    GuessCorrect,
    GuessIncorrect,
    UseClue,
    UseRemove,
    Peek,
    ReduceOptions
}

public class StickerLevelsData
{
    public int amountRequired { get; set; }
}

// Estructura de Packs
public class PacksData
{
    public float rareChance { get; set; }
    public int rareAmountOfStickers { get; set; }
    public float legendaryChance { get; set; }
    public int legendaryAmountOfStickers { get; set; }
    public int stickersPerPack { get; set; }
}