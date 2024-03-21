using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeCanvasButton : MonoBehaviour {
    public CanvasName canvasToSet;
    public GameClip clip;
    public virtual void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    public virtual void OnClick() {
        
        AdmobAdsManager.Instance.ShowInterstitialAd();
        
        CustomDebugger.Log("Pressed" +name);
        AudioManager.Instance.PlayClip(clip);
        CanvasManager.Instance.ChangeCanvas(canvasToSet);  
    }


}
public enum CanvasName
{ 
    NO_CANVAS = 0,
    MENU,
    GAME,
    SELECT_STAGE,
    SHOP,
    CONFIG,
    RETURN,
    LANGUAGE
}