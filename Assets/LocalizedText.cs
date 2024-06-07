using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LocalizedText : MonoBehaviour {
    TextMeshProUGUI localizedText;
    [SerializeField] public GameText gameText;
    // Start is called before the first frame update

    // Update is called once per frame
    public void UpdateText() {
        localizedText = GetComponent<TextMeshProUGUI>();
        string textToUpdate = LocalizationManager.Instance.GetGameText(gameText);
        if (textToUpdate != "") {
            localizedText.text = textToUpdate;
        }
        else {
            CustomDebugger.LogError(LocalizationManager.Instance.currentLanguage+" doesnt have a "+gameText+" text ");
            textToUpdate = LocalizationManager.Instance.GetGameText(gameText,"english");
            if (textToUpdate != "") {
                localizedText.text = textToUpdate;
            }
            else {
                CustomDebugger.LogError("English doesnt have a "+gameText+" text ");
            }
        }
        
    }
}
