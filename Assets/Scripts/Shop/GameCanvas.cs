using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameCanvas : MonoBehaviour
{
    [Header("Consumable Settings")]
    [SerializeField] private Button buttonClue;
    [SerializeField] private Button buttonRemove;
    [SerializeField] private Button buttonCut;
    [SerializeField] private Button buttonPeek;

    [SerializeField] private TextMeshProUGUI buttonTextClue;
    [SerializeField] private TextMeshProUGUI buttonTextRemove;
    [SerializeField] private TextMeshProUGUI buttonTextPeek;
    [SerializeField] private TextMeshProUGUI buttonTextCut;

    [SerializeField] private AudioClip buttonClueAudioClip;
    [SerializeField] private AudioClip buttonRemoveAudioClip;
    [SerializeField] private AudioClip buttonCutAudioClip;
    [SerializeField] private AudioClip buttonPeekAudioClip;

    private GameManager GameManager => GameManager.Instance;

    private Dictionary <int, Button> numpadButtons = new Dictionary<int, Button>();
    private void OnEnable()
    {
        GameManager.SetMatchInventory();
        AssignNumpadButtons(GameManager.numpadRow0, GameManager.numpadRow1, GameManager.numpadRow2);
        UpdateUI();
    }

    private void UpdateUI()
    {
        SetButtonText();
        SetInteractable();
    }

    private void SetButtonText()
    {
        if (buttonTextClue != null)
        {
            if (GameManager.matchInventory.ContainsKey(ConsumableID.Clue))
            {
                buttonTextClue.text = GameManager.matchInventory[ConsumableID.Clue].current.ToString();
            }
            else
            {
                buttonTextClue.text = "0";
            }
        }
        if (buttonTextPeek != null)
        {
            if (GameManager.matchInventory.ContainsKey(ConsumableID.Peek))
            {
                buttonTextPeek.text = GameManager.matchInventory[ConsumableID.Peek].current.ToString();
            }
            else
            {
                buttonTextPeek.text = "0";
            }
        }
        if (buttonTextCut != null)
        {
            if (GameManager.matchInventory.ContainsKey(ConsumableID.Cut))
            { 
                buttonTextCut.text = GameManager.matchInventory[ConsumableID.Cut].current.ToString(); 
            }
            else
            {
                buttonTextCut.text = "0";
            }
        }
        if (buttonTextRemove != null)
        {
            if (GameManager.matchInventory.ContainsKey(ConsumableID.Remove))
            {
                buttonTextRemove.text = GameManager.matchInventory[ConsumableID.Remove].current.ToString();
            }
            else
            {
                buttonTextRemove.text = "0";
            }
        }
        SetNumpadButtons(GameManager.GetCurrentlySelectedSticker());
    }
    private void SetInteractable()
    {
        if (buttonClue != null)
        {
            if (GameManager.matchInventory.ContainsKey(ConsumableID.Clue))
            {

                buttonClue.interactable = GameManager.matchInventory[ConsumableID.Clue].current > 0;
            }
            else
            {
                buttonClue.interactable = false;
            }
        }
        if (buttonRemove != null)
        {
            if (GameManager.matchInventory.ContainsKey(ConsumableID.Remove))
            {

                buttonRemove.interactable = GameManager.matchInventory[ConsumableID.Remove].current > 0;
            }
            else
            {
                buttonRemove.interactable = false;
            }
        }
        if (buttonCut != null)
        {
            if (GameManager.matchInventory.ContainsKey(ConsumableID.Cut))
            {
                buttonCut.interactable = GameManager.matchInventory[ConsumableID.Cut].current > 0;
            }
            else
            {
                buttonCut.interactable = false;
            }
        }
        if (buttonPeek != null)
        {
            if (GameManager.matchInventory.ContainsKey(ConsumableID.Peek))
            {
                buttonPeek.interactable = GameManager.matchInventory[ConsumableID.Peek].current > 0;
            }
            else
            {
                buttonPeek.interactable = false;
            }
        }
    }

    private void SetNumpadButtons((StickerData sticker, StickerMatchData matchData) stickerData)
    {
        ResetNumUIButtons();
        
        //BetterClue
        if (stickerData.matchData.lastClueAppearenceNumber != null)
        {
            CustomDebugger.Log("active turn sticker" + stickerData.matchData.lastClueAppearenceNumber);
            numpadButtons[(int)stickerData.matchData.lastClueAppearenceNumber].GetComponentInChildren<TextMeshProUGUI>().color = Color.green;
            for (int i = 0; i <= (int)stickerData.matchData.lastClueAppearenceNumber ; i++)
            {
                numpadButtons[i].interactable = false;
            }
        }
        //Cut
        foreach (var i in stickerData.matchData.cutNumbers)
        {
            numpadButtons[i].GetComponentInChildren<TextMeshProUGUI>().color = Color.red;
            numpadButtons[i].interactable = false;
        }


    }

    public void ResetNumUIButtons()
    {
        foreach (int button in numpadButtons.Keys)
        {
            numpadButtons[button].interactable = true;
            numpadButtons[button].GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
        }
    }

    private void AssignNumpadButtons(params GameObject[] rows)
    {
        Debug.Log("NumPAD_ROW_Buttons Lenght: " + rows.Length);
        for (int i = 0; i < rows.Length; i++)
        {
            for (int j = 0; j< rows[i].transform.childCount; j++)
            {
                Button button = rows[i].transform.GetChild(j).GetComponent<Button>();
                NumpadButton numpadButton = button.GetComponent<NumpadButton>();
                if (!numpadButtons.ContainsKey(numpadButton.number))
                {
                    numpadButtons.Add(numpadButton.number, button);
                }
            }
        }
        Debug.Log("NumPAD_Buttons Count: "+ numpadButtons.Count);
    }

    private (StickerData sticker, StickerMatchData matchData) USE(ConsumableID ID)
    {
        Debug.Log("USE: "+  ID.ToString());
        var turnSticker = GameManager.GetCurrentlySelectedSticker();
        if (!GameManager.matchInventory.ContainsKey(ID) || GameManager.matchInventory[ID].current <= 0) return (turnSticker);
        GameManager.audioSource.PlayOneShot(buttonClueAudioClip);
        //anim
        var consumable = GameManager.matchInventory[ID];
        GameManager.matchInventory[ID] = (consumable.current - 1, consumable.max, consumable.initial);
        if (GameManager.matchInventory[ID].current <= 0)
        {
            GameManager.matchInventory[ID] = (0, consumable.max, consumable.initial);
        }
        return (turnSticker);
    }

    private void SaveAction((StickerData sticker, StickerMatchData matchData) turnSticker, int scoreModification)
    {
        StartCoroutine(GameManager.FinishProcessingTurnAction(
        turnSticker.sticker.amountOfAppearences,
        TurnAction.UseClue,
        scoreModification,
        turnSticker.sticker
        ));
    }
    public void UseClue()
    {
        var turnSticker = USE(ConsumableID.Clue);
        UpdateUI();
        int scoreModification = GameManager.OnCorrectGuess();
        SaveAction(turnSticker, scoreModification);
    }
    [ContextMenu("USE Better Clue")]
    public void UseBetterClue()
    {
        var turnSticker = USE(ConsumableID.Clue);
        int amountOfAppears = GameManager.GetCurrentlySelectedSticker().sticker.amountOfAppearences;
        // por que crea uno nuevo?
        StickerMatchData stickerData = new StickerMatchData();
        stickerData.AddBetterClueEffect(amountOfAppears);
        GameManager.currentlyInGameStickers[GameManager.GetCurrentlySelectedSticker().sticker] = stickerData;
        UpdateUI();
        int scoreModification = GameManager.OnCorrectGuess();
        SaveAction(turnSticker, scoreModification);
    }
    public void UseRemove()
    {
        var turnSticker = USE(ConsumableID.Remove);
        UpdateUI();
        GameManager.RemoveStickerFromPool();
        GameManager.NextTurn();
    }
    [ContextMenu("USECUT")]
    public void UseCut()
    {
        var turnSticker = USE(ConsumableID.Cut);
        int amountOfAppears = GameManager.GetCurrentlySelectedSticker().sticker.amountOfAppearences;
        StickerMatchData stickerData = new StickerMatchData();
        stickerData.AddCutEffect(amountOfAppears, GameManager.selectedDifficulty);
        GameManager.currentlyInGameStickers[GameManager.GetCurrentlySelectedSticker().sticker] = stickerData;
        UpdateUI();
    }
    public void UsePeek()
    {
    }

    public void UseItem(ConsumableID itemId)
    {
        switch (itemId)
        {
            case ConsumableID.Clue:
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
    }
}
