using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class LifeCounter : MonoBehaviour
{
    public int lives;

    public List<GameObject> hearts;
    // Start is called before the first frame update

    public void LoseLive() {
        lives--;
        UpdateHearts();
        if (lives <= 0) {
            GameManager.Instance.EndGame();
        }
    }

    private void UpdateHearts() {
        int heartIndex = 0;
        foreach (var heart in hearts) {
            if (heartIndex >= lives) {
                heart.GetComponent<Image>().color = new Color(
                    heart.GetComponent<Image>().color.r,
                    heart.GetComponent<Image>().color.g,
                    heart.GetComponent<Image>().color.b,
                    255);
            }
            else {
                heart.GetComponent<Image>().color = new Color(
                    heart.GetComponent<Image>().color.r,
                    heart.GetComponent<Image>().color.g,
                    heart.GetComponent<Image>().color.b,
                    0);
            }

            heartIndex++;
        }
    }

    public void ResetLives() {
        lives = 3;
        UpdateHearts();
    }

    public void GainLive() {
        if (lives < hearts.Count) {
            lives++;
            UpdateHearts();
        }

    }
}

public static class MyExtensions
{
    private static readonly System.Random rng = new System.Random();
	
    //Fisher - Yates shuffle
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}