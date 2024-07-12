using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayerLevelDisplayButton : ChangeCanvasButton
{
    public TextMeshProUGUI levelDisplay;
    public PendingUpgradeNotification pendingUpgradeNotification;
    public int playerLevel => PersistanceManager.Instance.userData.playerLevel;
    public UserData userData => PersistanceManager.Instance.userData;
    public SelectUpgradePanel selectUpgradePanel;

    public TextMeshProUGUI currentExpTextPercentage;
    public Image currentExpBar;
    public Image justGainedExpBar;

    public static float barFillSpeed = 0.02f;
    
    private int currentExpDisplayed = 1;
    public override void Start() {
        levelDisplay.text = playerLevel.ToString();
        UpdateLevelDisplay();
        base.Start();
        PlayerLevelManager.Instance.OnPlayerGainExp += BeginUpdatingLevelDisplay;
    }


    public void BeginUpdatingLevelDisplay(int level = -1)
    {
        //level no es usado pero viene en el evento, por eso esta como parametro
        /*
        foreach (var VARIABLE in userData.unlockedUpgrades) {
            CustomDebugger.Log("counting levels: "+VARIABLE.Key + VARIABLE.Value);
        }
        CustomDebugger.Log("player level: "+playerLevel.ToString());
        */
        if (isActiveAndEnabled) {
            StartCoroutine(UpdateLevelDisplayWithAnimation());
        }
        else {
            UpdateLevelDisplay();
        }
    }

    public void UpdateLevelDisplay() {
        var finalLevelProgressPercentage = CalculateLevelProgressPercentage();

        SetLevelAndProgressBar(finalLevelProgressPercentage);
    }

    private void SetLevelAndProgressBar(float finalLevelProgressPercentage) {
        currentExpBar.fillAmount = finalLevelProgressPercentage;
        justGainedExpBar.fillAmount = finalLevelProgressPercentage;
        SetCurrentTextPercentage(currentExpBar.fillAmount);

        levelDisplay.text = playerLevel.ToString();
        int amountOfPendingUpgrades = userData.AmountOfPendingUpgrades();
        if (amountOfPendingUpgrades > 0) {
            GetComponent<Button>().interactable = true;
            pendingUpgradeNotification.gameObject.SetActive(true);
            pendingUpgradeNotification.SetAmountOfPendingUpgrades(amountOfPendingUpgrades);
        }
        else {
            GetComponent<Button>().interactable = false;
            pendingUpgradeNotification.gameObject.SetActive(false);
        }

        currentExpDisplayed = userData.experiencePoints;
    }

    private IEnumerator UpdateLevelDisplayWithAnimation() {

        var finalLevelProgressPercentage = CalculateLevelProgressPercentage();
        
        yield return StartCoroutine(AnimateExperienceGained(finalLevelProgressPercentage));
        
        SetLevelAndProgressBar(finalLevelProgressPercentage);
    }

    private void SetCurrentTextPercentage(float fillAmount) {
        currentExpTextPercentage.text = ((int)(fillAmount * 100)).ToString()+"%";
    }

    private IEnumerator AnimateExperienceGained(float finalLevelProgressPercentage) {
        int currentLevelDisplayed = userData.ExperiencePointsToLevelUp(currentExpDisplayed).calculatedLv;
        justGainedExpBar.fillAmount = currentExpBar.fillAmount;

        GameClip[] gameClips = new GameClip[3] {
            GameClip.getAchievementStar0,
            GameClip.getAchievementStar1,
            GameClip.getAchievementStar2
        };
        
        int gameClipIndex = 0;
        while (currentLevelDisplayed < userData.playerLevel) {
            //sube la barra hasta el final y animacion de subida de level
            while (justGainedExpBar.fillAmount < 1) {
                justGainedExpBar.fillAmount += barFillSpeed;
                SetCurrentTextPercentage(justGainedExpBar.fillAmount);
                gameClipIndex = userData.playerLevel - currentLevelDisplayed;
                if (gameClips.Length > gameClipIndex) {
                    AudioManager.Instance.PlayClip(gameClips[gameClipIndex]);
                }
                else {
                    AudioManager.Instance.PlayClip(gameClips[^1]);
                }
                yield return new WaitForSeconds(.1f);
                
            }

            
            yield return new WaitForSeconds(AudioManager.Instance.PlayClip(GameClip.highScore)-2f);
            
            currentLevelDisplayed++;
            levelDisplay.text = currentLevelDisplayed.ToString();
            currentExpBar.fillAmount = 0;
            justGainedExpBar.fillAmount = 0;
        }

        while (justGainedExpBar.fillAmount < finalLevelProgressPercentage) {
            justGainedExpBar.fillAmount += barFillSpeed;
            if (justGainedExpBar.fillAmount > finalLevelProgressPercentage) {
                justGainedExpBar.fillAmount = finalLevelProgressPercentage;
            }
            SetCurrentTextPercentage(justGainedExpBar.fillAmount);
            if (gameClips.Length > gameClipIndex) {
                AudioManager.Instance.PlayClip(gameClips[gameClipIndex]);
            }
            else {
                AudioManager.Instance.PlayClip(gameClips[^1]);
            }
            yield return new WaitForSeconds(.1f);
            
        }

        yield return new WaitForSeconds(.5f);
    }

    private float CalculateLevelProgressPercentage(int experiencePoints = 0) {
        if (experiencePoints == 0) { experiencePoints = userData.experiencePoints; }

        (int currentLevel,int currentRemainingExp) = userData.ExperiencePointsToLevelUp(experiencePoints);
        int currentLevelAbsoluteExp = userData.ExperienceAccordingToLevel(currentLevel);
        int nextLevelAbsoluteExp = userData.ExperienceAccordingToLevel(currentLevel+1);
        int currentLevelTotalExp = nextLevelAbsoluteExp - currentLevelAbsoluteExp;
        
        float finalRemainingExpToDisplay = (float)currentRemainingExp / currentLevelTotalExp;
        
        return finalRemainingExpToDisplay;
    }

    public override void OnClick() {
        if (selectUpgradePanel == null || canvasToSet == CanvasName.NO_CANVAS) {
            base.OnClick();
        }
        else {
            selectUpgradePanel.GetRandomUpgradesForPlayerSelection(playerLevel);
        }
    }
    void OnDestroy()
    {
        // Es importante desuscribirse para evitar errores si el objeto se destruye
        if (PlayerLevelManager.Instance != null)
        {
            PlayerLevelManager.Instance.OnPlayerGainExp -= BeginUpdatingLevelDisplay;
        }
    }
}
