using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayerLevelDisplayButton : MonoBehaviour
{
    public TextMeshProUGUI levelDisplay;
    public PendingUpgradeNotification pendingUpgradeNotification;
    public int playerLevel => PersistanceManager.Instance.userData.playerLevel;
    public UserData userData => PersistanceManager.Instance.userData;

    private void Start() {
        levelDisplay.text = playerLevel.ToString();
    }

    public void UpdateLevelDisplay()
    {
        foreach (var VARIABLE in userData.unlockedUpgrades) {
            CustomDebugger.Log("counting levels: "+VARIABLE.Key + VARIABLE.Value);
        }
        CustomDebugger.Log("player level: "+playerLevel.ToString());
        
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
        //
    }
}
