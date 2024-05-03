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
        foreach (var powerButton in buttons) {
            powerButton.SetButtonText();
            powerButton.SetInteractable();

            if (!disableUnavailablePowers) continue; 
            if (
                GameManager.Instance.matchInventory.ContainsKey(powerButton.consumableID)
                &&
                GameManager.Instance.matchInventory[powerButton.consumableID].max < 1
            ) {
                powerButton.gameObject.SetActive(false);
            }
            else {
                powerButton.gameObject.SetActive(true);
            }
        }
    }

    private (StickerData sticker, StickerMatchData matchData) ActivatePower(ConsumableID ID)
    {
        CustomDebugger.Log("USE: " + ID.ToString());
        var turnSticker = GameManager.GetCurrentlySelectedSticker();
        if (!GameManager.matchInventory.ContainsKey(ID) || GameManager.matchInventory[ID].current <= 0) return (turnSticker);
        AudioManager.Instance.PlayClip((buttons[(int)ID].gameClip));
        //anim
        var consumable = GameManager.matchInventory[ID];
        GameManager.matchInventory[ID] = (consumable.current - 1, consumable.max, consumable.initial);
        if (GameManager.matchInventory[ID].current <= 0)
        {
            GameManager.matchInventory[ID] = (0, consumable.max, consumable.initial);
        }
        return (turnSticker);
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

        var turnSticker = ActivatePower(ConsumableID.Clue);
        int scoreModification = GameManager.OnCorrectGuess();

        SaveAction(turnSticker, scoreModification,TurnAction.UseClue);
    }
    public void UseBetterClue()
    {
        CustomDebugger.Log("Better");

        var turnSticker = ActivatePower(ConsumableID.Clue);
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
        var turnSticker = ActivatePower(ConsumableID.Remove);
        GameManager.RemoveStickerFromPool();
        SaveAction(turnSticker, 0,TurnAction.UseRemove);

    }
    public void UseCut()
    {
        CustomDebugger.Log("intetno usar cut");
        var turnSticker = ActivatePower(ConsumableID.Cut);
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
            case ConsumableID.Peek:
                UsePeek();
                break;
            default:
                break;
        }

        gameCanvas.UpdateUI();
    }
}
