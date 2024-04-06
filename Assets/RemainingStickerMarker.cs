using UnityEngine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class RemainingStickerMarker : MonoBehaviour {
    public TextMeshProUGUI number;
    [FormerlySerializedAs("asd")] public Image markerIcon;

    public void DisplayText(bool display) {
        number.gameObject.SetActive(display);
    }
    public void SetText(string text) {
        CustomDebugger.Log("marker set text called with "+text);
        number.text = text;
    }
    
}
