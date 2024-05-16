using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeSelectionItem : UpgradeItem {
    public SelectUpgradePanel selectUpgradePanel;

    public void SelectThisUpgrade() {
        selectUpgradePanel.SelectUpgrade(upgradeID);
    }
}
