
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class PowerButton : MonoBehaviour
{
    //[SerializeField] public string name;
    [SerializeField] public ConsumableID consumableID;
    [SerializeField] public Button button;
    [SerializeField] public TextMeshProUGUI text;
    [SerializeField] public GameClip gameClip;
    public bool markedActive = false;
    //[SerializeField] public GameObject peekCanvas;
    public PowerPanelButtonHolder powerPanelButtonHolder;
    public void SetButtonText() {
        if (text == null) return;

        if (GameManager.Instance.matchInventory.ContainsKey(consumableID))
        {
            text.text = GameManager.Instance.matchInventory[consumableID].current.ToString();
        }
        else
        {
            CustomDebugger.Log("not implemented");
            text.text = "0";
        }
        text.color = text.text.Equals("0") ? Color.white : Color.yellow;
    }
    public void SetInteractable()
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
            button.GetComponent<Image>().color = button.interactable ? ((markedActive)?Color.yellow:Color.white) : Color.gray;
        }
    }
    
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }
    public void OnClick()
    {
        if (GameManager.Instance.disableInput) {
            CustomDebugger.Log("Input is disabled");
            return;
        }
        powerPanelButtonHolder.UsePower(consumableID);
    }


    public void MarkActive(bool active) {
        markedActive = active;
        CustomDebugger.Log(active+" marked button as");
        button.GetComponent<Image>().color = button.interactable ? ((markedActive)?Color.yellow:Color.white) : Color.gray;
        CustomDebugger.Log(button.GetComponent<Image>().color+" button color");
    }
 // public void Update() {
    //     if (markedActive) {
    //         throw new NotImplementedException();
    //     }
    // }
   
}