using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class BuyPackButtonTest : MonoBehaviour {
    public int packNumber;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    public void OnClick()
    {
        if (GameManager.Instance.disableInput) {
            CustomDebugger.Log("ïnput is disabled");
            return;
        }
        CustomDebugger.Log("open pack number "+packNumber);
        StartCoroutine(PackManager.Instance.OpenPack(packNumber));
    }
}