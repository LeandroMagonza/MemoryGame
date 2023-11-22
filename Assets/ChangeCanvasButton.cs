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
        Debug.Log("Pressed" +name);
        if (canvasToSet ==  null) {
            throw new Exception("Canvas NotImplementedException set");
        }
        CanvasManager.Instance.ChangeCanvas(canvasToSet);  
    }

}
