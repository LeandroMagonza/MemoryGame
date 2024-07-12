using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeSelectionItem : UpgradeItem {
    public SelectUpgradePanel selectUpgradePanel;
    public Color baseColor;
    public Color shiningColor;
    public bool shining = false;
    public float shiningSpeed = 1;

    public void SelectThisUpgrade() {
        selectUpgradePanel.SelectUpgrade(upgradeID);
    }

    public void Start() {
        shining = true;
        baseColor = iconMainHolder.color;
    }

    private void FixedUpdate() {
        if (shining) {
            iconMainHolder.color = new Color(
                iconMainHolder.color.r - baseColor.r * shiningSpeed * Time.deltaTime,
                iconMainHolder.color.g - baseColor.g * shiningSpeed * Time.deltaTime,
                iconMainHolder.color.b - baseColor.b * shiningSpeed * Time.deltaTime
            );
            if (iconMainHolder.color.r < baseColor.r * 0.5f) {
                iconMainHolder.color = baseColor;
            }
        }
        else {
            iconMainHolder.color = baseColor;
        }
    }

}
