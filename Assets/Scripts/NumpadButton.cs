using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class NumpadButton : MonoBehaviour {
    // en numpad el number es la cantidad de apariciones, en en quiz option pad es el numero de opcion a elegir, el indice en quizoptions
    public int number;
    public TextMeshProUGUI _numberText;
    public Button _button;
    // Start is called before the first frame update
    void Awake()
    {
        
        //_numberText = GetComponentInChildren<TextMeshProUGUI>();
        _numberText = transform.Find("TextNumpad").GetComponent<TextMeshProUGUI>();
        _button = GetComponent<Button>();
        SetText(number.ToString());
        gameObject.name = "NumpadButtonN " + number;
        GetComponent<Button>().onClick.AddListener(OnClick);
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
