using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLevelPanel : MonoBehaviour {
    public UnlockedUpgradesPanel unlockedUpgradesPanel;
    private void OnEnable() {
        unlockedUpgradesPanel.GenerateUpgradeItemForUnlocks();
        unlockedUpgradesPanel.OrderUpgradePanels();
    }
}
