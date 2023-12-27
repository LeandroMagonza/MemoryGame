using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class DifficultyButton : ChangeCanvasButton {
    public int difficulty;
    public int stage;
    public AchievementStars stars;
    public ImageSet imageSet;
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
        Debug.Log("Clicked difficultyButton "+stage+" "+difficulty);
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

    public void SetImageSet(ImageSet imageSet)
    {
        this.imageSet = imageSet;
    }

    public void UpdateDifficultyUnlocked() {
        //Debug.Log("Called updatedifficulty in" + (stage, difficulty));
       
        //stage condicion de tener el ultimo nivel del stage anterior pasado
        //GameManager.Instance.userData.GetUserStageData(stage-1, 2) is null ||
        //GameManager.Instance.userData.GetUserStageData(stage-1, 2).achievements.Contains(Achievement.ClearedEveryImage)
        
        // Debug.Log("GameManager.Instance.userData.GetUserStageData(stage, difficulty).HasUnlockedStage() "+GameManager.Instance.userData.GetUserStageData(stage, difficulty).HasUnlockedStage());
        // Debug.Log("GameManager.Instance.userData.GetUserStageData(stage, difficulty - 1) is null "+GameManager.Instance.userData.GetUserStageData(stage, difficulty - 1) is null );
        // Debug.Log("GameManager.Instance.userData.GetUserStageData(stage, difficulty  - 1).achievements.Contains(Achievement.ClearedEveryImage) stage "+stage+" dif "+difficulty+GameManager.Instance.userData.GetUserStageData(stage, difficulty  - 1).achievements.Contains(Achievement.ClearedEveryImage));
        bool conditionStage = GameManager.Instance.userData.GetUserStageData(stage, difficulty).HasUnlockedStage();
        bool conditionDifficulty = GameManager.Instance.userData.GetUserStageData(stage, difficulty - 1) is null ||
                                   GameManager.Instance.userData.GetUserStageData(stage, difficulty - 1).achievements
                                       .Contains(Achievement.ClearedEveryImage);
        if (conditionStage && conditionDifficulty)
        {
            GetComponent<Button>().interactable = true;
        }
        else
        {
            GetComponent<Button>().interactable = false;
        }
    }

    public IEnumerator SetAchievements(List<Achievement> achievements,float delay)
    {
        foreach (var achievement in achievements)
        {
            yield return stars.SetAchievement(achievement, delay);
        }
    }


}