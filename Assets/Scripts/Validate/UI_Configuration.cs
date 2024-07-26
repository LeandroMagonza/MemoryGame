/*
using System.Data;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UI_Configuration : MonoBehaviour
{
    public bool onValidate = false;
    [Space(5)]
    public TextConfiguration[] textToConfig;
    public ImageConfiguration[] imageToConfig;

    private void OnValidate()
    {
        #if UNITY_EDITOR
            if (onValidate) ValidateData();
        #endif      
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
    public string name;
    [Space(10)]
    public TextConfiguration[] textToConfig;

    [Space(10)]
    public ImageConfiguration[] imageToConfig;

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
    public string name;
    public TMP_FontAsset fontToSet;
    [Space(10)]
    public TextDataConfiguration[] dataToSet;

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
    public string name;
    public Color colorToSet;
    [Space(5)]
    public OutlineDataConfiguration outlineToSet; 
    [Header("TMP Component To Change")]
    public TextMeshProUGUI[] textsComponentsToChange;

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
    public string name;
    public Sprite spriteToSet;
    [Space(10)]
    public ImageDataConfiguration[] dataToSet;

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
    public string name;
    public Color colorToSet;
    [Space(5)]
    public OutlineDataConfiguration outlineToSet;
    [Header ("Images Component To Change")]
    public Image[] imagesComponentsToChange;

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
    public float outlineThicknessToSet;
    public Color outlineColorToSet;
}

*/