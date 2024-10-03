using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StreakDisplay : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI numberText;
    [SerializeField] public TextMeshProUGUI timerText; // Nuevo elemento para el texto del temporizador
    [SerializeField] public Image image;
    [SerializeField] public Color playedColor; // Color cuando la partida del día ha sido jugada
    [SerializeField] public Color notPlayedColor; // Color cuando la partida del día no ha sido jugada
    [SerializeField] public StreakExpBonusDisplay streakBonusDisplay; // Color cuando la partida del día no ha sido jugada
    private bool initialized;

    public void UpdateStreakText(int streakAmount)
    {
        if (streakAmount < 1) {
            image.gameObject.SetActive(false);
            streakBonusDisplay.gameObject.SetActive(false);
        }
        else
        {
            image.gameObject.SetActive(true);
            streakBonusDisplay.gameObject.SetActive(true);
        }

        initialized = true;
        numberText.text = streakAmount.ToString();
    }

    public void UpdateTimerText(bool hasPlayedToday)
    {
        if (!initialized) return;
        DateTime endOfDay = DateTime.Today.AddDays(1).AddSeconds(-1);
        string timeRemaining = ItemHelper.FormatTimeRemaining(endOfDay);
        timerText.text = timeRemaining;

        if (hasPlayedToday)
        {
            timerText.color = playedColor;
        }
        else
        {
            timerText.color = notPlayedColor;
        }
    }
}