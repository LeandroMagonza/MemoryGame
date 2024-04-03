using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameCanvasHeader : MonoBehaviour
{
    public LifeCounter lifeCounter;
    public TextMeshProUGUI timerText; 
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText;
    public TextMeshProUGUI comboBonusText;
    public RemainingBarController barController;

    public void PauseButtonPressed() {
        GameManager.Instance.TogglePause();
    }
}
