using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectUpgradePanel : MonoBehaviour {
    public PlayerLevelManager playerLevelManager => PlayerLevelManager.Instance;
    public UserData userData => PersistanceManager.Instance.userData;
    public List<UpgradeSelectionItem> selectionOptions;

    private const string SelectRandomUpgrade1Key = "SelectRandomUpgrade1";
    private const string SelectRandomUpgrade2Key = "SelectRandomUpgrade2";

    public void GetRandomUpgradesForPlayerSelection(int playerLevel) {
        List<UpgradeData> availableUpgrades = new List<UpgradeData>();

        foreach (UpgradeID upgradeID in Enum.GetValues(typeof(UpgradeID))) {
            if (upgradeID == UpgradeID.NONE) continue;

            var upgradeData = UpgradeData.GetUpgrade(upgradeID);
            int currentLevel = userData.GetUpgradeLevel(upgradeID);

            if (upgradeData.playerLevelRequired.Length > currentLevel &&
                playerLevel >= upgradeData.playerLevelRequired[currentLevel]) {
                bool requirementsMet = true;
                foreach (var requirement in upgradeData.upgradeRequired) {
                    if (!userData.unlockedUpgrades.ContainsKey(requirement.Key) ||
                        userData.unlockedUpgrades[requirement.Key] < requirement.Value) {
                        requirementsMet = false;
                        break;
                    }
                }

                if (requirementsMet) {
                    availableUpgrades.Add(upgradeData);
                }
            }
        }

        List<UpgradeData> chosenUpgrades = new List<UpgradeData>();
        if (PlayerPrefs.HasKey(SelectRandomUpgrade1Key) && PlayerPrefs.HasKey(SelectRandomUpgrade2Key)) {
            // Load saved upgrades
            var upgradeID1 = (UpgradeID)PlayerPrefs.GetInt(SelectRandomUpgrade1Key);
            var upgradeID2 = (UpgradeID)PlayerPrefs.GetInt(SelectRandomUpgrade2Key);
            chosenUpgrades.Add(UpgradeData.GetUpgrade(upgradeID1));
            chosenUpgrades.Add(UpgradeData.GetUpgrade(upgradeID2));
        } else {
            // Randomly select upgrades
            if (availableUpgrades.Count > 2) {
                while (chosenUpgrades.Count < 2) {
                    var randomUpgrade = availableUpgrades[UnityEngine.Random.Range(0, availableUpgrades.Count)];
                    if (!chosenUpgrades.Contains(randomUpgrade)) {
                        chosenUpgrades.Add(randomUpgrade);
                    }
                }
            } else if (availableUpgrades.Count > 0) {
                chosenUpgrades = availableUpgrades;
            } else {
                CustomDebugger.Log("No hay upgrades disponibles que cumplan con los criterios.");
            }

            // Save chosen upgrades
            if (chosenUpgrades.Count > 0) PlayerPrefs.SetInt(SelectRandomUpgrade1Key, (int)chosenUpgrades[0].itemId);
            if (chosenUpgrades.Count > 1) PlayerPrefs.SetInt(SelectRandomUpgrade2Key, (int)chosenUpgrades[1].itemId);
        }

        int chosenOptionIndex = 0;
        foreach (var VARIABLE in selectionOptions) {
            if (chosenUpgrades.Count > chosenOptionIndex) {
                VARIABLE.gameObject.SetActive(true);
                VARIABLE.SetUpgradeButton(chosenUpgrades[chosenOptionIndex].itemId, true);
                chosenOptionIndex++;
            } else {
                VARIABLE.gameObject.SetActive(false);
            }
        }
    }

    public void SelectUpgrade(UpgradeID upgradeID) {
        playerLevelManager.SelectUpgradeToGet(upgradeID);
        PlayerPrefs.DeleteKey(SelectRandomUpgrade1Key);
        PlayerPrefs.DeleteKey(SelectRandomUpgrade2Key);
    }

    public void OnEnable() {
        GetRandomUpgradesForPlayerSelection(userData.GetAmountOfUpgrades());
    }
}
