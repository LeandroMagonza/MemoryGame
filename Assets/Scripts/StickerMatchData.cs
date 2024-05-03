using System.Collections.Generic;
using System.Linq;
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
    public int remainingCuts = 0;
    //Bloq
    public void AddBetterClueEffect(int lastClueAppearenceNumber)
    {
        this.lastClueAppearenceNumber = lastClueAppearenceNumber;
    }
    public void AddCutEffect(int correctNumber, int difficulty, int amountOfCuts)
    {
        CustomDebugger.Log("correctNumber "+correctNumber);
        CustomDebugger.Log("difficulty "+difficulty);
        CustomDebugger.Log("amountOfCuts "+amountOfCuts);
        
        if (amountOfCuts + remainingCuts < 1) return;
        remainingCuts += amountOfCuts;
        int lastClue = 0;
        if (lastClueAppearenceNumber != null)
        {
            lastClue = (int)lastClueAppearenceNumber;
        }

        int totalOptions = difficulty;
        cutNumbers = new List<int>();

        // Crear lista de opciones válidas
        List<int> validOptions = new List<int>();
        for (int num = 1; num <= totalOptions; num++)
        {
            if (num > lastClue && num != correctNumber)
            {
                validOptions.Add(num);
            }
        }
        CustomDebugger.Log("validOptions "+validOptions.Count);
        // Mezclar la lista de opciones válidas
        System.Random rng = new System.Random();
        validOptions = validOptions.OrderBy(a => rng.Next()).ToList();

        // Determinar cuántos botones bloquear
        int maxCuts = Mathf.Min(remainingCuts, validOptions.Count);
        CustomDebugger.Log("maxCuts "+maxCuts);

        // Añadir botones a bloquear desde las opciones mezcladas
        for (int i = 0; i < maxCuts; i++)
        {
            cutNumbers.Add(validOptions[i]);
        }
        CustomDebugger.Log("cutNumbers "+cutNumbers.Count + cutNumbers.ToString());
        remainingCuts--;
    }
}