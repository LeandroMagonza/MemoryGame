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
    public TextMeshProUGUI titleText; 
    public TextMeshProUGUI amountOfImagesText; 
    public List<DifficultyButton> difficultyButtons; 
    public void SetTitle(string title) {
        titleText.text = title;
    }

    public void SetColor(Color color) {
        GetComponent<Image>().color = color;
    }

    public void SetAmountOfImages(int imageListCount) {
        amountOfImagesText.text = imageListCount.ToString();
    }

    public void SetScore(int difficulty, int score) {
        difficultyButtons[difficulty].SetScore(score);
    }
    public void SetStage(int stage)
    {
        int difficulty = 0;
        foreach (var difficultyButton in difficultyButtons)
        {
            difficultyButton.SetStage(stage);
            difficulty++;
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
    public Dictionary<int,int> imageDuplicates;
    //upgrades, inventario
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

