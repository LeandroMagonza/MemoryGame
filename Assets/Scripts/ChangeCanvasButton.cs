using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeCanvasButton : MonoBehaviour {
    public CanvasName canvasToSet;
    public GameClip clip;
    /*
    public Image iconImage;
    public Image backgroundImage;

    public static Dictionary<CanvasName, (IconName, Color)> canvasButtonStyleConfig = new ()
    {
        { CanvasName.MENU, (IconName.MENU, Color.yellow) },
        { CanvasName., (IconName.MENU, Color.yellow) },
        
    };
    */
    public virtual void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
        //ConfigStyle(canvasName);
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
    PLAYER_LEVEL,
    CONFIG,
    RETURN,
    LANGUAGE,
    SELECT_UPGRADE,
    SHOP,
    EXIT,
    LOADING
}

