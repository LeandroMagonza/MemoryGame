using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;

public class UnlockedUpgradesPanel : MonoBehaviour {
    public UpgradeItem UpgradeItemPrefab;
    public List<UpgradeItem> unlockedUpgrades = new ();
    public GameObject unlockedUpgradesHolder;

    public UserData userData => PersistanceManager.Instance.userData;
    public void OrderUpgradePanels()
    {
        var upgradesOrdered = unlockedUpgrades
            .OrderByDescending(tuple => tuple.upgradeID)
            .ToList();
        foreach (var upgrade in upgradesOrdered)
        {
            upgrade.gameObject.transform.SetAsLastSibling();
        }
    }

    public void GenerateUpgradeItemForUnlocks() {

        foreach (var VARIABLE in unlockedUpgrades.ToList()) {
            VARIABLE.gameObject.SetActive(false);
        }

        int upgradeItemID = 0;
        foreach (var VARIABLE in userData.upgrades) {
            if (VARIABLE.Value == 0) continue;
            if (unlockedUpgrades.Count<=upgradeItemID) {
                UpgradeItem newUpgradeItem = Instantiate(UpgradeItemPrefab, unlockedUpgradesHolder.transform);
                unlockedUpgrades.Add(newUpgradeItem);
            }
            unlockedUpgrades[upgradeItemID].SetUpgradeButton(VARIABLE.Key);
            upgradeItemID++;
        }
    }
    
}
