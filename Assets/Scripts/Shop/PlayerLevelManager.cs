using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerLevelManager : MonoBehaviour
{

    public UnlockedUpgradesPanel unlockedUpgradesPanel;
    //public SelectUpgradePanel selectUpgradePanel;
    [FormerlySerializedAs("moneyDisplay")] public TextMeshProUGUI levelDisplay;
    public UserData userData => PersistanceManager.Instance.userData;

    private void Start()
    {
        unlockedUpgradesPanel.OrderUpgradePanels();
    }
    private void OnEnable()
    {
        unlockedUpgradesPanel.gameObject.SetActive(true);
        //selectUpgradePanel.gameObject.SetActive(false);
        UpdateLevelDisplay();
        unlockedUpgradesPanel.GenerateUpgradeItemForUnlocks();
        unlockedUpgradesPanel.OrderUpgradePanels();
    }
    private void UpdateLevelDisplay()
    {
        levelDisplay.text = GameManager.Instance.userData.coins.ToString();
    }



    
}



