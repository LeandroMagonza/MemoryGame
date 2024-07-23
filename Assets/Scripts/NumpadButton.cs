using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Analytics;
using UnityEngine;
using UnityEngine.UI;

public class NumpadButton : MonoBehaviour
{
    public int number;
    [SerializeField] public TextMeshProUGUI _numberText;
    [SerializeField] public Button _button;
    [SerializeField] public GameObject bombFx;
    [SerializeField] public Color originalColor;
    [SerializeField] public Color correctGuessColor;
    public float waitTimeBeforeLerp = 0.5f; // Tiempo de espera antes de empezar el Lerp
    public float lerpDuration = 1.0f; // Duración del Lerp

    private bool originalColorSet = false; // Bandera para controlar la inicialización del color original

    void Awake()
    {
        SetText(number.ToString());
        gameObject.name = "NumpadButtonN " + number;
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    private void OnEnable() {
        StartCoroutine(RefreshSize());
    }

    private void CheckAndSetColor() {
        if (!originalColorSet) {
            originalColor = GetComponent<Image>().color;
            originalColorSet = true;
            CustomDebugger.Log(originalColor, DebugCategory.POWER_BUTTONS);
        }
    }

    [ContextMenu("RefreshSize")]
    public IEnumerator RefreshSize()
    {
        yield return new WaitForEndOfFrame();
        RectTransform rectTransform = GetComponent<RectTransform>();
        RectTransform rectReference = GameManager.Instance.gameCanvas.numpad.numpadButtons[0].GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(rectReference.sizeDelta.x, rectTransform.sizeDelta.y);
    }

    public void OnClick()
    {
        if (GameManager.Instance.gameEnded || GameManager.Instance.disableInput)
        {
            return;
        }
        CustomDebugger.Log("Clicked number " + number);
        StartCoroutine(GameManager.Instance.ProcessTurnAction(number));
    }

    public void SetText(string textToSet)
    {
        _numberText.text = textToSet;
    }

    public IEnumerator AnimateCut(GameObject referenceObject)
    {
        GameObject animationImage = new GameObject("CutAnimationImage");
        Image img = animationImage.AddComponent<Image>();
        img.sprite = ItemHelper.GetIconSprite(IconName.CUT);
        RectTransform rectTransform = animationImage.GetComponent<RectTransform>();
        rectTransform.SetParent(GameManager.Instance.gameCanvas.transform, false);

        Vector3 startPosition = referenceObject.transform.position;
        Vector3 endPosition = transform.position;
        rectTransform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        rectTransform.position = startPosition;

        Quaternion initialRotation = Quaternion.LookRotation(Vector3.forward, endPosition - startPosition);
        rectTransform.rotation = initialRotation;

        float duration = 0.2f; // Duration of the animation
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            rectTransform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;

            yield return new WaitForFixedUpdate();
        }

        rectTransform.position = endPosition;

        Destroy(animationImage);

        _numberText.color = Color.red;
        _button.interactable = false;
    }

    public void UpdateButtonColor(int correctGuess)
    {
        CheckAndSetColor();
        bool startingLevel = GameManager.Instance.selectedDifficulty == 2 && GameManager.Instance.selectedLevel == 4;

        if (number == correctGuess && startingLevel && gameObject.activeInHierarchy)
        {
            StartCoroutine(LerpToColor(correctGuessColor));
        }
        else
        {
            GetComponent<Image>().color = originalColor;
        }
    }

    private IEnumerator LerpToColor(Color targetColor)
    {
        // Set the initial color to originalColor
        Image buttonImage = GetComponent<Image>();
        buttonImage.color = originalColor;

        // Wait for the specified wait time before starting the Lerp
        yield return new WaitForSeconds(waitTimeBeforeLerp);

        float elapsedTime = 0;
        while (elapsedTime < lerpDuration)
        {
            buttonImage.color = Color.Lerp(originalColor, targetColor, elapsedTime / lerpDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        buttonImage.color = targetColor; // Ensure the color is set to the target color at the end
    }
}
