using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FadeOut : MonoBehaviour
{
    public TextMeshProUGUI textToFadeOut;

    public float fadeSpeed;

    public Color correctColor;
    public Color incorrectColor;
    // Start is called before the first frame update
    // Update is called once per frame
    public void SetAmountOfGuessesAndShowText(int amountOfGuesses,bool correctGuess) {
        textToFadeOut.text = amountOfGuesses.ToString();
        if (correctGuess) {
            textToFadeOut.color = correctColor;
        }
        else {
            textToFadeOut.color = incorrectColor;
        }
    }

    private void FixedUpdate() {
        textToFadeOut.color = new Color(
            textToFadeOut.color.r,
            textToFadeOut.color.g,
            textToFadeOut.color.b,
            textToFadeOut.color.a - fadeSpeed * Time.deltaTime
        );
        if (textToFadeOut.color.a*10 % 10 == 0) {
            Debug.Log(textToFadeOut.color.a);
        }
    }
}
