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
            if (heartIndex >= lives)
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
        int add = 0;
        if (GameManager.Instance.userData.upgrades.ContainsKey(UpgradeID.ExtraLife))
            add = GameManager.Instance.userData.upgrades[UpgradeID.ExtraLife];
        int index = heartCount - 1 + add;
        if (index + 1 != hearts.Count)
        {
            if (hearts.Count > 0)
            {
                List<GameObject> list = new List<GameObject>(hearts);
                hearts = new List<GameObject>();
                foreach(GameObject gameObject in list)
                {
                    Destroy(gameObject.gameObject);
                }
            }
            for (int i = index; i >= 0; i--)
            {
                var heart = Instantiate(heartPrefab, lifeCounterContainer);
                hearts.Add(heart);
            }
        }

        
    }
    public void LoseLive(ref bool protectedLife, bool deathDefy) 
    {
        //TODO: Chequear tema de que corazones aparecen, etc
        if (protectedLife)
        {
            protectedLife = false;
        }
        else if (!deathDefy)
            lives--;
        UpdateHearts();
        if (lives <= 0) {
            StartCoroutine(GameManager.Instance.EndGame(false));
        }
    }

    private void UpdateHearts() {
        int heartIndex = 0;
        foreach (var heart in hearts) {
            if (heartIndex >= lives) {
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
            if (heartIndex >= lives)
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
        lives = heartCount + add;
        Start();
    }

    public void GainLive() {
        if (lives < hearts.Count) {
            lives++;
            UpdateHearts();
        }

    }

    private Color GetHeartColor(int heartIndex)
    {
        int add = 0;
        if (GameManager.Instance.userData.upgrades.ContainsKey(UpgradeID.ExtraLife))
            add = GameManager.Instance.userData.upgrades[UpgradeID.ExtraLife];
        float lerp = 1 / (float)(heartCount + add);
        lerp *= heartIndex;
        float halfWay = lerp / 2; 
        Color c = Color.white;
        if (lerp <= halfWay)
             c = Color.Lerp(Color.red, Color.yellow, lerp);
        else
            c = Color.Lerp(Color.yellow, Color.green, lerp);
        return c;
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
}