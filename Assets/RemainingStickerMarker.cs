using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class RemainingStickerMarker : MonoBehaviour {
    public TextMeshProUGUI number;
    public Image asd;

    public void DisplayText(bool display) {
        number.gameObject.SetActive(display);
    }
    public void SetText(string text) {
        CustomDebugger.Log("marker set text called with "+text);
        number.text = text;
    }
    
}
