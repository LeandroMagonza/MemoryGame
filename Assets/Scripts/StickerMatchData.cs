using System.Collections.Generic;
using UnityEngine;

public class StickerMatchData
{
    //BetterClue
    public int? lastClueAppearenceNumber = null;
    //Cut?
    public List<int> cutNumbers = new List<int>();
    //Bloq
    public List<int> blockedNumbers = new List<int>();
    public void AddBetterClueEffect(int lastClueAppearenceNumber)
    {
        this.lastClueAppearenceNumber = lastClueAppearenceNumber;
    }
    public void AddCutEffect(int correctNumber, int difficulty)
    {
        int lastClue = 1;
        int iterator = 0;
        if (lastClueAppearenceNumber is not null)
        {
            lastClue = (int)lastClueAppearenceNumber;
        }
        int options = GameManager.Instance.DifficultyToAmountOfAppearences(difficulty) -
                      lastClue;
        cutNumbers = new List<int>();

        switch (options)
        {
            case int i when i < 2:
                iterator = 0;
                break;
            case int i when i < 4:
                iterator = 1;
                break;
            case int i when i < 7:
                iterator = 2;
                break;
            default:
                iterator = 3;
                break;
            
        }

        while (iterator >= 0)
        {
            int random = Random.Range(lastClue, GameManager.Instance.DifficultyToAmountOfAppearences(difficulty) +1 );
            Debug.Log($"Random: {random} Correct: {correctNumber}");
            if (random != correctNumber && !cutNumbers.Contains(random))
            {
                cutNumbers.Add(random);
                iterator--;
            }
            Debug.Log("iterator: "+iterator);
        }
    }

    public void AddBlockEffect(int blockedNumber)
    {
        blockedNumbers.Add(blockedNumber);
    }
}