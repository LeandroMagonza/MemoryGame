using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
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
    [SerializeField] public GameObject peekCanvas;
    public void OpenStickerPanel(int stage)
    {

        StageManager.Instance.stickerHolder.transform.GetComponentsInChildren<Sticker>();
        foreach (var stickerID in StageManager.Instance.stages[stage].stickers)
        {
            Sticker display = StickerManager.Instance.GetStickerHolder();
            StickerData stickerData = StickerManager.Instance.GetStickerDataFromSetByStickerID(StageManager.Instance.stages[stage].stickerSet, stickerID);
            display.SetStickerData(stickerData);
            // esto era para cuando tenias que desbloquear los stages, los lockeados eran los stickers que no tenias, pero ahora no se loquean asi que el boton
            // trae todos los stickers siempre
            /*if (stickerData.amountOfDuplicates == 0) {
                display.ConfigureLocked();
            }
            else {*/
            //}
            display.ConfigureForPack();

            display.transform.SetParent(StageManager.Instance.stickerHolder.transform);
            display.transform.localScale = Vector3.one;
        }
        StageManager.Instance.stickerPanel.SetActive(true);
    }
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
        text.color = text.text.Equals("0") ? Color.white : Color.yellow;
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
            button.GetComponent<Image>().color = button.interactable ? Color.white : Color.gray;
        }
    }


}
public class GameCanvas : MonoBehaviour
{
    public ConsumableButtonData[] buttons;
    private GameManager GameManager => GameManager.Instance;
    public static GameCanvas Instance;
    public Transform stickerHolder;
    private GameCanvas()
    {
        if (Instance == null)
            Instance = this;
    }
    private void OnEnable()
    {
        if (GameManager.startScale != Vector3.zero)
            GameManager.stickerDisplay.spriteHolder.transform.localScale = GameManager.startScale;
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
        ResetNumpadUIButtons();
        if (stickerData.matchData == null || stickerData.sticker == null) return;
        //BetterClue
        if (stickerData.matchData.lastClueAppearenceNumber != null)
        {
            CustomDebugger.Log("active turn sticker" + stickerData.matchData.lastClueAppearenceNumber);
            GameManager.numpadButtons[(int)stickerData.matchData.lastClueAppearenceNumber-1]._numberText.color = Color.green;
            for (int i = 0; i < (int)stickerData.matchData.lastClueAppearenceNumber; i++)
            {
                GameManager.numpadButtons[i]._button.interactable = false;
            }
        }
        if (stickerData.matchData.cutNumbers.Count > 0)
        {
            //Cut
            for (int i = 1; i < stickerData.matchData.cutNumbers.Count; i++)
            {
                CustomDebugger.Log($"stickerData.matchData.cutNumbers: {stickerData.matchData.cutNumbers.Count} number: {i} contains: {stickerData.matchData.cutNumbers.Contains(i)}");
                GameManager.numpadButtons[stickerData.matchData.cutNumbers[i]-1]._numberText.color = Color.red;
                GameManager.numpadButtons[stickerData.matchData.cutNumbers[i]-1]._button.interactable = false;
            }
        }
        Debug.Log("BN count: " + stickerData.matchData.blockedNumbers.Count);

        if (stickerData.matchData.blockedNumbers.Count > 0)
        {
            //Block
            for (int i = 0; i < stickerData.matchData.blockedNumbers.Count; i++)
            {
            
                CustomDebugger.Log($"stickerData.matchData.blockedNumbers: {stickerData.matchData.blockedNumbers.Count} number: {i} contains: {stickerData.matchData.blockedNumbers.Contains(i)}");
                GameManager.numpadButtons[stickerData.matchData.blockedNumbers[i]-1]._numberText.color = Color.red;
                GameManager.numpadButtons[stickerData.matchData.blockedNumbers[i]-1]._button.interactable = false;
            }
        }
    }

    public void ResetNumpadUIButtons()
    {
        if (GameManager.numpadButtons.Length > 0)
        {
            for (int i = 0; i < GameManager.numpadButtons.Length; i++)
            {
                if (GameManager.numpadButtons[i].gameObject.activeInHierarchy)
                {
                    GameManager.numpadButtons[i]._button.interactable = true;
                    GameManager.numpadButtons[i]._numberText.color = Color.white;
                }
            }
        }
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

        var turnSticker = USE(ConsumableID.Clue);
        int scoreModification = GameManager.OnCorrectGuess();

        SaveAction(turnSticker, scoreModification,TurnAction.UseClue);
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

        SaveAction(turnSticker, scoreModification,TurnAction.UseClue);
    }
    public void UseRemove()
    {
        var turnSticker = USE(ConsumableID.Remove);
        GameManager.RemoveStickerFromPool();
        SaveAction(turnSticker, 0,TurnAction.UseRemove);

    }
    public void UseCut()
    {
        var turnSticker = USE(ConsumableID.Cut);
        int amountOfAppears = turnSticker.matchData.amountOfAppearences;
        turnSticker.matchData.AddCutEffect(amountOfAppears, GameManager.selectedDifficulty);
        SaveAction(turnSticker, 0,TurnAction.UseCut);
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
