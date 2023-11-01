using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.XR;
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
    public string color; // Almacenar como string en formato hexadecimal
    public List<int> images;
    public Stage stageObject;

    [NonSerialized]
    public Color ColorValue; // Propiedad para acceder al valor de color

    public StageData(int stageID, string title, int basePoints, Color color, List<int> images)
    {
        this.stageID = stageID;
        this.title = title;
        this.basePoints = basePoints;
        this.ColorValue = color;
        this.color = ColorUtility.ToHtmlStringRGBA(color);
        this.images = images;
    }
}


public class UserData
{
    public string name;
    public int id;
    public List<int> images;
    public Dictionary<(int stage, int difficulty), UserStageData> stages = new();
}

public class UserStageData {
    public List<int> clearedImages;
    public int highScore;
    public Dictionary<Achievement, bool> achievements = new Dictionary<Achievement, bool>() {
        { Achievement.ClearedEveryImage, false },
        { Achievement.ClearedStage, false },
        { Achievement.ClearedStageNoMistakes, false },
        { Achievement.ClearedStageNoUpgrades, false },
        { Achievement.ClearedStageNoMistakesNoUpgrades, false }
    };
    public List<Match> matches;

}

public enum Achievement {
    ClearedEveryImage,
    ClearedStage,
    ClearedStageNoMistakes,
    ClearedStageNoUpgrades,
    ClearedStageNoMistakesNoUpgrades,
}

public class Match {
    public int stage;
    public int difficulty;
    public int score;
    public int amountOfTurns;
    public bool hardcore;
    public List<(int turn, int imageID, int amountOfAppearences, float turnDuration, int guess, TurnAction action, int
        remainingLives, float multiplier, int scoreModification)> turnHistory = new ();

    public Match(int stage, int difficulty, bool hardcore) {
        this.stage = stage;
        this.difficulty = difficulty;
        this.hardcore = hardcore;
    }

    public void AddTurn(int imageID, int amountOfAppearences, float turnDuration, int guess, TurnAction action, int
        remainingLives, float multiplier, int scoreModification) {
        turnHistory.Add((turnHistory.Count, imageID, amountOfAppearences, turnDuration, guess,action,remainingLives, multiplier, scoreModification));
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

public enum TurnAction {
    GuessCorrect,
    GuessIncorrect,
    UseClue,
    UseRemove,
    Peek,
}

