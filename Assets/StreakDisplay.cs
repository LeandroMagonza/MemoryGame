using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StreakDisplay : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI numberText;
    [SerializeField] public Image image;

    public void UpdateStreakText(int streakAmount)
    {
        if (streakAmount < 1) {
            image.gameObject.SetActive(false);
        }
        else {
            image.gameObject.SetActive(true);
        }
        
        numberText.text = streakAmount.ToString();
    }
}