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

        foreach (var userDataUpgrade in userData.unlockedUpgrades) {
            if (userDataUpgrade.Value == 0) continue;
            
            UpgradeItem currentUpgradeItem = null; 
            
            //Checkeo si ya existe el upgrade de user data en la lista de upgradeitems que se muestra
            for (int unlockedUpgradeItemIndex = 0; unlockedUpgradeItemIndex < unlockedUpgrades.Count; unlockedUpgradeItemIndex++) {
                if (unlockedUpgrades[unlockedUpgradeItemIndex].upgradeID == userDataUpgrade.Key) {
                    currentUpgradeItem = unlockedUpgrades[unlockedUpgradeItemIndex];
                }
            }

            //si no existe creo uno
            if (currentUpgradeItem is null) {
                currentUpgradeItem = Instantiate(UpgradeItemPrefab, unlockedUpgradesHolder.transform);
                unlockedUpgrades.Add(currentUpgradeItem);
            }
            
            currentUpgradeItem.gameObject.SetActive(true);
            currentUpgradeItem.SetUpgradeButton(userDataUpgrade.Key);
        }
    }
    
}
