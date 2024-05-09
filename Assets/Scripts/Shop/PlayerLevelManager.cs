using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayerLevelManager : MonoBehaviour
{

    public UnlockedUpgradesPanel unlockedUpgradesPanel;
    public UpgradeSelectionPanel upgradeSelectionPanel;
    public PlayerLevelDisplayButton playerLevelDisplayButton;
    public int playerLevel => GameManager.Instance.userData.playerLevel;
    private void Start()
    {
        unlockedUpgradesPanel.OrderUpgradePanels();
        unlockedUpgradesPanel.GenerateUpgradeItemForUnlocks();
        playerLevelDisplayButton.gameObject.SetActive(true);
        playerLevelDisplayButton.UpdateLevelDisplay();
    }
    private void OnEnable()
    {
        unlockedUpgradesPanel.gameObject.SetActive(true);
        upgradeSelectionPanel.gameObject.SetActive(false);
        playerLevelDisplayButton.gameObject.SetActive(true);
        playerLevelDisplayButton.UpdateLevelDisplay();
        unlockedUpgradesPanel.GenerateUpgradeItemForUnlocks();
        unlockedUpgradesPanel.OrderUpgradePanels();
    }


    public void OpenUpgradeSelectionPanel() {
        upgradeSelectionPanel.gameObject.SetActive(true);
        unlockedUpgradesPanel.gameObject.SetActive(false);

        upgradeSelectionPanel.GetRandomUpgradesForPlayerSelection(playerLevel);
        
}


    public void SelectUpgradeToGet(UpgradeID selectedUpgradeID) {
        UnlockUpgrade(selectedUpgradeID);
        upgradeSelectionPanel.gameObject.SetActive(false);
        unlockedUpgradesPanel.gameObject.SetActive(true);
    }

    public UpgradeID upgradeToAddTest;
    [ContextMenu("UnlockUpgradeTest")]
    public void UnlockUpgradeTest() {
        UnlockUpgrade(upgradeToAddTest);
    }
    public void UnlockUpgrade(UpgradeID upgradeID)
    {
        CustomDebugger.Log("Added Item:" + nameof(upgradeID));
        GameManager.Instance.userData.AddUpgradeToUser(upgradeID);
        PersistanceManager.Instance.SaveUserData();
        OnEnable();
        //UpdateMoneyDisplay();
        //UpdateConsumableButtonsUI();
        //UpdateUpgradeButtonsIU();

    }
    [ContextMenu("LogUpgradeLevelRequiredTable")]
    public void LogUpgradeLevelRequiredTable() {
        CustomDebugger.Log("LogUpgradeLevelRequiredTable");
        List<int> upgradesAvailablePerLevel = new List<int>();
        foreach (UpgradeID upgradeID in Enum.GetValues(typeof(UpgradeID))) {
            
            CustomDebugger.Log(upgradeID);
            if (upgradeID == UpgradeID.NONE) continue;

            UpgradeData currentUpgradeData = UpgradeData.GetUpgrade(upgradeID,true);
            CustomDebugger.Log("Levels:"+currentUpgradeData.playerLevelRequired.Length);
            
            foreach (var VARIABLE in currentUpgradeData.playerLevelRequired) {
                while (VARIABLE+1 > upgradesAvailablePerLevel.Count) {
                    upgradesAvailablePerLevel.Add(0);
                }
                upgradesAvailablePerLevel[VARIABLE]++;
            }
            CustomDebugger.Log("Upgrade Available per levels count:"+upgradesAvailablePerLevel.Count);
        }
        CustomDebugger.Log("Upgrade Available per levels:"+upgradesAvailablePerLevel);

        int remainingUpgrades = 0;
        for (int i = 0; i < upgradesAvailablePerLevel.Count; i++) {
            remainingUpgrades += upgradesAvailablePerLevel[i];
            CustomDebugger.Log("PlayerLevel: "+i+" UpgradesAvailable: "+remainingUpgrades);
            remainingUpgrades--;
        }
    }

    
}



