using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Services.Analytics;
using UnityEngine;

public class ConsumableFactoryManager : MonoBehaviour
{
    #region Singleton
    private static ConsumableFactoryManager _instance;
    public static ConsumableFactoryManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<ConsumableFactoryManager>();
            if (_instance == null)
                CustomDebugger.LogError("Singleton<" + typeof(ConsumableFactoryManager) + "> instance has been not found.");
            return _instance;
        }
    }
    protected void Awake()
    {
        if (_instance == null)
        {
            _instance = this as ConsumableFactoryManager;
        }
        else if (_instance != this)
            DestroySelf();
    }
    private void DestroySelf()
    {
        if (Application.isPlaying)
            Destroy(this);
        else
            DestroyImmediate(this);
    }
    #endregion

    public UserConsumableData userConsumableData => PersistanceManager.Instance.userConsumableData;
    public FactoriesBarUi factoriesBarUi;
    public DateTime LastClaimTime(ConsumableID consumableID)
    {
            string lastClaimTimeString = PlayerPrefs.GetString("LastClaimTime:"+consumableID, null);
            if (string.IsNullOrEmpty(lastClaimTimeString))
            {
                return DateTime.MinValue; // Si no hay un tiempo guardado, usa la hora actual como predeterminada
            }
            return DateTime.Parse(lastClaimTimeString);
    }


   public void GenerateNextGenerationTimes(ConsumableID consumableID, bool now = false)
{
    CustomDebugger.Log("Generating next generation times for " + consumableID);

    ConsumableData consumableData = ConsumableData.GetConsumable(consumableID);

    int upgradeLevel = PersistanceManager.Instance.GetUpgradeLevel(ItemHelper.GetCorrespondingUpgrade(consumableID));
    if (upgradeLevel == 0) return;

    int maxCapacity = consumableData.initialStorage + upgradeLevel;
    double generationInterval = 1;
    if (!now) {
        generationInterval = (consumableData.generationMinutes / (double)upgradeLevel) * 60; // en segundos
    }
    CustomDebugger.Log("NGT " + consumableID + " capacity " + maxCapacity);

    DateTime nextTime;
    var generationTimes = userConsumableData.GetNextGenerationTimes(consumableID);
    CustomDebugger.Log("read generation times :"+generationTimes.Count);
    if (generationTimes.Count == 0)
    {
        nextTime = DateTime.Now.AddSeconds(generationInterval);
        generationTimes.Add((nextTime, 0));
    }
    else
    {
        nextTime = generationTimes[^1].scheduledTime;
    }

    CustomDebugger.Log("added first generation times :"+generationTimes.Count);

    for (int i = generationTimes.Count; i < maxCapacity; i++)
    {
        nextTime = nextTime.AddSeconds(generationInterval);
        CustomDebugger.Log("NGT " + consumableID + " order " +i+ " at time "+nextTime);
        int notificationId = 0;
        if (i == maxCapacity - 1)
        {
            var lm = LocalizationManager.Instance;
            string title = lm.GetGameText(GameText.FactoryNotificationTitle);
            string body = lm.GetGameText(GameText.FactoryNotificationBody);

            title = ItemHelper.ReplacePlaceholders(title, new Dictionary<string, string>() {
                {"consumableName", lm.GetGameText(consumableData.name)}
            });
            body = ItemHelper.ReplacePlaceholders(body, new Dictionary<string, string>() {
                {"consumableName", consumableData.GetName(maxCapacity>1)}
            });

            notificationId = NotificationManager.Instance.ScheduleNotification(title, body, nextTime, consumableID);
        }
        generationTimes.Add((nextTime, notificationId));
    }
    CustomDebugger.Log("calculated generation times :"+generationTimes.Count);
    userConsumableData.ModifyConsumable(consumableID,0,generationTimes);
    CustomDebugger.Log("modified generation times :"+userConsumableData.GetNextGenerationTimes(consumableID).Count);
    
    int index = 0;
    string debug = consumableID+" ";
    foreach (var VARIABLE in userConsumableData.GetNextGenerationTimes(consumableID)) {
        debug += index +" "+ VARIABLE.scheduledTime+"\n";
        index++;
    }
    CustomDebugger.Log("final next generation times" +debug);
    CustomDebugger.Log("final next generation times: " + userConsumableData.GetNextGenerationTimes(consumableID).Count);
}


    public void ClaimAllConsumables() {
        foreach (ConsumableID consumableID in Enum.GetValues(typeof(ConsumableID)))
        {
            ClaimConsumable(consumableID,false);
        }
        PersistanceManager.Instance.SaveUserConsumableData();
    }

    public void ClaimConsumable(ConsumableID consumableID, bool saveConsumables = true)
    {
        int consumablesToClaim = CalculateAmountOfConsumablesToClaim(consumableID, true);

        var consumableEntry = userConsumableData.GetConsumableEntry(consumableID);
        consumableEntry.amount += consumablesToClaim;

        // Resetear los tiempos de generación
        GenerateNextGenerationTimes(consumableID);

        // Guardar el nuevo LastClaimTime en PlayerPrefs
        factoriesBarUi.UpdateDisplays();
        PlayerPrefs.SetString("LastClaimTime:"+consumableID, DateTime.Now.ToString());
        if (saveConsumables) {
            PersistanceManager.Instance.SaveUserConsumableData();
        }
    }

    public int CalculateAmountOfConsumablesToClaim(ConsumableID consumableID, bool delete)
    {
        var consumableEntry = userConsumableData.GetConsumableEntry(consumableID);

        if (delete) {
            NotificationManager.Instance.CancelNotificationsFromCategory(consumableID);
        }
        return consumableEntry.GetGeneratedAmount(delete);
    }


    public IEnumerator GenerateAllConsumablesNextGenerationTimes() {
        CustomDebugger.Log("Generating all consumables next generation times");
        foreach (ConsumableID type in Enum.GetValues(typeof(ConsumableID))) {
            GenerateNextGenerationTimes(type);
        }
        PersistanceManager.Instance.SaveUserConsumableData();
        yield return null;
    }
}


