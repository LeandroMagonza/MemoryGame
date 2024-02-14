using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
public class DifficultyButton : ChangeCanvasButton {
    public int difficulty;
    public int stage;
    public AchievementStars stars;
    public TextMeshProUGUI scoreText;
    // Start is called before the first frame update
    public override void Start()
    {
        gameObject.name = "ButtonDifficulty S" + stage+" D"+difficulty;
        GetComponent<Button>().onClick.AddListener(OnClick);
        this.canvasToSet = GameManager.Instance.GetGameCanvas();
    }

    public override void OnClick()
    {
        CustomDebugger.Log("Clicked difficultyButton "+stage+" "+difficulty);
        StageManager.Instance.SetStageAndDifficulty(stage, difficulty);
        base.OnClick();
        GameManager.Instance.Reset();
    }

    public void SetScore(int score) {
        scoreText.text = score.ToString();
    }

    public void SetStage(int stage) {
        this.stage = stage;
        gameObject.name = "ButtonDifficulty S" + stage+" D"+difficulty;
    }

    public void UpdateDifficultyUnlocked() {
        //CustomDebugger.Log("Called updatedifficulty in" + (stage, difficulty));
       
        //stage condicion de tener el ultimo nivel del stage anterior pasado
        //GameManager.Instance.userData.GetUserStageData(stage-1, 2) is null ||
        //GameManager.Instance.userData.GetUserStageData(stage-1, 2).achievements.Contains(Achievement.ClearedEveryImage)
        
        /*CustomDebugger.Log("GameManager.Instance.userData.GetUserStageData(stage, difficulty).HasUnlockedStage() stage "+stage+" dif "+difficulty+" "+GameManager.Instance.userData.GetUserStageData(stage, difficulty).HasUnlockedStage());
        CustomDebugger.Log("GameManager.Instance.userData.GetUserStageData(stage, difficulty - 1) is null stage "+stage+" dif "+difficulty+" "+GameManager.Instance.userData.GetUserStageData(stage, difficulty - 1) is null );
        CustomDebugger.Log("GameManager.Instance.userData.GetUserStageData(stage, difficulty  - 1).achievements.Contains(Achievement.ClearedEveryImage) stage "+stage+" dif "+
                  (difficulty - 1)+" "+GameManager.Instance.userData.GetUserStageData(stage, difficulty  - 1).achievements.Contains(Achievement.ClearedEveryImage));*/
        CustomDebugger.Log("button name "+ name);
        // por que no encuentra el stage en userdata
        bool conditionStage = GameManager.Instance.userData.GetUserStageData(stage, difficulty).HasUnlockedStage();
        bool conditionDifficulty = GameManager.Instance.userData.GetUserStageData(stage, difficulty - 1) is null ||
                                   GameManager.Instance.userData.GetUserStageData(stage, difficulty - 1).achievements
                                       .Contains(Achievement.BarelyClearedStage);
        CustomDebugger.Log("conditionStage " + conditionStage);
        CustomDebugger.Log("conditionDifficulty " + conditionDifficulty);
        if (conditionStage && conditionDifficulty)
        {
            GetComponent<Button>().interactable = true;
        }
        else
        {
            GetComponent<Button>().interactable = false;
        }

        stars.SetAchievements(GameManager.Instance.userData.GetUserStageData(stage, difficulty).achievements);
    }


}