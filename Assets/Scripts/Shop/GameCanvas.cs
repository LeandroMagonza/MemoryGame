using System;
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
    private void OnEnable()
    {
        GameManager.Instance.SetMatchInventory();
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
            if (GameManager.Instance.matchInventory.ContainsKey(ConsumableID.Clue))
            {
                Debug.Log("Contains Clue");
                buttonTextClue.text = GameManager.Instance.matchInventory[ConsumableID.Clue].ToString();
            }
            else
            {
                buttonTextClue.text = "0";
            }
        }
        if (buttonTextPeek != null)
        {
            if (GameManager.Instance.matchInventory.ContainsKey(ConsumableID.Peek))
            {
                buttonTextPeek.text = GameManager.Instance.matchInventory[ConsumableID.Peek].ToString();
            }
            else
            {
                buttonTextPeek.text = "0";
            }
        }
        if (buttonTextCut != null)
        {
            if (GameManager.Instance.matchInventory.ContainsKey(ConsumableID.Cut))
            { 
                buttonTextCut.text = GameManager.Instance.matchInventory[ConsumableID.Cut].ToString(); 
            }
            else
            {
                buttonTextCut.text = "0";
            }
        }
        if (buttonTextRemove != null)
        {
            if (GameManager.Instance.matchInventory.ContainsKey(ConsumableID.Remove))
            {
                buttonTextRemove.text = GameManager.Instance.matchInventory[ConsumableID.Remove].ToString();
            }
            else
            {
                buttonTextRemove.text = "0";
            }
        }
    }
    private void SetInteractable()
    {
        if (buttonClue != null)
        {
            if (GameManager.Instance.matchInventory.ContainsKey(ConsumableID.Clue))
            {

                buttonClue.interactable = GameManager.Instance.matchInventory[ConsumableID.Clue] > 0;
            }
            else
            {
                buttonClue.interactable = false;
            }
        }
        if (buttonRemove != null)
        {
            if (GameManager.Instance.matchInventory.ContainsKey(ConsumableID.Remove))
            {

                buttonRemove.interactable = GameManager.Instance.matchInventory[ConsumableID.Remove] > 0;
            }
            else
            {
                buttonRemove.interactable = false;
            }
        }
        if (buttonCut != null)
        {
            if (GameManager.Instance.matchInventory.ContainsKey(ConsumableID.Cut))
            {
                buttonCut.interactable = GameManager.Instance.matchInventory[ConsumableID.Cut] > 0;
            }
            else
            {
                buttonCut.interactable = false;
            }
        }
        if (buttonPeek != null)
        {
            if (GameManager.Instance.matchInventory.ContainsKey(ConsumableID.Peek))
            {
                buttonPeek.interactable = GameManager.Instance.matchInventory[ConsumableID.Peek] > 0;
            }
            else
            {
                buttonPeek.interactable = false;
            }
        }
    }
    public void UseClue()
    {
        Debug.Log("USE CLUE");
        var turnSticker = GameManager.Instance.GetCurrentlySelectedSticker();
        if (!GameManager.Instance.matchInventory.ContainsKey(ConsumableID.Clue) || GameManager.Instance.matchInventory[ConsumableID.Clue] <= 0) return;
        GameManager.Instance.audioSource.PlayOneShot(buttonClueAudioClip);
        //anim
        GameManager.Instance.matchInventory[ConsumableID.Clue]--;
        if (GameManager.Instance.matchInventory[ConsumableID.Clue] <= 0)
        {
            GameManager.Instance.matchInventory[ConsumableID.Clue] = 0;
        }
        UpdateUI();
        int scoreModification = GameManager.Instance.OnCorrectGuess();

        StartCoroutine(GameManager.Instance.FinishProcessingTurnAction(
            turnSticker.amountOfAppearances,
            TurnAction.UseClue,
            scoreModification,
            turnSticker
            )); 
    }

    public void UseRemove()
    {
        Debug.Log("USE REMOVE");
        if (!GameManager.Instance.matchInventory.ContainsKey(ConsumableID.Remove) || GameManager.Instance.matchInventory[ConsumableID.Remove] == 0) return;
        GameManager.Instance.audioSource.PlayOneShot(buttonRemoveAudioClip);
        //anim
        GameManager.Instance.matchInventory[ConsumableID.Remove]--;
        if (GameManager.Instance.matchInventory[ConsumableID.Remove] <= 0)
        {
            GameManager.Instance.matchInventory[ConsumableID.Remove] = 0;
        }
        UpdateUI();
        GameManager.Instance.RemoveStickerFromPool();
        GameManager.Instance.NextTurn();
    }
    public void UseCut()
    {
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
