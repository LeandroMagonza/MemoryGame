using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class DifficultyButton : MonoBehaviour {
    public int difficulty;
    public int stage;

    public TextMeshProUGUI scoreText;
    // Start is called before the first frame update
    void Start()
    {
        gameObject.name = "ButtonDifficulty S" + stage+" D"+difficulty;
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    public void OnClick()
    {
        Debug.Log("Clicked difficultyButton "+stage+" "+difficulty);
        GameManager.Instance.SetStageAndDifficulty(stage, difficulty);
    }

    public void SetScore(int score) {
        scoreText.text = score.ToString();
    }

    public void SetStage(int stage) {
        this.stage = stage;
        gameObject.name = "ButtonDifficulty S" + stage+" D"+difficulty;
    }
}