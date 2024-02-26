using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class LifeCounter : MonoBehaviour
{
    public GameObject heartPrefab;
    public Transform lifeCounterContainer;
    public int currentLives;
    public int baseLives = 3;

    public List<GameObject> hearts;
    // Start is called before the first frame update
    public void Start()
    {
        SetLives();
        UpdateHearts();
        if (GameManager.Instance.userData.upgrades.ContainsKey(UpgradeID.LifeProtector) && GameManager.Instance.userData.upgrades[UpgradeID.LifeProtector] > 0)
        {
            ProtectHearts();
        }
    }

    public void SetProtectLifeColor()
    {
        int heartIndex = 0;
        foreach (var heart in hearts)
        {
            if (heartIndex >= currentLives)
            {
                ChangeHeartColor(heart, Color.black);
            }
            else
            {
                ChangeHeartColor(heart, Color.cyan);
            }
            heartIndex++;
        }
    }

    public void SetLives()
    {
        if (currentLives != hearts.Count)
        {
            if (hearts.Count > 0)
            {
                //List<GameObject> list = new List<GameObject>(hearts);
                foreach(GameObject gameObject in hearts.ToArray())
                {
                    Destroy(gameObject.gameObject);
                }
                hearts = new List<GameObject>();
                
            }
            for (int i = 0; i < currentLives ; i++)
            {
                var heart = Instantiate(heartPrefab, lifeCounterContainer);
                hearts.Add(heart);
            }
        }

        
    }
    public bool LoseLive(ref bool protectedLife, bool deathDefy) 
    {
        //Devuelve true si te quedaste sin vidas
        if (protectedLife)
        {
            protectedLife = false;
        }
        else if (!deathDefy)
            currentLives--;
        UpdateHearts();
        if (currentLives <= 0)
        {
            return true;
        }
        return false;
    }

    private void UpdateHearts() {
        int heartIndex = 0;
        foreach (var heart in hearts) {
            if (heartIndex >= currentLives) {
                ChangeHeartColor(heart, Color.black);
            }
            else {
                ChangeHeartColor(heart,  GetHeartColor(heartIndex));
            }
            heartIndex++;
        }
    }
    public void ProtectHearts()
    {
        int heartIndex = 0;
        foreach (var heart in hearts)
        {
            if (heartIndex >= currentLives)
            {
                ChangeHeartColor(heart, Color.black);
            }
            else
            {
                ChangeHeartColor(heart, Color.cyan);
            }
            heartIndex++;
        }
    }
    public void ResetLives() {
        int add = 0;
        if (GameManager.Instance.userData.upgrades.ContainsKey(UpgradeID.ExtraLife))
            add = GameManager.Instance.userData.upgrades[UpgradeID.ExtraLife];
        currentLives = baseLives + add;
        Start();
    }    
    public void ResetLivesPerfectMode() {
        currentLives = 1;
        Start();
    }

    public void GainLive() {
        if (currentLives < hearts.Count) {
            currentLives++;
            UpdateHearts();
        }

    }

    private Color GetHeartColor(int heartIndex)
    {
        int add = 0;
        if (GameManager.Instance.userData.upgrades.ContainsKey(UpgradeID.ExtraLife))
            add = GameManager.Instance.userData.upgrades[UpgradeID.ExtraLife];
        
        return MyExtensions.GetLerpColor(heartIndex,baseLives+add-1,new List<Color>(){Color.red,Color.yellow,Color.green});
    }  


    private void ChangeHeartColor(GameObject heart, Color color)
    {
        heart.transform.Find("HeartFill").GetComponent<Image>().color = color;
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
    
    public static Color GetLerpColor(int index,int max,List<Color> colors)
    {
        if (colors.Count<2) {
            throw new Exception("Can't lerp between less than 2 colors");
        }
        if (index == 0) {
            return colors[0];
        }
        if (index == max) {
            return colors[colors.Count-1];
        }
        
        float selectedStartingColorIndex = (float)index/max*(colors.Count - 1);
        // float bracketSize = (float)(max + 1) / (colors.Count - 1);
        // for (int colorIndex = 0; colorIndex < colors.Count-1; colorIndex++) {
        //     if (colorIndex*bracketSize > index) {
        //         break;
        //     }
        //
        //     selectedStartingColorIndex = colorIndex;
        // }
        
        //float partialFill = (index - selectedStartingColorIndex * bracketSize) / bracketSize;
        CustomDebugger.Log(index+" partial fill "+selectedStartingColorIndex%1);
        return Color.Lerp(colors[Mathf.FloorToInt(selectedStartingColorIndex)], colors[Mathf.FloorToInt(selectedStartingColorIndex)+1], selectedStartingColorIndex%1);
    }


}