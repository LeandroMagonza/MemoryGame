using System;
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
    public Image bonus;
    public TextMeshProUGUI level;
    public TextMeshProUGUI levelStickerCurrent;
    public TextMeshProUGUI levelStickerTotal;

    public void SetStickerData(StickerData stickerDataToSet)
    {
        Debug.Log("Setting Sticker data for stickerID "+stickerDataToSet.stickerID);
        currentStickerData = stickerDataToSet;
        SetName(stickerDataToSet);
        SetExpBar(stickerDataToSet);
        SetSprite(stickerDataToSet);
        SetLevel(stickerDataToSet);

    }

    public void SetName(StickerData stickerDataToSet) {
        name.text = stickerDataToSet.name;
        name.color = stickerDataToSet.color;
    }
    private void SetExpBar(StickerData stickerDataToSet) {
        int duplicatesForCurrentLevel = 0;

        int duplicatesForImage;
        int duplicatesForNextLevel;
        
        if (!PersistanceManager.Instance.StickerLevels.ContainsKey(stickerDataToSet.level)) {
            if (stickerDataToSet.level == 0) {
                duplicatesForCurrentLevel = 0;
            }
            else {
                throw new Exception("sticker level out of bounds");
            }
        }
        else {
            duplicatesForCurrentLevel = PersistanceManager.Instance.StickerLevels[stickerDataToSet.level].amountRequired;
        }
        
        Debug.Log("stickerDataToSet.amountOfDuplicates "+stickerDataToSet.amountOfDuplicates);
        Debug.Log("duplicates for this level "+duplicatesForCurrentLevel);
        Debug.Log("duplicates on this sticker "+stickerDataToSet.amountOfDuplicates);
        Debug.Log("stickerDataLevel "+stickerDataToSet.level);
        int levelExpCurrent = stickerDataToSet.amountOfDuplicates - duplicatesForCurrentLevel;
        int levelExpTotal;
        
        if (PersistanceManager.Instance.StickerLevels.ContainsKey(stickerDataToSet.level + 1)) {
            levelExpTotal = PersistanceManager.Instance.StickerLevels[stickerDataToSet.level + 1].amountRequired -
                            duplicatesForCurrentLevel;
            Debug.Log("duplicates for next level"+PersistanceManager.Instance.StickerLevels[stickerDataToSet.level + 1].amountRequired);
        }
        else {
            levelExpTotal = -1;
        }
        Debug.Log("levelExpCurrent"+levelExpCurrent);
        Debug.Log("levelExpTotal"+levelExpTotal);
        if (levelExpTotal == -1) {
            expBar.fillAmount = 1;
            expBar.color = Color.green;
            DisplayLevelStickerProgressAmount(false);
        }
        else {
            Debug.Log("fill amount "+(float)levelExpCurrent / levelExpTotal);
            expBar.fillAmount = (float)levelExpCurrent / levelExpTotal;
            expBar.color = Color.yellow;
            levelStickerCurrent.text = levelExpCurrent.ToString();
            levelStickerTotal.text = levelExpTotal.ToString();
            DisplayLevelStickerProgressAmount(true);
        }
    }

    private void SetSprite(StickerData stickerDataToSet) {
        spriteHolder.sprite = stickerDataToSet.sprite;
    }

    private void SetLevel(StickerData stickerDataToSet) {
        level.text = stickerDataToSet.level.ToString();
    }

    public void DisplayName(bool active){
        name.gameObject.SetActive(active);
    }
    public void DisplayLevel(bool active){
        level.gameObject.SetActive(active);
    }  
    public void DisplayLevelStickerProgressAmount(bool active){
        levelStickerTotal.gameObject.SetActive(active);
        levelStickerCurrent.gameObject.SetActive(active);
        
    }
    public void DisplayFrame(bool active) {
        GetComponent<Image>().enabled = active;
    }
    public void DisplayExpBar(bool active) {
        expBar.gameObject.SetActive(active);
    }    
    public void DisplayBonus(bool active) {
        bonus.gameObject.SetActive(active);
    }

    public void ConfigureForPack() {
        DisplayFrame(true);
        DisplayLevel(true);
        DisplayLevelStickerProgressAmount(true);
        DisplayName(true);
        DisplayExpBar(true);
        DisplayBonus(false);
    }
    public void ConfigureForGame() {
        DisplayFrame(false);
        DisplayLevel(false);
        DisplayLevelStickerProgressAmount(false);
        DisplayName(true);
        DisplayExpBar(false);
        DisplayBonus(true);
    }
}
