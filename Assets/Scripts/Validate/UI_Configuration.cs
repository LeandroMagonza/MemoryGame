#if UNITY_EDITOR

using System.Data;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UI_Configuration : MonoBehaviour
{
    [SerializeField] private bool onValidate = false;
    [Space(5)]
    [SerializeField] private TextConfiguration[] textToConfig;
    [SerializeField] private ImageConfiguration[] imageToConfig;

    private void OnValidate()
    {
        if (onValidate)
            ValidateData();
    }
    [ContextMenu("ValidateData")]
    public void ValidateData()
    {
        foreach (var config in textToConfig)
        {
            config.Initialize();
        }
        foreach (var config in imageToConfig)
        {
            config.Initialize();
        }
    }
}
[System.Serializable]
public struct UI_ConfigurationData
{
    [SerializeField] public string name;
    [Space(10)]
    [SerializeField] public TextConfiguration[] textToConfig;

    [Space(10)]
    [SerializeField] public ImageConfiguration[] imageToConfig;

    public void Initialize()
    {
        foreach (var config in textToConfig)
        {
            config.Initialize();
        }
        foreach (var config in imageToConfig)
        {
            config.Initialize();
        }
    }
}

[System.Serializable]
public struct TextConfiguration
{
    [SerializeField] public string name;
    [SerializeField] public TMP_FontAsset fontToSet;
    [Space(10)]
    [SerializeField] public TextDataConfiguration[] dataToSet;

    public void Initialize()
    {
        foreach (TextDataConfiguration textData in dataToSet)
        {
            textData.Set(this);
        }
    }
}

[System.Serializable]
public struct TextDataConfiguration
{
    [SerializeField] public string name;
    [SerializeField] public Color colorToSet;
    [Space(5)]
    [SerializeField] public OutlineDataConfiguration outlineToSet; 
    [Header("TMP Component To Change")]
    [SerializeField] public TextMeshProUGUI[] textsComponentsToChange;

    public void Set(TextConfiguration configuration)
    {
        foreach (TextMeshProUGUI uGUI in  textsComponentsToChange)
        {
            uGUI.font = configuration.fontToSet;
            uGUI.color = colorToSet;
            CanvasRenderer canvasRenderer = uGUI.canvasRenderer;
            if (canvasRenderer != null )
            {
                uGUI.outlineWidth = outlineToSet.outlineThicknessToSet;
                uGUI.outlineColor = outlineToSet.outlineColorToSet;
            }
        }
    }
}
[System.Serializable]
public struct ImageConfiguration
{
    [SerializeField] public string name;
    [SerializeField] public Sprite spriteToSet;
    [Space(10)]
    [SerializeField] public ImageDataConfiguration[] dataToSet;

    public void Initialize()
    {
        foreach (ImageDataConfiguration imageData in dataToSet)
        {
            imageData.Set(this);
        }
    }
}
[System.Serializable]
public struct ImageDataConfiguration
{
    [SerializeField] public string name;
    [SerializeField] public Color colorToSet;
    [Space(5)]
    [SerializeField] public OutlineDataConfiguration outlineToSet;
    [Header ("Images Component To Change")]
    [SerializeField] public Image[] imagesComponentsToChange;

    public void Set(ImageConfiguration configuration)
    {
        foreach (Image image in imagesComponentsToChange)
        {
            image.sprite = configuration.spriteToSet;

            image.color = colorToSet;
            Outline outline = image.GetComponent<Outline>();
            if (outline is not Outline)
                outline = image.AddComponent<Outline>();
            outline.effectDistance = new Vector2 (outlineToSet.outlineThicknessToSet, -outlineToSet.outlineThicknessToSet);
            outline.effectColor = outlineToSet.outlineColorToSet;
        }
    }
}
[System.Serializable]
public struct OutlineDataConfiguration
{


    [SerializeField] public float outlineThicknessToSet;
    [SerializeField] public Color outlineColorToSet;
}

#endif