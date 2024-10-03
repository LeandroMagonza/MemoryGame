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
    [FormerlySerializedAs("backgroundImage")] public Image iconImage;
    [FormerlySerializedAs("amountToClaim")] public TextMeshProUGUI amount;
    public TextMeshProUGUI timer;
    public DateTime lastCheck = DateTime.Now;
    public string debugLastCheck;
    public string debugGenerationTimes;
    public Button claimButton;

    public Color originalIconColor;
    [SerializeField] private Color originalBackgroundButtonColor;
    public Image backgroundButtonImage;
    public Color claimableColor = Color.green;    
    private void Start() {
        iconImage.sprite = ItemHelper.GetIconSprite(ConsumableData.GetConsumable(consumableID).icon);
        claimButton.GetComponent<Button>().onClick.AddListener(ClaimConsumable);

        int index = 0;
        debugGenerationTimes = "";
        foreach (var VARIABLE in PersistanceManager.Instance.userConsumableData.GetNextGenerationTimes(consumableID)) {
            debugGenerationTimes += index +" "+ VARIABLE.scheduledTime+"\n";
            index++;
        }
        originalIconColor = iconImage.color;
    }

    private void ClaimConsumable() {
        if (isFactory && ConsumableFactoryManager.Instance.CalculateAmountOfConsumablesToClaim(consumableID, false) > 0)
        {
            ConsumableFactoryManager.Instance.ClaimConsumable(consumableID);
            AudioManager.Instance.PlayClip(GameClip.bonus);
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

    public int UpdateTexts()// devuelve los pending claims para habilitar o no la badge de notificacion
    {
        int consumableValueToUpdate;
        claimButton.interactable = false;
        if (isFactory) {
            var upgrade = ItemHelper.GetCorrespondingUpgrade(consumableID);
            var upgradeLv = PersistanceManager.Instance.userData.GetUpgradeLevel(upgrade);

            if (upgradeLv < 1) {
                SetInUse(false);
                return 0;
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
                return 0;
            }
        }
        SetInUse(true);
        
        iconImage.gameObject.transform.parent.gameObject.SetActive(true);
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
                    timer.text = ItemHelper.FormatTimeRemaining(nextGenerationTime);
                    backgroundButtonImage.color = originalBackgroundButtonColor;
                }
                else
                {
                    
                    timer.text = LocalizationManager.Instance.GetGameText(GameText.Claim).ToUpper()+"!";
                    //CustomDebugger.Log("Claim Color: "+new Color(80,215,00,1),DebugCategory.CONSUMABLE_DISPLAY);
                    backgroundButtonImage.color = claimableColor;
                    //CustomDebugger.Log("Actually Set Color: "+backgroundButtonImage.color,DebugCategory.CONSUMABLE_DISPLAY);
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

        return consumableValueToUpdate;
    }



    public void SetInUse(bool inUse) {
        Color imageColor = iconImage.color;
        Color buttonBackgroundColor = backgroundButtonImage.color;
        
        if (inUse) {
            imageColor = new Color(
                imageColor.r,
                imageColor.g,
                imageColor.b,
                1
            );
            
            buttonBackgroundColor = new Color(
                buttonBackgroundColor.r,
                buttonBackgroundColor.g,
                buttonBackgroundColor.b,
                1
            );
            timer.transform.parent.gameObject.SetActive(true);
            amount.gameObject.SetActive(true);
        }
        else {
            timer.transform.parent.gameObject.SetActive(false);
            amount.gameObject.SetActive(false);
            
            imageColor = new Color(
                imageColor.r,
                imageColor.g,
                imageColor.b,
                .5f
            );
            buttonBackgroundColor = new Color(
                buttonBackgroundColor.r,
                buttonBackgroundColor.g,
                buttonBackgroundColor.b,
                .68f
            ); 
        }

        iconImage.color = imageColor;
        backgroundButtonImage.color = buttonBackgroundColor;
    }
}
