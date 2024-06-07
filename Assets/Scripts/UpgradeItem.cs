using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using Unity.VisualScripting;

public class UpgradeItem : MonoBehaviour
{
    public UpgradeID upgradeID;
    public Color upgradeColor;
    public Image background;
    public TextMeshProUGUI upgradeName;
    public TextMeshProUGUI upgradeDescription;
    public TextMeshProUGUI level;
    public Image iconMainHolder;
    public Image iconSecondaryHolder;
    
    public UserData userData => PersistanceManager.Instance.userData;
    // Start is called before the first frame update
    public void SetUpgradeButton(UpgradeID upgradeIDtoSet, bool addLevel = false)
    {
        int currentLevel = 0;
        if (userData.unlockedUpgrades.ContainsKey(upgradeIDtoSet)) {
            currentLevel = userData.unlockedUpgrades[upgradeIDtoSet];
        }
        if (addLevel) {
            currentLevel++;
        } 
        
        UpgradeData upgradeData = UpgradeData.GetUpgrade(upgradeIDtoSet);
        bool isMaxLevel = upgradeData.IsMaxLevel(currentLevel);
        int max = upgradeData.GetMaxLevel();
        
        upgradeID = upgradeIDtoSet;
        upgradeName.text = upgradeData.GetName();
        level.text = currentLevel + "/" + max;
        level.color = (isMaxLevel) ? Color.yellow: Color.white;
        background.color = upgradeData.backgroundColor;
        upgradeDescription.text = upgradeData.GenerateDescription(addLevel);
        iconMainHolder.sprite = ItemHelper.GetIconSprite(upgradeData.iconMain);
        if (upgradeData.iconSecondary != IconName.NONE) {
            iconSecondaryHolder.sprite = ItemHelper.GetIconSprite(upgradeData.iconSecondary);
        }
    }
}
