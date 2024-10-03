using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class TimerEnabledButton : MonoBehaviour
{
    public float timer = 8f;
    public float maxTimer = 8f;
    public TextMeshProUGUI buttonText;
    // Start is called before the first frame update
    private void OnEnable() {
        gameObject.GetComponent<Button>().interactable = false;
        buttonText.color = new Color(buttonText.color.r, buttonText.color.g, buttonText.color.b, 0);
        timer = maxTimer;
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    // Update is called once per frame
    void Update()
    {
        if (timer < 0)
        {
            gameObject.GetComponent<Button>().interactable = true;
            buttonText.color = new Color(buttonText.color.r, buttonText.color.g, buttonText.color.b,1 );
            return;
        }
        timer -= Time.deltaTime;
    }
    public void OnClick()
    {
        GameManager.Instance.CloseTutorialPanel();
    }
}
