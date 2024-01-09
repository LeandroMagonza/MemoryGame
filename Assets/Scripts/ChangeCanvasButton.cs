using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeCanvasButton : MonoBehaviour {
    public Canvas canvasToSet;
    public virtual void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    public virtual void OnClick() {
        CustomDebugger.Log("Pressed" +name);
        if (canvasToSet ==  null) {
            CustomDebugger.Log("Canvas NotImplementedException set");
            return;
        }
        CanvasManager.Instance.ChangeCanvas(canvasToSet);  
    }
    public void PlayOneShotOnManager(AudioClip clip) {
        GameManager.Instance.audioSource.PlayOneShot(clip);
    }

}
