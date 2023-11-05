using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
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
    public void SetStage(int stage) {
        foreach (var difficultyButton in difficultyButtons) {
            difficultyButton.SetStage(stage);
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
    public List<int> images;
    public Stage stageObject;

    [NonSerialized]
    public Color ColorValue; // Propiedad para acceder al valor de color

    // Constructor sin parámetros
    public StageData()
    {
    }

    // Constructor con parámetros
    public StageData(int stageID, string title, int basePoints, Color color, List<int> images)
    {
        this.stageID = stageID;
        this.title = title;
        this.basePoints = basePoints;
        this.ColorValue = color;
        this.color = ColorUtility.ToHtmlStringRGBA(color);
        this.images = images;
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
    public List<(int id, int level)> imageLevels;
    public int stage;
    public int difficulty;
    public DateTime date;
    public int score;
    public int amountOfTurns;
    public bool hardcore;
    public List<(int turn, int imageID, int amountOfAppearences, float turnDuration, int guess, TurnAction action, int
        remainingLives, int multiplier, int scoreModification)> turnHistory = new ();

    public Match(int stage, int difficulty, bool hardcore) {
        this.stage = stage;
        this.difficulty = difficulty;
        this.hardcore = hardcore;
    }

    public void AddTurn(int imageID, int amountOfAppearences, float turnDuration, int guess, TurnAction action, int
        remainingLives, int streak, int scoreModification) {
        turnHistory.Add((turnHistory.Count, imageID, amountOfAppearences, turnDuration, guess,action,remainingLives, streak, scoreModification));
    }

    public (List<int> clearedImages, List<Achievement> achievementsFullfilled) EndMatch() {
        foreach (var turn in turnHistory) {
            score += turn.scoreModification;
            amountOfTurns ++;
        }
        List<int> clearedImages = new();
        List<Achievement> achievementsFullFilled = new List<Achievement>();
        return (clearedImages, achievementsFullFilled);
    }
}
[Serializable]
public class UserData
{
    public string name;
    public int id;
    public List<UserStageData> stages;

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
    public List<AchievementData> achievements;
    public List<Match> matches;
}

[Serializable]
public class AchievementData
{
    public string achievement;
    public bool unlocked;
}

// Otras clases...

[Serializable]
public enum TurnAction {
    GuessCorrect,
    GuessIncorrect,
    UseClue,
    UseRemove,
    Peek,
    ReduceOptions
}

