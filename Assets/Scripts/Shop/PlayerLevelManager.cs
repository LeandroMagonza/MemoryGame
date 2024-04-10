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
    public int playerLevel => GameManager.Instance.userData.unlockedUpgrades.Count;
    [FormerlySerializedAs("moneyDisplay")] public TextMeshProUGUI levelDisplay;
    public UserData userData => PersistanceManager.Instance.userData;
    public PendingUpgradeNotification pendingUpgradeNotification;
    private void Start()
    {
        unlockedUpgradesPanel.OrderUpgradePanels();
        unlockedUpgradesPanel.GenerateUpgradeItemForUnlocks();
        UpdateLevelDisplay();
    }
    private void OnEnable()
    {
        unlockedUpgradesPanel.gameObject.SetActive(true);
        upgradeSelectionPanel.gameObject.SetActive(false);
        UpdateLevelDisplay();
        unlockedUpgradesPanel.GenerateUpgradeItemForUnlocks();
        unlockedUpgradesPanel.OrderUpgradePanels();
    }
    private void UpdateLevelDisplay()
    {
        levelDisplay.text = playerLevel.ToString();
        int amountOfPendingUpgrades = userData.AmountOfPendingUpgrades();
        if (amountOfPendingUpgrades>0) {
            levelDisplay.transform.parent.GetComponent<Button>().interactable = true;
            pendingUpgradeNotification.gameObject.SetActive(true);
            pendingUpgradeNotification.SetAmountOfPendingUpgrades(amountOfPendingUpgrades);
        }
        else {
            levelDisplay.transform.parent.GetComponent<Button>().interactable = false;
            pendingUpgradeNotification.gameObject.SetActive(false);
        }
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

    
}



