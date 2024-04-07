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
    public TextMeshProUGUI upgradeName;
    public TextMeshProUGUI upgradeDescription;
    public TextMeshProUGUI level;
    public Image icon;
    
    public UserData userData => PersistanceManager.Instance.userData;
    // Start is called before the first frame update
    public void SetUpgradeButton(UpgradeID upgradeIDtoSet)
    {
        int currentLevel = 0;

        if (!userData.upgrades.ContainsKey(upgradeIDtoSet)) {
            throw new Exception("Intentando setear upgrade item con upgrade id que el jugador no tiene");
        }
        
        currentLevel = userData.upgrades[upgradeIDtoSet];       
        bool isMaxLevel = UpgradeData.GetUpgrade(upgradeIDtoSet).IsMaxLevel(currentLevel);
        int max = UpgradeData.GetUpgrade(upgradeIDtoSet).GetMaxLevel();
        string description = UpgradeData.GetUpgrade(upgradeIDtoSet).description;
        upgradeID = upgradeIDtoSet;
        level.text = currentLevel + "/" + max;
        level.color = (isMaxLevel) ? Color.yellow: Color.white;
        upgradeDescription.text = description;
    }
}
