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
    public Canvas initialCanvas;
    public List<Canvas> allCanvas;

    private void Start()
    {
        ChangeCanvas(initialCanvas);
    }


    public void ChangeCanvas(Canvas canvasToSet) {
        foreach (var VARIABLE in allCanvas) {
            VARIABLE.gameObject.SetActive(false);//disable all canvas
        }
        canvasToSet.gameObject.SetActive(true);
        //enable canvasToSet
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
}
