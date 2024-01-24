using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct ConsumableButtonData
{
    [SerializeField] public string name;
    [SerializeField] public ConsumableID ID;
    [SerializeField] public Button button;
    [SerializeField] public TextMeshProUGUI text;
    [SerializeField] public AudioClip audioClip;

    public void SetButtonText(ConsumableID consumableID)
    {
        if (text != null)
        {

            if (GameManager.Instance.matchInventory.ContainsKey(consumableID))
            {
                text.text = GameManager.Instance.matchInventory[consumableID].current.ToString();
            }
            else
            {
                CustomDebugger.Log("not implemented");
                text.text = "0";
            }
        }
        else
        {
        }

    }
    public void SetInteractable(ConsumableID consumableID)
    {
        if (button != null)
        {
            if (GameManager.Instance.matchInventory.ContainsKey(consumableID))
            {

                button.interactable = GameManager.Instance.matchInventory[consumableID].current > 0;
            }
            else
            {
                button.interactable = false;
            }
        }
    }
}
public class GameCanvas : MonoBehaviour
{
    public ConsumableButtonData[] buttons;
    private GameManager GameManager => GameManager.Instance;
    public static GameCanvas Instance;
    private Dictionary<int, Button> numpadButtons = new Dictionary<int, Button>();
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }
    private void OnEnable()
    {
        GameManager.SetMatchInventory();
        AssignNumpadButtons(GameManager.numpadRow0, GameManager.numpadRow1, GameManager.numpadRow2);
        UpdateUI();
    }

    public void UpdateUI()
    {
        SetButtonText();
    }

    private void SetButtonText()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            for (int j = 0; j < Enum.GetNames(typeof(ConsumableID)).Length; j++)
            {
                if (buttons[i].ID == (ConsumableID)j)
                {
                    buttons[i].SetButtonText((ConsumableID)j);
                    buttons[i].SetInteractable((ConsumableID)j);
                }
            }
        }
        SetNumpadButtons(GameManager.GetCurrentlySelectedSticker());
    }


    private void SetNumpadButtons((StickerData sticker, StickerMatchData matchData) stickerData)
    {
        ResetNumUIButtons();
        if (stickerData.matchData == null || stickerData.sticker == null) return;
        //BetterClue
        if (stickerData.matchData.lastClueAppearenceNumber != null)
        {
            CustomDebugger.Log("active turn sticker" + stickerData.matchData.lastClueAppearenceNumber);
            numpadButtons[(int)stickerData.matchData.lastClueAppearenceNumber].GetComponentInChildren<TextMeshProUGUI>().color = Color.green;
            for (int i = 1; i <= (int)stickerData.matchData.lastClueAppearenceNumber; i++)
            {
                numpadButtons[i].interactable = false;
            }
        }
        if (stickerData.matchData.cutNumbers.Count > 0)
        {
            //Cut
            for (int i = 1; i < stickerData.matchData.cutNumbers.Count; i++)
            {
                CustomDebugger.Log($"stickerData.matchData.cutNumbers: {stickerData.matchData.cutNumbers.Count} number: {i} contains: {stickerData.matchData.cutNumbers.Contains(i)}");
                numpadButtons[stickerData.matchData.cutNumbers[i]].GetComponentInChildren<TextMeshProUGUI>().color = Color.red;
                numpadButtons[stickerData.matchData.cutNumbers[i]].interactable = false;
            }
        }
    }

    public void ResetNumUIButtons()
    {
        if (numpadButtons.Count > 0)
        {
            for (int i = 1; i <= numpadButtons.Count; i++)
            {
                numpadButtons[i].interactable = true;
                numpadButtons[i].GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
            }
        }
    }

    private void AssignNumpadButtons(params GameObject[] rows)
    {
        CustomDebugger.Log("NumPAD_ROW_Buttons Lenght: " + rows.Length);
        for (int i = 0; i < rows.Length; i++)
        {
            for (int j = 0; j < rows[i].transform.childCount; j++)
            {
                Button button = rows[i].transform.GetChild(j).GetComponent<Button>();
                NumpadButton numpadButton = button.GetComponent<NumpadButton>();
                if (!numpadButtons.ContainsKey(numpadButton.number))
                {
                    numpadButtons.Add(numpadButton.number, button);
                }
            }
        }
        CustomDebugger.Log("NumPAD_Buttons Count: " + numpadButtons.Count);
    }

    private (StickerData sticker, StickerMatchData matchData) USE(ConsumableID ID)
    {
        CustomDebugger.Log("USE: " + ID.ToString());
        var turnSticker = GameManager.GetCurrentlySelectedSticker();
        if (!GameManager.matchInventory.ContainsKey(ID) || GameManager.matchInventory[ID].current <= 0) return (turnSticker);
        GameManager.audioSource.PlayOneShot(buttons[(int)ID].audioClip);
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
        CustomDebugger.Log("Common");

        var turnSticker = USE(ConsumableID.Clue);
        int scoreModification = GameManager.OnCorrectGuess();
        UpdateUI();
        SaveAction(turnSticker, scoreModification);
    }
    [ContextMenu("USE Better Clue")]
    public void UseBetterClue()
    {
        CustomDebugger.Log("Better");

        var turnSticker = USE(ConsumableID.Clue);
        int amountOfAppears = GameManager.GetCurrentlySelectedSticker().sticker.amountOfAppearences;
        // por que crea uno nuevo?
        StickerMatchData stickerData = new StickerMatchData();
        stickerData.AddBetterClueEffect(amountOfAppears);
        GameManager.currentlyInGameStickers[GameManager.GetCurrentlySelectedSticker().sticker] = stickerData;
        int scoreModification = GameManager.OnCorrectGuess();
        UpdateUI();
        SaveAction(turnSticker, scoreModification);
    }
    public void UseRemove()
    {
        var turnSticker = USE(ConsumableID.Remove);
        GameManager.RemoveStickerFromPool();
        GameManager.NextTurn();
        UpdateUI();
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

    public void UseItem(int ID)
    {
        ConsumableID itemID = (ConsumableID)ID;
        switch (itemID)
        {
            case ConsumableID.Clue:
                CustomDebugger.Log(GameManager.userData.upgrades.ContainsKey(UpgradeID.BetterClue));
//                CustomDebugger.Log(GameManager.userData.upgrades[UpgradeID.BetterClue]);
                if (GameManager.userData.upgrades.ContainsKey(UpgradeID.BetterClue) && GameManager.userData.upgrades[UpgradeID.BetterClue] > 0)
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
    }
}
