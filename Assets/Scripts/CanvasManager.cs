using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    private void DestroySelf() {
        if (Application.isPlaying)
            Destroy(this);
        else
            DestroyImmediate(this);
    }
    #endregion


    public Image backgroundImage;
    public CanvasName initialCanvas = CanvasName.MENU;
    public Dictionary<CanvasName,Canvas> allCanvas = new Dictionary<CanvasName, Canvas>();

    public Canvas menuCanvas;
    public Canvas selectStageCanvas;
    public Canvas gameCanvas;
    public Canvas shopCanvas;
    private void Start()
    {
        
        allCanvas.Add(CanvasName.MENU,menuCanvas);
        allCanvas.Add(CanvasName.SELECT_STAGE,selectStageCanvas);
        allCanvas.Add(CanvasName.GAME,gameCanvas);
        allCanvas.Add(CanvasName.SHOP,shopCanvas);
        
        ChangeCanvas(initialCanvas);
    }


    public void ChangeCanvas(CanvasName canvasToSet) {
        if (canvasToSet == CanvasName.NO_CANVAS) return; 
        
        foreach (var VARIABLE in allCanvas) { 
            //disable all canvas
            VARIABLE.Value.gameObject.SetActive(false);
        }
        
        allCanvas[canvasToSet].gameObject.SetActive(true);
    }

    public void SetMainMenuCanvas(string HexbackgoundColor)
    {
        Color color = Color.white;
        if (ColorUtility.TryParseHtmlString("#" + HexbackgoundColor, out Color colorValue))
        {
            color = colorValue;
        }
        Camera.main.backgroundColor = color;
        Sprite backgound = Resources.Load<Sprite>(StageManager.Instance.gameVersion + "/background");
        if (backgound is Sprite)
            backgroundImage.sprite = backgound;

    }

    private void Update() {
        if (Input.GetKeyUp(KeyCode.Escape) ) {
            if (allCanvas[CanvasName.MENU].gameObject.activeInHierarchy) {
                CustomDebugger.Log("Application quit called");
                Application.Quit();
            }
        }
    }

}
