using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class AchievementStars : MonoBehaviour
{
    public Sprite starOff;
    public Sprite starOn;
    public List<GameObject> stars;
    // Start is called before the first frame update
    public IEnumerator SetAchievement(Achievement achievementToSet, float delay)
    {
        yield return new WaitForSeconds(delay);
        stars[(int)achievementToSet].GetComponent<Image>().sprite = starOn;
    }
}
