using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Analytics;
using UnityEngine;
using UnityEngine.UI;
public class NumpadButton : MonoBehaviour {
    // en numpad el number es la cantidad de apariciones, en en quiz option pad es el numero de opcion a elegir, el indice en quizoptions
    public int number;
    [SerializeField] public TextMeshProUGUI _numberText;
    [SerializeField] public Button _button;
    [SerializeField] public GameObject bombFx;
    // Start is called before the first frame update

    void Awake()
    {
        //_numberText = GetComponentInChildren<TextMeshProUGUI>();
        // _numberText = transform.Find("TextNumpad").GetComponent<TextMeshProUGUI>();
        // _button = GetComponent<Button>();
        SetText(number.ToString());
        gameObject.name = "NumpadButtonN " + number;
        GetComponent<Button>().onClick.AddListener(OnClick);
    }
    private void OnEnable()
    {
        StartCoroutine(RefreshSize());
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
        if (GameManager.Instance.gameEnded || GameManager.Instance.disableInput) {
            return;
        }
       CustomDebugger.Log("Clicked number "+ number);
       StartCoroutine(GameManager.Instance.ProcessTurnAction(number));
    }
    
    public void SetText(string textToSet)
    {
        _numberText.text = textToSet;
    }
    public IEnumerator AnimateCut(GameObject referenceObject) {
        //float rotationSpeed = 10000f;
        GameObject animationImage = new GameObject("CutAnimationImage");
        Image img = animationImage.AddComponent<Image>();
        img.sprite = ItemHelper.GetIconSprite(IconName.CUT);
        // img.color = Color.red; // Set the color of the animation image
        RectTransform rectTransform = animationImage.GetComponent<RectTransform>();
        rectTransform.SetParent(GameManager.Instance.gameCanvas.transform, false);

        Vector3 startPosition = referenceObject.transform.position;
        Vector3 endPosition = transform.position;
        rectTransform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        rectTransform.position = startPosition;

        // Calculate the initial rotation of the image
        Quaternion initialRotation = Quaternion.LookRotation(Vector3.forward, endPosition - startPosition);
        rectTransform.rotation = initialRotation;

        float duration = 0.2f; // Duration of the animation
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            rectTransform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;

            // Rotate the image continuously
            //rectTransform.Rotate(Vector3.forward * (rotationSpeed * Time.deltaTime));

            yield return new WaitForFixedUpdate();
        }

        rectTransform.position = endPosition;

        Destroy(animationImage);

        // Set button as cut
        _numberText.color = Color.red;
        _button.interactable = false;
    }


}
