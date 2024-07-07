using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class FactoryConsumableDisplay : MonoBehaviour
{
    // Start is called before the first frame update
    public ConsumableID consumableID;
    public bool isFactory; // factory == true || inventory == false
    public Image backgroundImage;
    [FormerlySerializedAs("amountToClaim")] public TextMeshProUGUI amount;
    public TextMeshProUGUI timer;
    public DateTime lastCheck = DateTime.Now;
    public string debugLastCheck;
    public string debugGenerationTimes;
    public Button claimButton;
    private void Start() {
        backgroundImage.sprite = ItemHelper.GetIconSprite(ConsumableData.GetConsumable(consumableID).icon);
        claimButton.GetComponent<Button>().onClick.AddListener(ClaimConsumable);

        int index = 0;
        debugGenerationTimes = "";
        foreach (var VARIABLE in PersistanceManager.Instance.userConsumableData.GetNextGenerationTimes(consumableID)) {
            debugGenerationTimes += index +" "+ VARIABLE.scheduledTime+"\n";
            index++;
        }
    }

    private void ClaimConsumable() {
        if (isFactory && ConsumableFactoryManager.Instance.CalculateAmountOfConsumablesToClaim(consumableID, false) > 0)
        {
            ConsumableFactoryManager.Instance.ClaimConsumable(consumableID);
        }
    }

    void Update() {
        debugLastCheck = lastCheck.ToString();
        // Solo actualizar el estado de los consumibles si estamos en el menú principal
        if (lastCheck.AddSeconds(1) < DateTime.Now)
        { 
            /*
            int index = 0;
            debugGenerationTimes = "";
            foreach (var VARIABLE in PersistanceManager.Instance.userConsumableData.GetNextGenerationTimes(consumableID)) {
                debugGenerationTimes += index +" "+ VARIABLE.scheduledTime+"\n";
                index++;
            }
            CustomDebugger.Log(consumableID+" factoryDisplay "+debugGenerationTimes);
            */
            UpdateTexts();
            lastCheck = DateTime.Now;
        }
    }

    public void UpdateTexts()
    {
        int consumableValueToUpdate;
        claimButton.interactable = false;
        if (isFactory) {
            var upgrade = ItemHelper.GetCorrespondingUpgrade(consumableID);
            var upgradeLv = PersistanceManager.Instance.userData.GetUpgradeLevel(upgrade);

            if (upgradeLv < 1) {
                SetInUse(false);
                return;
            }
            consumableValueToUpdate = ConsumableFactoryManager.Instance.CalculateAmountOfConsumablesToClaim(consumableID, false);
            if (consumableValueToUpdate > 0) {
                claimButton.interactable = true;
            }
        }
        else
        {
            consumableValueToUpdate = PersistanceManager.Instance.userConsumableData.GetConsumableEntry(consumableID).amount;
            if (consumableValueToUpdate == 0) {
                SetInUse(false);
                return;
            }
        }
        SetInUse(true);
        
        backgroundImage.gameObject.transform.parent.gameObject.SetActive(true);
        amount.text = consumableValueToUpdate.ToString();
    
        //CustomDebugger.Log("Update Texts for factory bar called on item " + consumableID + ((isFactory) ? " factory" : " inventory") + ": " + consumableValueToUpdate);

        if (isFactory)
        {
            var generationTimes = PersistanceManager.Instance.userConsumableData.GetNextGenerationTimes(consumableID);
            timer.transform.parent.gameObject.SetActive(true);

            if (generationTimes.Count > 0)
            {
                DateTime nextGenerationTime = generationTimes[0].scheduledTime;
                if (DateTime.Now < nextGenerationTime)
                {
                    timer.text = FormatTimeRemaining(nextGenerationTime);
                }
                else
                {
                    timer.text = "CLAIM!";
                }
            }
            else
            {
                timer.text = "ERROR";
            }
        }
        else
        {
            timer.transform.parent.gameObject.SetActive(false);
        }
    }

    private string FormatTimeRemaining(DateTime scheduledTime)
    {
        TimeSpan timeRemaining = scheduledTime.Subtract(DateTime.Now);
        var lm = LocalizationManager.Instance;
        if (timeRemaining.TotalHours >= 1)
        {
            return string.Format("{0}:{1:D2} "+lm.GetGameText(GameText.HoursAbb), (int)timeRemaining.TotalHours, timeRemaining.Minutes);
        }
        else if (timeRemaining.TotalMinutes >= 1)
        {
            return string.Format("{0} "+lm.GetGameText(GameText.MinutesAbb), (int)timeRemaining.TotalMinutes);
        }
        else
        {
            return string.Format("{0} "+lm.GetGameText(GameText.SecondsAbb), (int)timeRemaining.TotalSeconds);
        }
    }

    public void SetInUse(bool inUse) {
        if (inUse) {
            backgroundImage.color = new Color(
                backgroundImage.color.r,
                backgroundImage.color.g,
                backgroundImage.color.b,
                1
            );
            backgroundImage.gameObject.transform.parent.GetComponent<Image>().color = new Color(
                backgroundImage.color.r,
                backgroundImage.color.g,
                backgroundImage.color.b,
                1
            );
            timer.transform.parent.gameObject.SetActive(true);
            amount.gameObject.SetActive(true);
        }
        else {
            timer.transform.parent.gameObject.SetActive(false);
            amount.gameObject.SetActive(false);
            //backgroundImage.gameObject.transform.parent.gameObject.SetActive(false);
            backgroundImage.color = new Color(
                backgroundImage.color.r,
                backgroundImage.color.g,
                backgroundImage.color.b,
                .5f
            );
            backgroundImage.gameObject.transform.parent.GetComponent<Image>().color = new Color(
                backgroundImage.color.r,
                backgroundImage.color.g,
                backgroundImage.color.b,
                .68f
            ); 
            
        }
    }
}
