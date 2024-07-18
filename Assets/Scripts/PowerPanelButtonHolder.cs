using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerPanelButtonHolder : MonoBehaviour
{
    private GameManager GameManager => GameManager.Instance;
    public PowerButton[] buttons;
    public static PowerPanelButtonHolder Instance;
    public GameCanvas gameCanvas;
    [SerializeField] public GameObject pausePanel;

    private PowerPanelButtonHolder()
    {
        if (Instance == null)
            Instance = this;
    }

    public void SetAllPowerButtonsText(bool disableUnavailablePowers) {
        bool anyPowerActive = false;
        foreach (var powerButton in buttons) {
            powerButton.SetButtonText();
            powerButton.SetInteractable();
            powerButton.markedActive = false;
            if (!disableUnavailablePowers) continue; 
            if (
                GameManager.Instance.matchInventory.ContainsKey(powerButton.consumableID)
                &&
                GameManager.Instance.matchInventory[powerButton.consumableID].max > 0
            ) {
                powerButton.gameObject.SetActive(true);
                anyPowerActive = true;
            }
            else {
                powerButton.gameObject.SetActive(false);
            }
        }

        //gameObject.SetActive(anyPowerActive);
    }

 

    private void SaveAction((StickerData sticker, StickerMatchData matchData) turnSticker, int scoreModification,TurnAction turnAction)
    {
        StartCoroutine(GameManager.FinishProcessingTurnAction(
        turnSticker.matchData.amountOfAppearences,
        turnAction,
        scoreModification,
        turnSticker.sticker,
        turnSticker.matchData,
        false
        ));
    }
    public void UseClue()
    {
        CustomDebugger.Log("Common");

        ConsumableID consumableID = ConsumableID.Clue;
        var turnSticker = GameManager.ActivatePower(consumableID,(buttons[(int)consumableID].gameClip));
        int scoreModification = GameManager.OnCorrectGuess();

        SaveAction(turnSticker, scoreModification,TurnAction.UseClue);
    }
    public void UseBetterClue()
    {
        CustomDebugger.Log("Better");

        ConsumableID consumableID = ConsumableID.Clue;
        var turnSticker = GameManager.ActivatePower(consumableID,(buttons[(int)consumableID].gameClip));
        int amountOfAppears = GameManager.GetCurrentlySelectedSticker().matchData.amountOfAppearences;
        Debug.Log("amount of appears: " + amountOfAppears);
        // por que crea uno nuevo?
        turnSticker.matchData.AddBetterClueEffect(amountOfAppears);
        Debug.Log("amount of appears: " + amountOfAppears);
        int scoreModification = GameManager.OnCorrectGuess();

        SaveAction(turnSticker, scoreModification,TurnAction.UseClue);
    }
    public void UseRemove()
    {
        ConsumableID consumableID = ConsumableID.Remove;
        var turnSticker = GameManager.ActivatePower(consumableID,(buttons[(int)consumableID].gameClip));
        GameManager.RemoveStickerFromPool();
        SaveAction(turnSticker, 0,TurnAction.UseRemove);

    }
    public void UseCut()
    {
        CustomDebugger.Log("intetno usar cut");
        ConsumableID consumableID = ConsumableID.Cut;
        var turnSticker = GameManager.ActivatePower(consumableID,(buttons[(int)consumableID].gameClip));
        int amountOfAppearences = turnSticker.matchData.amountOfAppearences;
        int amountOfCuts = 1;
        if (GameManager.userData.unlockedUpgrades.ContainsKey(UpgradeID.BetterCut)) {
            amountOfCuts += GameManager.userData.unlockedUpgrades[UpgradeID.BetterCut];
        }
        
        turnSticker.matchData.AddCutEffect(amountOfAppearences, GameManager.selectedDifficulty,amountOfCuts);
   }
    public void UsePeek()
    {

    }

    public void UsePower(ConsumableID itemID)
    {
        switch (itemID)
        {
            case ConsumableID.Clue:
                if (GameManager.userData.unlockedUpgrades.ContainsKey(UpgradeID.BetterClue) && GameManager.userData.unlockedUpgrades[UpgradeID.BetterClue] > 0)
                    UseBetterClue();
                else
                    UseClue();
                break;
            case ConsumableID.Remove:
                UseRemove();
                break;
            case ConsumableID.Cut:
                UseCut();
                break;
            // case ConsumableID.Peek:
            //     UsePeek();
            //     break;
            case ConsumableID.Highlight:
                ToggleHighlight();
                break; 
            case ConsumableID.Bomb:
                ToggleBomb();
                break;
            default:
                break;
        }

        gameCanvas.UpdateUI();
    }

    private void ToggleHighlight() {
        ConsumableID consumableID = ConsumableID.Highlight;
        GameManager.highlightActive = !GameManager.highlightActive;
        if (GameManager.highlightActive) {
            AudioManager.Instance.PlayClip(buttons[(int)consumableID].gameClip,1);
            GameManager.bombActive = false;
        }
        else {
            AudioManager.Instance.PlayClip(buttons[(int)consumableID].gameClip,.8f);
        }

        SetAllPowerButtonsText(false);
        CustomDebugger.Log(GameManager.highlightActive+" HighlightActive");
        buttons[(int)consumableID].MarkActive(GameManager.highlightActive);
    }   
    private void ToggleBomb() {
        ConsumableID consumableID = ConsumableID.Bomb;
        GameManager.bombActive = !GameManager.bombActive;
        if (GameManager.bombActive) {
            AudioManager.Instance.PlayClip(buttons[(int)consumableID].gameClip,1);
            GameManager.highlightActive = false;
        }
        else {
            AudioManager.Instance.PlayClip(buttons[(int)consumableID].gameClip,.8f);
        }

        SetAllPowerButtonsText(false);
        CustomDebugger.Log(GameManager.bombActive+" HighlightActive");
        buttons[(int)consumableID].MarkActive(GameManager.bombActive);
    }

    public PowerButton GetPowerButton(ConsumableID consumableID) {
        foreach (var VARIABLE in buttons) {
            if(VARIABLE.consumableID == consumableID) return VARIABLE;
        }
        return null;
    }
}
