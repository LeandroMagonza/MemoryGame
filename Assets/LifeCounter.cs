using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class LifeCounter : MonoBehaviour
{
    public GameObject heartPrefab;
    public Transform lifeCounterContainer;
    public int lives;
    public int heartCount = 3;

    public List<GameObject> hearts;
    // Start is called before the first frame update
    private void Start()
    {
        SetLives();
        UpdateHearts();
    }


    public void AddHeart()
    {
        heartCount++;
    }
    public void SetProtectLifeColor()
    {
        int heartIndex = 0;
        foreach (var heart in hearts)
        {
            if (heartIndex >= lives)
            {
                heart.GetComponent<Image>().color = Color.black;
            }
            else
            {
                heart.GetComponent<Image>().color = Color.cyan;
            }
            heartIndex++;
        }
    }
    public void SetLives()
    {
        for (int i = heartCount -1 ; i >= 0; i--)
        {
            var heart = Instantiate(heartPrefab, lifeCounterContainer);
            hearts.Add(heart);
        }
    }
    public void LoseLive(ref bool protectedLife, bool deathDefy) 
    {
        if (protectedLife)
            protectedLife = false;
        else if (!deathDefy)
            lives--;
        UpdateHearts();
        if (lives <= 0) {
            GameManager.Instance.Lose();
        }
    }

    private void UpdateHearts() {
        int heartIndex = 0;
        foreach (var heart in hearts) {
            if (heartIndex >= lives) {
                heart.GetComponent<Image>().color = Color.black;
            }
            else {
                heart.GetComponent<Image>().color = GetHeartColor(heartIndex);
            }
            heartIndex++;
        }
    }

    public void ResetLives() {
        lives = heartCount;
        UpdateHearts();
    }

    public void GainLive() {
        if (lives < hearts.Count) {
            lives++;
            UpdateHearts();
        }

    }

    private Color GetHeartColor(int heartIndex)
    {
        float lerp = 1 / (float)heartCount;
        lerp *= heartIndex;
        float halfWay = lerp / 2; 
        Color c = Color.white;
        if (lerp <= halfWay)
             c = Color.Lerp(Color.red, Color.yellow, lerp);
        else
            c = Color.Lerp(Color.yellow, Color.green, lerp);
        return c;
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