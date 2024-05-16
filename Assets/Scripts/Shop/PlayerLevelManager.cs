using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayerLevelManager : MonoBehaviour
{
    
    #region Singleton
    private static PlayerLevelManager _instance;
    public static PlayerLevelManager Instance {
        get {
            if (_instance == null)
                _instance = FindObjectOfType<PlayerLevelManager>();
            if (_instance == null) {
                //CustomDebugger.LogError("Singleton<" + typeof(PlayerLevelManager) + "> instance has been not found.");
            }
                
            return _instance;
        }
    }
    protected void Awake() {
        if (_instance == null) {
            _instance = this as PlayerLevelManager;
        }
        else if (_instance != this)
            DestroySelf();
    }
    private void DestroySelf() {
        if (Application.isPlaying)
            Destroy(this);
        else
            DestroyImmediate(this);
    }
    

    #endregion

    [FormerlySerializedAs("upgradeSelectionPanel")] public SelectUpgradePanel selectUpgradePanel;
    public PlayerLevelPanel playerLevelPanel;
    public UnlockedUpgradesPanel unlockedUpgradesPanel => playerLevelPanel.unlockedUpgradesPanel;
    public PlayerLevelDisplayButton playerLevelDisplayButton;
    public int playerLevel => GameManager.Instance.userData.playerLevel;

    // Delegado que define la firma para el evento OnPlayerLevelUp
    public delegate void PlayerLevelUpHandler(int newLevel);

    // Evento que otros componentes pueden suscribirse
    public event PlayerLevelUpHandler OnPlayerLevelUp;


    public void SelectUpgradeToGet(UpgradeID selectedUpgradeID) {
        UnlockUpgrade(selectedUpgradeID);
        OnPlayerLevelUp?.Invoke(GameManager.Instance.userData.playerLevel);
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
        unlockedUpgradesPanel.GenerateUpgradeItemForUnlocks();
        unlockedUpgradesPanel.OrderUpgradePanels();
        CanvasManager.Instance.ChangeCanvas(CanvasName.RETURN);

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



