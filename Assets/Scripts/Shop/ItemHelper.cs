using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public static class ItemHelper
{
    // La constante para dividir los minutos en intervalos
    private const int MinutesPerHour = 60;
    public static  List<(ConsumableID consumable, UpgradeID upgrade)>  upgradeRelation = new () {
        (ConsumableID.Clue, UpgradeID.FactoryClue) ,
        (ConsumableID.Remove, UpgradeID.FactoryRemove) ,
        (ConsumableID.Cut, UpgradeID.FactoryCut) ,
        //(ConsumableID.Peek, UpgradeID.FactoryPeek) ,
        (ConsumableID.Highlight, UpgradeID.FactoryHighlight) ,
        (ConsumableID.Bomb, UpgradeID.FactoryBomb) ,
    };

    public static Dictionary<IconName, Sprite> iconSprites = new Dictionary<IconName, Sprite>();
    public static string FormatGenerationTime(int generationMinutes, int divisor)
    {
        // Divide los minutos por la constante para obtener la fracción
        int dividedMinutes = generationMinutes / divisor;

        // Calcula horas y minutos restantes
        int hours = dividedMinutes / MinutesPerHour;
        int minutes = dividedMinutes % MinutesPerHour;

        // Genera un string en formato "1 hs", "1:30 hs", "15 min"
        string formattedTime;

        if (hours > 0)
        {
            if (minutes > 0)
            {
                formattedTime = $"{hours}:{minutes:D2} "+LocalizationManager.Instance.GetGameText(GameText.HoursAbb);
            }
            else
            {
                formattedTime = $"{hours} "+LocalizationManager.Instance.GetGameText(GameText.HoursAbb);
            }
        }
        else
        {
            formattedTime = $"{minutes} "+LocalizationManager.Instance.GetGameText(GameText.MinutesAbb);
        }

        return formattedTime;
    }

    public static UpgradeID GetCorrespondingUpgrade(ConsumableID consumableID) {
        foreach (var item in ItemHelper.upgradeRelation) {
            if (consumableID == item.consumable) {
                return item.upgrade;
            }
        }
        return UpgradeID.NONE;
    }    
    public static ConsumableID GetCorrespondingConsumable(UpgradeID upgradeID) {
        foreach (var item in ItemHelper.upgradeRelation) {
            if (upgradeID == item.upgrade) {
                return item.consumable;
            }
        }
        return ConsumableID.NONE;
    }

    public static Sprite GetIconSprite(IconName iconName) {
        if (iconSprites.ContainsKey(iconName)) {
            return iconSprites[iconName];
        }

        string path = "Icons/" + iconName;
        Sprite iconSprite = Resources.Load<Sprite>(path);
        if (iconSprite is null) {
            CustomDebugger.LogError("Icon: "+iconName+" not found in Icons folder path "+path);
        }
        else {
            iconSprites.Add(iconName,iconSprite);
        }
        return iconSprite;
    }
    public static string ReplacePlaceholders(string template, Dictionary<string, string> replacements, Dictionary<string, int> intReplacements = null)
        {
            foreach (var replacement in replacements)
            {
                string placeholder = "{" + replacement.Key + "}";
                template = template.Replace(placeholder, replacement.Value);
            }
    
            if (intReplacements == null)
            {
                return template;
            }
    
            foreach (var intReplacement in intReplacements)
            {
                string pattern = @"\{" + intReplacement.Key + @"\|([-+*]?\d+)\}";
                template = Regex.Replace(template, pattern, match =>
                {
                    string operation = match.Groups[1].Value;
                    if (operation.StartsWith("+") && int.TryParse(operation.Substring(1), out int addNumber))
                    {
                        return (intReplacement.Value + addNumber).ToString();
                    }
                    else if (operation.StartsWith("-") && int.TryParse(operation.Substring(1), out int subNumber))
                    {
                        return System.Math.Abs(intReplacement.Value - subNumber).ToString();
                    }
                    else if (operation.StartsWith("*") && int.TryParse(operation.Substring(1), out int mulNumber))
                    {
                        return (intReplacement.Value * mulNumber).ToString();
                    }
                    else if (int.TryParse(operation, out int number))
                    {
                        return (intReplacement.Value + number).ToString(); // Default to addition if no operator
                    }
                    return match.Value;
                });
            }
    
            return template;
        }
}

public enum IconName {
    NONE,
    CLUE,
    CUT,
    REMOVE,
    PEEK,
    HIGHLIGHT,
    BOMB,
    FACTORY,
    HEART,
    PLUS,
    SHIELD,
    SKULL,
    STICKER,
    BAG,
    MENU
}