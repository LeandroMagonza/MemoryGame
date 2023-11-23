using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class Sticker : MonoBehaviour
{
    private StickerData currentStickerData;
    
    public Image spriteHolder;
    public TextMeshProUGUI name;
    public Image expBar;
    public TextMeshProUGUI level;

    public void SetStickerData(StickerData stickerDataToSet)
    {
        currentStickerData = stickerDataToSet;
        int levelExpCurrent = stickerDataToSet.amountOfDuplicates - PersistanceManager.Instance.stickerLevels[stickerDataToSet.level].amountRequired;
        int levelExpTotal;
        if (PersistanceManager.Instance.stickerLevels.ContainsKey(stickerDataToSet.level+1))
        {
            levelExpTotal = PersistanceManager.Instance.stickerLevels[stickerDataToSet.level+1].amountRequired - PersistanceManager.Instance.stickerLevels[stickerDataToSet.level].amountRequired;
        }
        else
        {
            levelExpTotal = levelExpCurrent;
        }
        currentStickerData = stickerDataToSet;
        spriteHolder.sprite = stickerDataToSet.sprite;
        level.text = stickerDataToSet.level.ToString();
        expBar.fillAmount = levelExpCurrent / levelExpTotal;

    }
    
}
