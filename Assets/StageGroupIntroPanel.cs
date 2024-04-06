using System;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
public class StageGroupIntroPanel : MonoBehaviour
{
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI groupNameText;
    public Image groupImageColor;
    public Image background;
    public float shrinkSpeed = .9f;

    public void SetGroup((string name, Color color) group) {
        background.color = Color.black;
        countdownText.text = "3";
        groupNameText.text = group.name; 
        groupImageColor.color = group.color; 
    }
    public void SetCountdown(int number) {
        countdownText.text = number.ToString();
        countdownText.transform.localScale = Vector3.one;
    }

    private void FixedUpdate() {
        var localScale = countdownText.transform.localScale;
        localScale = new Vector3(
            localScale.x * shrinkSpeed,
            localScale.y * shrinkSpeed,
             localScale.z
            );
        countdownText.transform.localScale = localScale;
    }
}
