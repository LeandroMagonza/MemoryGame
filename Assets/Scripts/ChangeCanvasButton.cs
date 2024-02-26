using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeCanvasButton : MonoBehaviour {
    public Canvas canvasToSet;
    public GameClip clip;
    public virtual void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    public virtual void OnClick() {
        
        AdmobAdsManager.Instance.ShowInterstitialAd();
        
        CustomDebugger.Log("Pressed" +name);
        AudioManager.Instance.PlayClip(clip);
        if (canvasToSet ==  null) {
            CustomDebugger.Log("Canvas NotImplementedException set");
            return;
        }
        CanvasManager.Instance.ChangeCanvas(canvasToSet);  
    }


}
