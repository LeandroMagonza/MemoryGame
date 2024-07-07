using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


public class FactoriesBarUi : MonoBehaviour
{
    public List<FactoryConsumableDisplay> unorganizedDisplaysFactories = new List<FactoryConsumableDisplay>();
    public List<FactoryConsumableDisplay> unorganizedDisplaysInventory = new List<FactoryConsumableDisplay>();
    public Dictionary<ConsumableID, FactoryConsumableDisplay> consumableDisplaysFactories = new();
    public Dictionary<ConsumableID, FactoryConsumableDisplay> consumableDisplaysInventory = new();
    [FormerlySerializedAs("bar")] public GameObject bars;
    public GameObject factoryAndInventoryIcons;
    public GameObject toggleBarButton;
    public float slideBarSpeed = 500f; // Speed in units per second

    private bool isSliding = false; // Flag to prevent multiple toggles during slide

    public void SetConsumablesToClaim(ConsumableID consumableID, int consumablesToClaim)
    {
        if (!consumableDisplaysFactories.ContainsKey(consumableID)) { return; }
        
        //consumableDisplays[consumableID].
    }

    public void Start()
    {
        foreach (var VARIABLE in unorganizedDisplaysFactories)
        {
            VARIABLE.isFactory = true;
            consumableDisplaysFactories.Add(VARIABLE.consumableID, VARIABLE);
        }
        foreach (var VARIABLE in unorganizedDisplaysInventory) {
            VARIABLE.isFactory = false;
            consumableDisplaysInventory.Add(VARIABLE.consumableID, VARIABLE);
        }

        ConsumableFactoryManager.Instance.factoriesBarUi = this;
        // Ver cual habilitar según upgrades
        toggleBarButton.GetComponent<Button>().onClick.AddListener(() => StartCoroutine(SlideBar()));
        UpdateDisplays();
    }

    public void UpdateDisplays()
    {
        foreach (var consumableAndDisplay in consumableDisplaysFactories)
        {
            consumableAndDisplay.Value.UpdateTexts();
        }
        foreach (var consumableAndDisplay in consumableDisplaysInventory)
        {
            consumableAndDisplay.Value.UpdateTexts();
        }
    }
    
    private IEnumerator SlideBar()
    {
        if (isSliding)
        {
            yield break; // If already sliding, do nothing
        }

        isSliding = true;

        RectTransform barRectTransform = bars.GetComponent<RectTransform>();
        RectTransform toggleButtonRectTransform = toggleBarButton.GetComponent<RectTransform>();
        RectTransform factoryAndInventoryIconsRectTransform = factoryAndInventoryIcons.GetComponent<RectTransform>();

        Vector3 startPosition = barRectTransform.localPosition;
        Vector3 endPosition;

        if (startPosition.x < 0)
        {
            endPosition = new Vector3(0, startPosition.y, startPosition.z);
            toggleBarButton.transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            float toggleButtonLeftEdge = toggleButtonRectTransform.localPosition.x - (toggleButtonRectTransform.rect.width / 2);
            float factoryAndInventoryIconsRightEdge = factoryAndInventoryIconsRectTransform.localPosition.x + (factoryAndInventoryIconsRectTransform.rect.width / 2);

            float targetPositionX = barRectTransform.localPosition.x - (toggleButtonLeftEdge - factoryAndInventoryIconsRightEdge);
            endPosition = new Vector3(targetPositionX, startPosition.y, startPosition.z);
            toggleBarButton.transform.localScale = new Vector3(-1, 1, 1);
        }

        float elapsedTime = 0;
        float totalDistance = Vector3.Distance(startPosition, endPosition);

        while (elapsedTime < totalDistance / slideBarSpeed)
        {
            barRectTransform.localPosition = Vector3.Lerp(startPosition, endPosition, (elapsedTime * slideBarSpeed) / totalDistance);
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        barRectTransform.localPosition = endPosition;
        isSliding = false;
    }
}
