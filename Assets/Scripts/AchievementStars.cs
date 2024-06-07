using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class AchievementStars : MonoBehaviour
{
    public Sprite starOff;
    public Sprite starOn;
    public List<GameObject> stars;
    // Start is called before the first frame update
    public void SetAchievement(Achievement achievementToSet, float delay)
    {
        if (stars.Count <= (int)achievementToSet) return;
        
        stars[(int)achievementToSet].GetComponent<Image>().sprite = starOn;
        if (delay <= 0) return;
        
        switch ((int)achievementToSet) {
            case 1:
                AudioManager.Instance.PlayClip(GameClip.getAchievementStar1);
                break;
            case 2:
                AudioManager.Instance.PlayClip(GameClip.getAchievementStar2);
                break;
            default: 
                AudioManager.Instance.PlayClip(GameClip.getAchievementStar0);
                break;
        }
        
        
    }
    public IEnumerator SetAchievements(List<Achievement> achievements,float delay)
    {
        foreach (var achievementToSet in achievements)
        {
            yield return new WaitForSeconds(delay);
            SetAchievement(achievementToSet, delay);
            
        }

        yield return new WaitForSeconds(.5f);
        if (stars[2].GetComponent<Image>().sprite == starOn) {
            AudioManager.Instance.PlayClip(GameClip.highScore);
        }
    }    
    public void SetAchievements(List<Achievement> achievements)
    {
        foreach (var achievementToSet in achievements)
        {
            SetAchievement(achievementToSet,0);
            
        }
    }

    public void ResetStars() {
        foreach (var starToReset in stars) {
            starToReset.GetComponent<Image>().sprite = starOff;
        }
    }
}
