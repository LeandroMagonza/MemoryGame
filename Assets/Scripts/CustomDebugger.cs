using System.Collections.Generic;
using UnityEngine;

public static class CustomDebugger
{
    private static bool initialized = false;
    public static List<DebugCategory> disabledCategories = new List<DebugCategory>();

    public static void Log(object message, DebugCategory debugCategory = DebugCategory.GENERAL)
    {
        if (!initialized)
        {
            Initialize();
        }
        if (!disabledCategories.Contains(debugCategory))
        {
            Debug.Log(message);
        }
        
    }

    private static void Initialize()
    {
        //disabledCategories.Add(DebugCategory.GENERAL);
        //disabledCategories.Add(DebugCategory.LOAD);
        //disabledCategories.Add(DebugCategory.SAVE);
        initialized = true;
    }

    public static void LogError(object message)
    {
        Debug.LogError(message);
    }
}

public enum DebugCategory
{
    GENERAL,
    LOAD,
    SAVE,
    STICKERLOAD
}
