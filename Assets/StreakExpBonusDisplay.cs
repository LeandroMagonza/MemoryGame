using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StreakExpBonusDisplay : MonoBehaviour
{
    public TextMeshProUGUI expDisplay;
    [SerializeField] public Image image;
    // Start is called before the first frame update
    public void OnEnable() {
        UpdateExpText();
    }

    public void UpdateExpText() {
        expDisplay.text = Mathf.RoundToInt(GameManager.Instance.GetStreakBonusPercentage()*100).ToString();
    }

}
