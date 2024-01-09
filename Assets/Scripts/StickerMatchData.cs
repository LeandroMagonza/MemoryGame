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
        int options = GameManager.Instance.DifficultyToAmountOfAppearences(difficulty) -
                      (int)lastClueAppearenceNumber;
        cutNumbers = new List<int>();
        int iterator = 0;

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
        while (iterator > 0)
        {
            int random = Random.Range((int)lastClueAppearenceNumber+1, GameManager.Instance.DifficultyToAmountOfAppearences(difficulty)+1);
            if (random == correctNumber) continue;
            cutNumbers.Add(random);
            iterator--;
        }
    }

    public void AddBlockEffect(int blockedNumber)
    {
        blockedNumbers.Add(blockedNumber);
    }
}