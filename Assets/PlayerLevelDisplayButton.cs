using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayerLevelDisplayButton : ChangeCanvasButton
{
    public TextMeshProUGUI levelDisplay;
    public PendingUpgradeNotification pendingUpgradeNotification;
    public int playerLevel => PersistanceManager.Instance.userData.playerLevel;
    public UserData userData => PersistanceManager.Instance.userData;
    public SelectUpgradePanel selectUpgradePanel;

    public override void Start() {
        PlayerLevelManager.Instance.OnPlayerLevelUp += UpdateLevelDisplay;
        levelDisplay.text = playerLevel.ToString();
        base.Start();
    }

    public void OnEnable() {
        UpdateLevelDisplay();
    }

    public void UpdateLevelDisplay(int level = -1)
    {
        //level no es usado pero viene en el evento, por eso esta como parametro
        foreach (var VARIABLE in userData.unlockedUpgrades) {
            CustomDebugger.Log("counting levels: "+VARIABLE.Key + VARIABLE.Value);
        }
        CustomDebugger.Log("player level: "+playerLevel.ToString());
        
        levelDisplay.text = playerLevel.ToString();
        int amountOfPendingUpgrades = userData.AmountOfPendingUpgrades();
        if (amountOfPendingUpgrades>0) {
            GetComponent<Button>().interactable = true;
            pendingUpgradeNotification.gameObject.SetActive(true);
            pendingUpgradeNotification.SetAmountOfPendingUpgrades(amountOfPendingUpgrades);
        }
        else {
            GetComponent<Button>().interactable = false;
            pendingUpgradeNotification.gameObject.SetActive(false);
        }
    }

    public override void OnClick() {
        if (selectUpgradePanel == null || canvasToSet == CanvasName.NO_CANVAS) {
            base.OnClick();
        }
        else {
            selectUpgradePanel.GetRandomUpgradesForPlayerSelection(playerLevel);
        }
    }
    void OnDestroy()
    {
        // Es importante desuscribirse para evitar errores si el objeto se destruye
        if (PlayerLevelManager.Instance != null)
        {
            PlayerLevelManager.Instance.OnPlayerLevelUp -= UpdateLevelDisplay;
        }
    }
}
