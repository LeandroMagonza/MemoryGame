using System.Collections.Generic;
using UnityEngine;

public class StickerMatchData
{
    public int? activeTurnApply = null;
    public List<int> affectedValues = new List<int>();
    public void AddBetterClueEffect(int activeTurnApply)
    {
        this.activeTurnApply = activeTurnApply;
        this.affectedValues = new List<int>();
        for (int i = 1; i < activeTurnApply; i++)
        {
            affectedValues.Add(i);
        }
    }
    public void AddCutEffect(int activeTurnApply, int dificulty)
    {
        this.affectedValues = new List<int>();
        int iterator = dificulty + 1;
        int buttons = 0;
        switch (dificulty)
        {
            case 0:
                buttons = 4;
                break;
            case 1:
                buttons = 7;
                break;
            case 2:
                buttons = 10;
                break;
        }
        while (iterator > 0)
        {
            int random = Random.Range(1, buttons);
            if (random == activeTurnApply) continue;
            affectedValues.Add(random);
            iterator--;
        }
    }
}