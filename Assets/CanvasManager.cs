using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    #region Singleton
    
    private static CanvasManager _instance;
    
    public static CanvasManager Instance {
        get {
            if (_instance == null)
                _instance = FindObjectOfType<CanvasManager>();
            if (_instance == null)
                Debug.LogError("Singleton<" + typeof(CanvasManager) + "> instance has been not found.");
            return _instance;
        }
    }
    protected void Awake() {
        if (_instance == null) {
            _instance = this as CanvasManager;
        }
        else if (_instance != this)
            DestroySelf();
    }
    #endregion

    public List<Canvas> allCanvas;
    private void DestroySelf() {
        if (Application.isPlaying)
            Destroy(this);
        else
            DestroyImmediate(this);
    }
    

    public void ChangeCanvas(Canvas canvasToSet) {
        foreach (var VARIABLE in allCanvas) {
            VARIABLE.gameObject.SetActive(false);//disable all canvas
        }
        canvasToSet.gameObject.SetActive(true);
        //enable canvasToSet
    }
}
