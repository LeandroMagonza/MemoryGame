using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class StickerMatchData
{
    public int amountOfAppearences = 0;
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
        int lastClue = 0;
        int iterator = 0;
        if (lastClueAppearenceNumber is not null)
        {
            lastClue = (int)lastClueAppearenceNumber;
        }
        int options = difficulty - lastClue;
        cutNumbers = new List<int>();

        switch (options)
        {
            case int i when i <= 1:
                iterator = 0;
                break;
            case int i when i <= 3:
                iterator = 1;
                break;
            default:
                iterator = Mathf.CeilToInt((options)/2);
                break;
            
        }

        while (iterator >= 0)
        {
            int random = Random.Range(lastClue+1, difficulty + 1 );
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
        Debug.Log("Add Block: "+ blockedNumber);
        blockedNumbers.Add(blockedNumber);
        Debug.Log("Add: "+ blockedNumbers.Count);
    }
}