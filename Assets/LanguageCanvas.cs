using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LanguageCanvas : MonoBehaviour {
    public GameObject languageButtonHolder;
    public GameObject selectLanguageButtonPrefab;
    
    public void UpdateLanguageButtons() {

        int languageButtonIndex = 0;
        foreach(Transform child in languageButtonHolder.transform)
        {
            child.gameObject.SetActive(false);
        }
        foreach (var language in LocalizationManager.Instance.languagesList) {
            Transform selectButtonTransform;
            if (transform.childCount <= languageButtonIndex) {
                selectButtonTransform = Instantiate(selectLanguageButtonPrefab,languageButtonHolder.transform).transform;
            }
            else {
                selectButtonTransform = languageButtonHolder.transform.GetChild(languageButtonIndex);
            }
            
            selectButtonTransform.GetComponent<SelectLanguageButton>().SetLanguage(language);
            selectButtonTransform.gameObject.SetActive(true);
            languageButtonIndex++;
        }
        
    }

    public void UpdateActiveLanguageButtonMarker(string selectedLanguage) {
        CustomDebugger.Log("UpdateActiveLanguageButtonMarker");
        foreach(Transform child in languageButtonHolder.transform)
        {
            SelectLanguageButton selectedLanguageButton = child.gameObject.GetComponent<SelectLanguageButton>();
            CustomDebugger.Log("found button "+selectedLanguageButton.language);

            if (selectedLanguageButton.language == selectedLanguage) {
                selectedLanguageButton.GetComponent<Image>().enabled = true;
                CustomDebugger.Log("enabled marker"+selectedLanguageButton.language);
                
            }
            else {
                selectedLanguageButton.GetComponent<Image>().enabled = false;
                CustomDebugger.Log("disabled marker "+selectedLanguageButton.language);
            }
        }
    }
}
