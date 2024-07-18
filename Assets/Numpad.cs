using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Numpad : MonoBehaviour
{
    public NumpadButton[] numpadButtons;

    public void SetNumpadButtons((StickerData sticker, StickerMatchData matchData) stickerData)
    {
        ResetNumpadUIButtons();
        if (stickerData.matchData == null || stickerData.sticker == null) return;
    
        //BetterClue
        if (stickerData.matchData.lastClueAppearenceNumber != null)
        {
            CustomDebugger.Log("active turn sticker" + stickerData.matchData.lastClueAppearenceNumber);
            numpadButtons[(int)stickerData.matchData.lastClueAppearenceNumber-1]._numberText.color = Color.green;
            for (int i = 0; i < (int)stickerData.matchData.lastClueAppearenceNumber; i++)
            {
                numpadButtons[i]._button.interactable = false;
            }
        }

        Debug.Log("numbers to cut in updateui: " + stickerData.matchData.cutNumbers.Count);
    
        foreach (var cutNumber in stickerData.matchData.cutNumbers)
        {
            StartCoroutine(numpadButtons[cutNumber-1].AnimateCut(GameManager.Instance.GetPowerButton(ConsumableID.Cut).gameObject));
        }
    }

    public void ResetNumpadUIButtons()
    {
        if (numpadButtons.Length > 0)
        {
            for (int i = 0; i < numpadButtons.Length; i++)
            {
                if (numpadButtons[i].gameObject.activeInHierarchy)
                {
                    numpadButtons[i]._button.interactable = true;
                    numpadButtons[i]._numberText.color = Color.white;
                }
            }
        }
    }

    public void SetNumpadByDifficulty(int difficulty)
    {
        for (int i = 0; i < numpadButtons.Length; i++)
        {
            numpadButtons[i].gameObject.transform.parent.gameObject.SetActive(false);
            numpadButtons[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < difficulty; i++)
        {
            numpadButtons[i].gameObject.SetActive(true);
            numpadButtons[i].gameObject.transform.parent.gameObject.SetActive(true);
            numpadButtons[i].RefreshSize();
        }
    }
}
