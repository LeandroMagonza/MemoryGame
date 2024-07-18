using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectLanguageButton : MonoBehaviour {
    public string language;
    public Image flagImage;

    public void SetLanguage(string language) {
        this.language = language;
        flagImage.sprite = LocalizationManager.Instance.GetLanguageIcon(language);
    }

    public void SelectLanguage() {
        StartCoroutine(LocalizationManager.Instance.ChangeLanguage(language));
        AudioManager.Instance.PlayClip(GameClip.enterStages);

    }
}
