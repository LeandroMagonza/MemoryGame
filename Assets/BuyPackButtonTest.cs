using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class BuyPackButtonTest : MonoBehaviour {
    public int packNumber;
    public int maxPackNumber = 4;
    public TextMeshProUGUI[] numbers;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
        HighlightPackSelected();

    }

    public void OnClick()
    {
        if (GameManager.Instance.disableInput) {
            Debug.Log("ïnput is disabled");
            return;
        }
        Debug.Log("open pack number "+packNumber);
        StartCoroutine(PackManager.Instance.OpenPack(packNumber));
    }

    public void SetPackNumber(int packNumber)
    {
        this.packNumber = packNumber;
        HighlightPackSelected();
    }

    public void NextPackNumber()
    {
        packNumber++;
        if (packNumber > maxPackNumber)
        {
            packNumber = maxPackNumber;
        }
        HighlightPackSelected();

    }

    public void PreviousPackNumber()
    {
        packNumber--;
        if (packNumber < 0) 
            packNumber = 0;
        HighlightPackSelected();

    }

    public void HighlightPackSelected()
    {
        foreach (TextMeshProUGUI number in numbers)
        {
            number.color = Color.white;
        }
        numbers[packNumber].color = new Color(1f, 0.5f, 0);
    }
}