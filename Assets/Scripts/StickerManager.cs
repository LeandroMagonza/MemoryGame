using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using ColorUtility = UnityEngine.ColorUtility;
public class StickerManager : MonoBehaviour
{
    #region Singleton
    
    private static StickerManager _instance;
    
    public static StickerManager Instance {
        get {
            if (_instance == null)
                _instance = FindObjectOfType<StickerManager>();
            if (_instance == null)
                Debug.LogError("Singleton<" + typeof(StickerManager) + "> instance has been not found.");
            return _instance;
        }
    }
    protected void Awake() {
        if (_instance == null) {
            _instance = this as StickerManager;
        }
        else if (_instance != this)
            DestroySelf();
    }
    private void DestroySelf() {
        if (Application.isPlaying)
            Destroy(this);
        else
            DestroyImmediate(this);
    }
    #endregion

    [FormerlySerializedAs("stickerPrefab")] public Sticker stickerHolderPrefab;
    private List<Sticker> _stickerPool = new List<Sticker>();
    // esto se tiene que ir porque ya esta todo en sticker data, y se va a cargar directo desde el csv

    public Dictionary<StickerSet,Dictionary<int,StickerData>> currentLoadedSetStickerData = new Dictionary<StickerSet, Dictionary<int, StickerData>>();

    // Update is called once per frame
    public Sticker GetStickerHolder()
    {
        if (_stickerPool.Count == 0)
        {
            //Create stickerHolderPrefab
            return Instantiate(stickerHolderPrefab, transform);
        }

        Sticker stickerToReturn = _stickerPool[0];
        _stickerPool.Remove(stickerToReturn);
        
        return stickerToReturn;
    }

    public void RecycleSticker(Sticker stickerToReturn)
    {
        _stickerPool.Add(stickerToReturn);
    }
    public StickerData GetStickerDataFromSetByStickerID(StickerSet stickerSet,int stickerID)
    {
        
        if (!currentLoadedSetStickerData.ContainsKey(stickerSet))
        {
            //TODO: fix Problema ACA
            // no me acuerdo que habia que arreglar jaja
            LoadAllStickersFromSet(stickerSet);
        }
        if (!currentLoadedSetStickerData.ContainsKey(stickerSet) )
        {
            throw new Exception("Failed to load sticker set " + stickerSet + " currentLoadedSetStickerData doesnt contain the key");
        } if (currentLoadedSetStickerData[stickerSet] is null)
        {
            throw new Exception("Failed to load sticker set " + stickerSet+" element stickerSet in currentLoadedSetStickerData is null");
        } if (currentLoadedSetStickerData[stickerSet].Count == 0)
        {
            throw new Exception("Failed to load sticker set " + stickerSet+ "currentLoadedSetStickerData[stickerSet] has no stickers ");
        }

        if (!currentLoadedSetStickerData[stickerSet].ContainsKey(stickerID))
        {
            CustomDebugger.Log(stickerID + " not found in sticker set " + stickerSet,DebugCategory.STICKERLOAD);
            string stickersetcontents = "";
            foreach (var IDAndStickerData in currentLoadedSetStickerData[stickerSet])
            {
                stickersetcontents += "(" + IDAndStickerData.Key + ", " + IDAndStickerData.Value.name + ");\n";
            }
            CustomDebugger.Log(stickersetcontents,DebugCategory.STICKERLOAD);
        }
        return currentLoadedSetStickerData[stickerSet][stickerID];
        
    }
    public int GetStickerLevelByAmountOfDuplicates(int amountOfDuplicates)
    {
        CustomDebugger.Log("GetStickerLevelByAmountOfDuplicates");
        CustomDebugger.Log("amount of duplicates "+amountOfDuplicates);
        int level = 0;
        CustomDebugger.Log("stikcerlevels count "+PersistanceManager.Instance.StickerLevels.Count);
        foreach (var VARIABLE in PersistanceManager.Instance.StickerLevels)
        {
            
            CustomDebugger.Log("level "+VARIABLE.Key+" duplicates required "+VARIABLE.Value.amountRequired);
            if (amountOfDuplicates < VARIABLE.Value.amountRequired) break;
            level = VARIABLE.Key;
        }
        CustomDebugger.Log("resulitng level "+level);
        return level;
    }

    public void LoadAllStickersFromSet(StickerSet setToLoad) {
        string setType = "IMAGES";
        if (currentLoadedSetStickerData.ContainsKey(setToLoad))
        {
            Debug.LogError("Stickers for " + setToLoad + " already loaded.");
            return;
        }

        TextAsset csvData = Resources.Load<TextAsset>(setToLoad.ToString() + "/" + "additionalInfo");
        if (csvData == null)
        {
            Debug.LogError("CSV data for stickers not found.");
            return;
        }

        string[] lines = csvData.text.Split('\n');
        Sprite[] spritesheet = Array.Empty<Sprite>();
        
        if (setType == "SPRITESHEET")
        {
            spritesheet= Resources.LoadAll<Sprite>(setToLoad.ToString() + "/" + name);
        }
        
        var dictionary = new Dictionary<int, StickerData>();

        for (int i = 1; i < lines.Length; i++)
        {
            string[] fields = lines[i].Split(',');
            if (fields.Length < 4) continue;

            int id = int.Parse(fields[0]);
            string name = fields[1];
            string type = fields[2];
            ColorUtility.TryParseHtmlString(fields[3], out Color color);

            Sprite sprite = null;
            if (setType == "SPRITESHEET")
            {
                sprite = spritesheet[id];
            }
            else if (setType == "IMAGES")
            {
                string imagePath = setToLoad + "/"  + id;
                sprite = Resources.Load<Sprite>(imagePath);
                if (sprite == null)
                {
                    imagePath = setToLoad + "/" + "(" + id + ")";
                    sprite = Resources.Load<Sprite>(imagePath);
                }
            }

            if (sprite == null)
            {
                Debug.LogError("Sprite not found for ID: " + id);
                continue;
            }

            StickerData stickerData = new StickerData(id, sprite, setToLoad, name, color, type);
            dictionary.Add(id, stickerData);
        }

        currentLoadedSetStickerData.Add(setToLoad, dictionary);
        Debug.Log("Loaded stickers from set " + setToLoad);
    }

}


public class StickerData
{
    public int stickerID;
    public Sprite sprite;
    public int amountOfDuplicates => PersistanceManager.Instance.GetStickerDuplicates(stickerSet,stickerID);
    public int level => StickerManager.Instance.GetStickerLevelByAmountOfDuplicates(amountOfDuplicates);
    public string name;
    public Color color;
    public string type;
    public StickerSet stickerSet;

    public StickerData(int stickerID, Sprite sprite, StickerSet stickerSet,  string name, Color color, string type)
    {
        this.stickerID = stickerID;
        this.sprite = sprite;
        this.name = name;
        this.color = color;
        this.type = type;
        this.stickerSet = stickerSet;

    }

}

