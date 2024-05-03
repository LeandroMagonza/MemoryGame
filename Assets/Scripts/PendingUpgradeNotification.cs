using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PendingUpgradeNotification : MonoBehaviour {
    public TextMeshProUGUI amountOfPendingUpgrades;
    
    public void SetAmountOfPendingUpgrades(int _amountOfPendingUpgrades) {
        amountOfPendingUpgrades.text = _amountOfPendingUpgrades.ToString();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //shine
    }
}
