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
