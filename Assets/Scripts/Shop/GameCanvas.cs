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
    [SerializeField] public GameClip gameClip;

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
        Debug.Log("BN count: " + stickerData.matchData.blockedNumbers.Count);

        if (stickerData.matchData.blockedNumbers.Count > 0)
        {
            //Block
            for (int i = 0; i < stickerData.matchData.blockedNumbers.Count; i++)
            {
            
                CustomDebugger.Log($"stickerData.matchData.blockedNumbers: {stickerData.matchData.blockedNumbers.Count} number: {i} contains: {stickerData.matchData.blockedNumbers.Contains(i)}");
                numpadButtons[stickerData.matchData.blockedNumbers[i]].GetComponentInChildren<TextMeshProUGUI>().color = Color.red;
                numpadButtons[stickerData.matchData.blockedNumbers[i]].interactable = false;
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

    private void SaveAction((StickerData sticker, StickerMatchData matchData) turnSticker, int scoreModification)
    {
        StartCoroutine(GameManager.FinishProcessingTurnAction(
        turnSticker.matchData.amountOfAppearences,
        TurnAction.UseClue,
        scoreModification,
        turnSticker.sticker,
        turnSticker.matchData
        ));
    }
    public void UseClue()
    {
        CustomDebugger.Log("Common");

        var turnSticker = USE(ConsumableID.Clue);
        int scoreModification = GameManager.OnCorrectGuess();

        SaveAction(turnSticker, scoreModification);
    }
    public void UseBetterClue()
    {
        CustomDebugger.Log("Better");

        var turnSticker = USE(ConsumableID.Clue);
        int amountOfAppears = GameManager.GetCurrentlySelectedSticker().matchData.amountOfAppearences;
        Debug.Log("amount of appears: " + amountOfAppears);
        // por que crea uno nuevo?
        turnSticker.matchData.AddBetterClueEffect(amountOfAppears);
        Debug.Log("amount of appears: " + amountOfAppears);
        int scoreModification = GameManager.OnCorrectGuess();

        SaveAction(turnSticker, scoreModification);
    }
    public void UseRemove()
    {
        var turnSticker = USE(ConsumableID.Remove);
        GameManager.RemoveStickerFromPool();
        GameManager.NextTurn();

    }
    public void UseCut()
    {
        var turnSticker = USE(ConsumableID.Cut);
        int amountOfAppears = turnSticker.matchData.amountOfAppearences;
        turnSticker.matchData.AddCutEffect(amountOfAppears, GameManager.selectedDifficulty);

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
        UpdateUI();
    }
}
