using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class NumpadButton : MonoBehaviour {
    public int number;
    private TextMeshProUGUI _numberText; 
    // Start is called before the first frame update
    void Start()
    {
        
        //_numberText = GetComponentInChildren<TextMeshProUGUI>();
        _numberText = transform.Find("TextNumpad").GetComponent<TextMeshProUGUI>();
        _numberText.text = number.ToString();
        gameObject.name = "NumpadButtonN " + number;
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    public void OnClick()
    {
        if (GameManager.Instance.gameEnded) {
            return;
        }
       Debug.Log("Clicked number "+number);
       GameManager.Instance.ProcessGuess(number);
    }
}
