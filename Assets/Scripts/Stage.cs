using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using ColorUtility = UnityEngine.ColorUtility;

public class Stage : MonoBehaviour
{
    public int stage;
    public int difficulty;
    public Color baseColor;
    public TextMeshProUGUI titleText; 
    public TextMeshProUGUI amountStickersTotalText; 
    //public TextMeshProUGUI amountStickersCurrentText; 
    public DifficultyButton difficultyButton;
    public GameObject unlockMessage;
    public bool shining = false;
    public float shiningSpeed = 1;
    public void SetTitle(string title) {
        titleText.text = title;
    }

    public void SetColor(Color color)
    {
        baseColor = color;
        GetComponent<Image>().color = color;
    }

    public void SetAmountOfStickersTotal(int imageListCount) {
        amountStickersTotalText.text = imageListCount.ToString();
    }
    /*public void SetAmountOfStickersCurrent(int imageListCount) {
        amountStickersCurrentText.text = imageListCount.ToString();
    }*/

    public void SetScore(int score) {
        difficultyButton.SetScore(score);
    }
    public void SetStage(int stage,int difficulty)
    {
        this.stage = stage;
        this.difficulty = difficulty;
        difficultyButton.SetStage(stage,difficulty);
        
        UpdateDifficultyUnlockedAndAmountOfStickersUnlocked();
        
    }
    public void UpdateDifficultyUnlockedAndAmountOfStickersUnlocked()
    {
        // se eliminan el requerimiento de stickers, solo se requiere ahora que tengas el nivel anterior con una 
        /*
        List<int> unlockedStickers = new List<int>();
        
        foreach (var stickerFromStage in GameManager.Instance.stages[stage].stickers) {
            //tiene por lo menos una vez la figurita del stage, en sus imageduplicates
            if (GameManager.Instance.userData.stickerDuplicates.ContainsKey((GameManager.Instance.stages[stage].stickerSet,stickerFromStage))
                &&
                GameManager.Instance.userData.stickerDuplicates[(GameManager.Instance.stages[stage].stickerSet,stickerFromStage)] > 0) {
                unlockedStickers.Add(stickerFromStage);
            }
        }
        //SetAmountOfStickersCurrent(unlockedStickers.Count);
        if (unlockedStickers.Count < GameManager.Instance.stages[stage].stickers.Count) {
            unlockMessage.SetActive(true);
            difficultyButton.transform.parent.gameObject.SetActive(false);
        }
        else {
            unlockMessage.SetActive(false);
            difficultyButton.transform.parent.gameObject.SetActive(true);
        }
        */

        SetAmountOfStickersTotal(GameManager.Instance.stages[stage].stickers.Count);
        unlockMessage.SetActive(false);
        difficultyButton.UpdateDifficultyUnlocked();
    }

    public void OpenStickerPanel() {
        StageManager.Instance.OpenStickerPanel(stage);
    }

    private void FixedUpdate()
    {
        if (shining) {
            GetComponent<Image>().color = new Color(
                GetComponent<Image>().color.r - baseColor.r * shiningSpeed * Time.deltaTime,
                GetComponent<Image>().color.g - baseColor.g * shiningSpeed * Time.deltaTime,
                GetComponent<Image>().color.b - baseColor.b * shiningSpeed * Time.deltaTime
            );
            if(GetComponent<Image>().color.r < baseColor.r*0.5f) {
                GetComponent<Image>().color = baseColor;
            }
        }
        else
        {
            GetComponent<Image>().color = baseColor;
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
    public int packCost;
    [FormerlySerializedAs("imageSet")] public StickerSet stickerSet;
    [NonSerialized]
    public Color ColorValue; // Propiedad para acceder al valor de color

    // Constructor sin parámetros
    public StageData()
    {
    }

    // Constructor con parámetros
    public StageData(int stageID, string title, int basePoints, Color color, List<int> stickers, StickerSet stickerSet)
    {
        this.stageID = stageID;
        this.title = title;
        this.basePoints = basePoints;
        this.ColorValue = color;
        this.color = ColorUtility.ToHtmlStringRGBA(color);
        this.stickers = stickers;
        this.stickerSet = stickerSet;
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

//[JsonConverter(typeof(StringEnumConverter))]
public enum Achievement {
    BarelyClearedStage,
    ClearedStageWithMoreThanOneHP,
    ClearedStageNoMistakes,
    ClearedStageNoUpgrades,
    ClearedStageNoMistakesNoUpgrades,
    FastWin
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
    public List<Achievement> achievementsFulfilled = new ();

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
        int removesUsed = 0;
        bool reachedOneHP = false;
        bool speedrunAchievement = true;
        CustomDebugger.Log("Ending match with turns:"+turnHistory.Count);
        foreach (var turn in turnHistory) {
            score += turn.scoreModification;
            amountOfTurns ++;
            CustomDebugger.Log("turnNumber: "+amountOfTurns);
            CustomDebugger.Log("turn.amountOfAppearences == difficulty && (turn.action == TurnAction.GuessCorrect ||turn.action == TurnAction.UseClue)");
            CustomDebugger.Log(turn.amountOfAppearences +"=="+ difficulty +"&& ("+turn.action +"=="+ TurnAction.GuessCorrect +"||"+turn.action+" == "+TurnAction.UseClue+")");
            CustomDebugger.Log(turn.amountOfAppearences == difficulty && (turn.action == TurnAction.GuessCorrect ||turn.action == TurnAction.UseClue));
            if (turn.amountOfAppearences == difficulty
                &&
                (
                 GameManager.Instance.increaseAmountOfAppearencesOnMistake
                 || 
                 turn.action == TurnAction.GuessCorrect || turn.action == TurnAction.UseClue)
                ) {
                clearedImages.Add(turn.imageID);
                CustomDebugger.Log("Ädding clearedImages stickerID"+turn.imageID);
            }

            if (turn.remainingLives < maxLives)
            {
                lostLife = true;
            }

            if (turn.action == TurnAction.UseRemove) {
                removesUsed++;
            }  
            if (turn.remainingLives == 1) {
                reachedOneHP = true;
            }

            if (turn.turnDuration > GameManager.Instance.maxTimerSpeedrun)
            {
                speedrunAchievement = false;
            }

        }
        
        CustomDebugger.Log("clearedImages " +clearedImages.Count);
        date = DateTime.Now;
        //Check achievements
        int amountOfStickersInStage = GameManager.Instance.stages[stage].stickers.Count;
        CustomDebugger.Log("-------------------Achievement ClearedEveryImage----------------------");
        CustomDebugger.Log("amountOfImagesInStage == clearedImages.Count");
        CustomDebugger.Log(amountOfStickersInStage +" == "+ clearedImages.Count);
        CustomDebugger.Log(amountOfStickersInStage == clearedImages.Count);
        CustomDebugger.Log("---------------------------------------------------------");
        
        if (amountOfStickersInStage-removesUsed <= clearedImages.Count)
        {
            achievementsFulfilled.Add(Achievement.BarelyClearedStage);
            if (!reachedOneHP)
            {
                achievementsFulfilled.Add(Achievement.ClearedStageWithMoreThanOneHP);
                if (!lostLife)
                {
                    achievementsFulfilled.Add(Achievement.ClearedStageNoMistakes);
                    if (speedrunAchievement)
                    {
                        achievementsFulfilled.Add(Achievement.FastWin);
                    }
                }
            }

           
        }

        CustomDebugger.Log("-------------------Achievement ClearedStageWithMoreThanOneHP----------------------");
        CustomDebugger.Log("reachedOneHP");
        CustomDebugger.Log(reachedOneHP);
        CustomDebugger.Log("---------------------------------------------------------");

        
        
        return (clearedImages, achievementsFulfilled);
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
    
    [JsonIgnore] // Ignora esta propiedad durante la deserialización
    public Dictionary<(StickerSet,int),int> stickerDuplicates = new Dictionary<(StickerSet, int), int>();

    public List<DuplicateEntry> readStickerDuplicates = new List<DuplicateEntry>();
    //upgrades, inventario
    public Dictionary<ConsumableID, int> consumables = new Dictionary<ConsumableID, int>();
    public Dictionary<UpgradeID, int> upgrades = new Dictionary<UpgradeID, int>();
    //historial de compras

    public UserStageData GetUserStageData(int stage, int difficulty)
    {
        if (stage < 0 || difficulty < 0 )
        {
            return null;
        }
        foreach (var userStageData in stages)
        {
            if (userStageData.stage == stage && userStageData.difficulty == difficulty)
            {
                return userStageData;
            }
        }
        return null;
    }
    public void ConvertStickerListToDictionary()
    {
        CustomDebugger.Log("stickers read "+readStickerDuplicates.Count);
        foreach (var VARIABLE in readStickerDuplicates)
        {
            CustomDebugger.Log((VARIABLE.Key).ToString()+ (VARIABLE.Value),DebugCategory.STICKERLOAD);
        }
        stickerDuplicates = readStickerDuplicates.ToDictionary(
            entry => (entry.Key.StickerSet, entry.Key.StickerID),
            entry => entry.Value
        );
        CustomDebugger.Log("stickers read "+stickerDuplicates.Count);
    }    
    public void ConvertStickerDictionaryToList()
    {
        readStickerDuplicates = new List<DuplicateEntry>();
        foreach (var stickerDuplicate in stickerDuplicates)
        {
            DuplicateEntry newDuplicateEntry = new DuplicateEntry();
            StickerKey stickerKey = new StickerKey();
            stickerKey.StickerSet = stickerDuplicate.Key.Item1;
            stickerKey.StickerID = stickerDuplicate.Key.Item2;
            newDuplicateEntry.Key = stickerKey;
            newDuplicateEntry.Value = stickerDuplicate.Value;
            readStickerDuplicates.Add(newDuplicateEntry); 
        }
    }
    public bool ModifyCoins(int modificationAmount)
    {
        modificationAmount += coins;
        if (modificationAmount >= 0)
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
            upgrades[upgradeID]++;
        }
        else
        {
            upgrades.Add(upgradeID, 1);
        }

        if (upgradeID == UpgradeID.ExtraLife)
        {
            GameManager.Instance.lifeCounter.Start();
        }
    }
    public void AddConsumableObject(ConsumableID consumableID)
    {
        if (consumables.ContainsKey(consumableID))
        {
            consumables[consumableID]++;
        }
        else
        {
            consumables.Add(consumableID, 1);
        }
    }
    public void modifyConsumableObject(ConsumableID consumableID, int amount)
    {
        if (consumables.ContainsKey(consumableID))
        {
            consumables[consumableID] += amount;
        }
    }


    public Dictionary<ConsumableID, (int current, int max, (int baseValue, int consumableValue) initial)> GetMatchInventory()
    {
        Dictionary<ConsumableID, (int current, int max, (int baseValue, int consumableValue) initial)> temp_inventory = new Dictionary<ConsumableID, (int current, int max, (int baseValue, int consumableValue) initial)>();

        Dictionary<ConsumableID, UpgradeID> upgradeRelation = new Dictionary<ConsumableID, UpgradeID>() {
            {ConsumableID.Clue, UpgradeID.ExtraClue },
            {ConsumableID.Remove, UpgradeID.ExtraRemove },
            {ConsumableID.Cut, UpgradeID.ExtraCut },
            {ConsumableID.Peek, UpgradeID.ExtraPeek }
        };

        foreach (var item in upgradeRelation)
        {
            ConsumableID consumableID = item.Key;
            UpgradeID upgradeID = item.Value;

            int currentLevel = 0;
            if (PersistanceManager.Instance.userData.upgrades.ContainsKey(upgradeID))
            {
                currentLevel = PersistanceManager.Instance.userData.upgrades[upgradeID];
            }

            int max = ConsumableData.GetConsumable(consumableID).max + UpgradeData.GetUpgrade(upgradeID).GetAdditionalMax(currentLevel);
            int baseValue = UpgradeData.GetUpgrade(upgradeID).GetAdditionalItem(currentLevel);
            int initialValue = 0;
            if (PersistanceManager.Instance.userData.consumables.ContainsKey(consumableID))
            {
                initialValue += PersistanceManager.Instance.userData.consumables[consumableID];
            }
            int total = initialValue + baseValue;
            total = Mathf.Clamp(total, 0, max);
            temp_inventory.Add(consumableID, (total, max, (baseValue, initialValue)));
        }

        //temp_inventory[ConsumableID.Clue];
        //;
        //PersistanceManager.Instance.userData.upgrades.[UpgradeID.MaxClue];



        CustomDebugger.Log("tempCount" + temp_inventory.Count);
        return temp_inventory;
    }
}
    

[Serializable]
public class UserStageData
{
    public int stage;
    public int difficulty;
    //public List<int> clearedStickers = new List<int>();
    public int highScore = 0;
    //[JsonProperty (ItemConverterType = typeof(StringEnumConverter))]
    [JsonIgnore]
    public List<Achievement> achievements = new List<Achievement>();
    [JsonProperty("achievements")]
    public List<string> achievementsUnparsed = new List<string>();
    
    public List<Match> matches = new List<Match>();

    public UserStageData(int stage, int difficulty)
    {
        this.stage = stage;
        this.difficulty = difficulty;
    }
    public List<Achievement> AddMatch(Match currentMatch)
    {
        List<Achievement> firstTimeAchievements = new List<Achievement>();
        matches.Add(currentMatch);
        var matchResult = currentMatch.EndMatch();
        
        CustomDebugger.Log("achievements fullfilled count"+matchResult.achievementsFullfilled.Count);
        foreach (var previouslyFullfilledAchievement in matchResult.achievementsFullfilled) {
            CustomDebugger.Log("achievement fullfilled "+previouslyFullfilledAchievement);
            if (!achievements.Contains(previouslyFullfilledAchievement))
            {
                achievements.Add(previouslyFullfilledAchievement);
                firstTimeAchievements.Add(previouslyFullfilledAchievement);
            } 
        }
        
        CustomDebugger.Log("Added Match with turns "+currentMatch.turnHistory.Count);
        return firstTimeAchievements;
    }
    /*public bool HasUnlockedStage()
    {
        foreach (var imageFromStage in GameManager.Instance.stages[stage].stickers) {
            //tiene por lo menos una vez la figurita del stage, en sus imageduplicates
            if (!GameManager.Instance.userData.stickerDuplicates.ContainsKey((GameManager.Instance.stages[stage].stickerSet,imageFromStage))
                ||
                GameManager.Instance.userData.stickerDuplicates[(GameManager.Instance.stages[stage].stickerSet,imageFromStage)] <= 0) {
                return  false;
            }
        }
        return true;
    }*/
}

public class StickerKey
{
    [JsonConverter(typeof(StickerSetConverter))]
    public StickerSet StickerSet { get; set; }
    public int StickerID { get; set; }
}

public class DuplicateEntry
{
    public StickerKey Key { get; set; }
    public int Value { get; set; }
}

//[JsonConverter(typeof(StringEnumConverter))]
public enum TurnAction {
    GuessCorrect,
    GuessIncorrect,
    UseClue,
    UseRemove,
    UseCut,
    UsePeek,
    RanOutOfTime,
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