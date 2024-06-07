using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectUpgradePanel : MonoBehaviour {
    public PlayerLevelManager playerLevelManager => PlayerLevelManager.Instance;
    public UserData userData => PersistanceManager.Instance.userData;
    public List<UpgradeSelectionItem> selectionOptions;

    public void GetRandomUpgradesForPlayerSelection(int playerLevel) {
        // Lista para almacenar los upgrades disponibles
        List<UpgradeData> availableUpgrades = new List<UpgradeData>();

        // Recorre todos los IDs de upgrades
        foreach (UpgradeID upgradeID in Enum.GetValues(typeof(UpgradeID))) {
            if (upgradeID == UpgradeID.NONE) continue;

            var upgradeData = UpgradeData.GetUpgrade(upgradeID);

            // Obtiene el nivel actual del upgrade para el jugador
            int currentLevel = userData.GetUpgradeLevel(upgradeID);

            // Verifica si el jugador tiene el nivel requerido para el próximo nivel del upgrade
            CustomDebugger.Log(upgradeData.name);
            // CustomDebugger.Log("upgradeData.userLevelRequired.Length >= currentLevel");
            // CustomDebugger.Log(upgradeData.userLevelRequired.Length + " >= " + currentLevel);
            // CustomDebugger.Log(upgradeData.userLevelRequired.Length >= currentLevel);
            //
            // CustomDebugger.Log("playerLevel >= upgradeData.userLevelRequired[currentLevel]");
            // CustomDebugger.Log(playerLevel + " >= " + (upgradeData.userLevelRequired[currentLevel] ? "" : ""));
            // CustomDebugger.Log(playerLevel >= upgradeData.userLevelRequired[currentLevel]);

            if (upgradeData.playerLevelRequired.Length > currentLevel &&
                playerLevel >= upgradeData.playerLevelRequired[currentLevel]) {
                // Verifica si el jugador cumple con los upgrades requeridos para este upgrade
                bool requirementsMet = true;
                foreach (var requirement in upgradeData.upgradeRequired) {
                    if (!userData.unlockedUpgrades.ContainsKey(requirement.Key) ||
                        userData.unlockedUpgrades[requirement.Key] < requirement.Value) {
                        requirementsMet = false;
                        break;
                    }
                }

                // Si cumple con los requisitos y no ha alcanzado el nivel máximo, añadir a la lista de disponibles
                if (requirementsMet
                    //&& !upgradeData.IsMaxLevel(currentLevel)
                   ) {
                    availableUpgrades.Add(upgradeData);
                }
            }
        }

        // Si hay más de 2 upgrades disponibles, elegir 2 aleatoriamente para mostrar
        var chosenUpgrades = new List<UpgradeData>();
        if (availableUpgrades.Count > 2) {
            while (chosenUpgrades.Count < 2) {
                var randomUpgrade = availableUpgrades[UnityEngine.Random.Range(0, availableUpgrades.Count)];
                if (!chosenUpgrades.Contains(randomUpgrade)) {
                    chosenUpgrades.Add(randomUpgrade);
                }
            }
        }
        else if (availableUpgrades.Count > 0) {
            chosenUpgrades = availableUpgrades;
        }
        else {
            // No hay upgrades disponibles que cumplan con los criterios
            CustomDebugger.Log("No hay upgrades disponibles que cumplan con los criterios.");
        }

        int chosenOptionIndex = 0;
        foreach (var VARIABLE in selectionOptions) {
            if (chosenUpgrades.Count > chosenOptionIndex) {
                VARIABLE.gameObject.SetActive(true);
                VARIABLE.SetUpgradeButton(chosenUpgrades[chosenOptionIndex].itemId,true);
                chosenOptionIndex++;
            }
            else {
                VARIABLE.gameObject.SetActive(false);
            }
        }
    }

    public void SelectUpgrade(UpgradeID upgradeID) {
        playerLevelManager.SelectUpgradeToGet(upgradeID);
    }


    public void OnEnable() {
        GetRandomUpgradesForPlayerSelection(userData.GetAmountOfUpgrades());
    }


}