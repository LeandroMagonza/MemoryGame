using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class BuyPackButtonTest : MonoBehaviour {
    public int packNumber;
    public int maxPackNumber = 4;
    public TextMeshProUGUI[] numbers;
    public TextMeshProUGUI stickerCostDisplay;
    public TextMeshProUGUI stickerSelectedDisplay;

    //Se comenta todo lo de los packs porque ya no hay stages de donde sacar la data, y va a haber que cambiarse toda la logica si se vuelve a utilizar
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
        //HighlightPackSelected();

    }

    public void OnClick()
    {
        if (GameManager.Instance.disableInput) {
            CustomDebugger.Log("ïnput is disabled");
            return;
        }
        CustomDebugger.Log("open pack number "+packNumber);
        //StartCoroutine(PackManager.Instance.OpenPack(packNumber));
    }

    public void SetPackNumber(int packNumber)
    {
        this.packNumber = packNumber;
        //HighlightPackSelected();
    }

    public void NextPackNumber()
    {
        packNumber++;
        if (packNumber > maxPackNumber)
        {
            packNumber = maxPackNumber;
        }
        //HighlightPackSelected();

    }

    public void PreviousPackNumber()
    {
        packNumber--;
        if (packNumber < 0) 
            packNumber = 0;
        //HighlightPackSelected();

    }

    // public void HighlightPackSelected()
    // {
    //     foreach (TextMeshProUGUI number in numbers)
    //     {
    //         number.color = Color.white;
    //     }
    //     numbers[packNumber].color = new Color(1f, 0.5f, 0);
    //     
    //     stickerCostDisplay.text = "BUY\n"+(PersistanceManager.Instance.stages[packNumber].packCost).ToString();
    //     stickerSelectedDisplay.text = "Current Pack Selected: "+ PersistanceManager.Instance.stages[packNumber].title.ToString();
    // }
}