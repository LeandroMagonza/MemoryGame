using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct ConsumableButtonData
{
    [SerializeField] public string name;
    [SerializeField] public ConsumableID ID;
    [SerializeField] public Button button;
    [SerializeField] public TextMeshProUGUI text;
    [SerializeField] public GameClip gameClip;
    [SerializeField] public GameObject peekCanvas;
    public void OpenStickerPanel(int stage)
    {

        StageManager.Instance.stickerHolder.transform.GetComponentsInChildren<Sticker>();
        foreach (var stickerID in StageManager.Instance.stages[stage].stickers)
        {
            Sticker display = StickerManager.Instance.GetStickerHolder();
            StickerData stickerData = StickerManager.Instance.GetStickerDataFromSetByStickerID(StageManager.Instance.stages[stage].stickerSet, stickerID);
            display.SetStickerData(stickerData);
            // esto era para cuando tenias que desbloquear los stages, los lockeados eran los stickers que no tenias, pero ahora no se loquean asi que el boton
            // trae todos los stickers siempre
            /*if (stickerData.amountOfDuplicates == 0) {
                display.ConfigureLocked();
            }
            else {*/
            //}
            display.ConfigureForPack();

            display.transform.SetParent(StageManager.Instance.stickerHolder.transform);
            display.transform.localScale = Vector3.one;
        }
        StageManager.Instance.stickerPanel.SetActive(true);
    }
    public void SetButtonText(ConsumableID consumableID)
    {
        if (text != null)
        {

            if (GameManager.Instance.matchInventory.ContainsKey(consumableID))
            {
                text.text = GameManager.Instance.matchInventory[consumableID].current.ToString();
            }
            else
            {
                CustomDebugger.Log("not implemented");
                text.text = "0";
            }
        }
        text.color = text.text.Equals("0") ? Color.white : Color.yellow;
    }
    public void SetInteractable(ConsumableID consumableID)
    {
        if (button != null)
        {
            if (GameManager.Instance.matchInventory.ContainsKey(consumableID))
            {

                button.interactable = GameManager.Instance.matchInventory[consumableID].current > 0;
            }
            else
            {
                button.interactable = false;
            }
            button.GetComponent<Image>().color = button.interactable ? Color.white : Color.gray;
        }
    }


}
public class GameCanvas : MonoBehaviour
{
    private GameManager GameManager => GameManager.Instance;
    public static GameCanvas Instance;    
    [SerializeField] public GameObject pausePanel;
    public GameCanvasHeader gameCanvasHeader;
    public RemainingBarController barController => gameCanvasHeader.barController;

    private GameCanvas()
    {
        if (Instance == null)
            Instance = this;
    }
    private void OnEnable()
    {
        if (GameManager.startScale != Vector3.zero)
            GameManager.stickerDisplay.spriteHolder.transform.localScale = GameManager.startScale;
    }
}
