using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeSelectionItem : UpgradeItem {
    public UpgradeSelectionPanel upgradeSelectionPanel;

    public void SelectThisUpgrade() {
        upgradeSelectionPanel.SelectUpgrade(upgradeID);
    }
}
