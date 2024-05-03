using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class NumpadButton : MonoBehaviour {
    // en numpad el number es la cantidad de apariciones, en en quiz option pad es el numero de opcion a elegir, el indice en quizoptions
    public int number;
    [SerializeField] public TextMeshProUGUI _numberText;
    [SerializeField] public Button _button;
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
        RectTransform rectReferrence = GameManager.Instance.gameCanvas.numpad.numpadButtons[0].GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(rectReferrence.sizeDelta.x, rectTransform.sizeDelta.y);
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
}
