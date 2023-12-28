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
    private Dictionary<StickerSet,Dictionary<int, (string name, string type, Color color)>> stickersAdditionalData = new Dictionary<StickerSet, Dictionary<int, (string name, string type, Color color)>>();

    private StickerSet? currentLoadedSetName = null;
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
            LoadAllStickersFromSet(stickerSet);
        }

        return currentLoadedSetStickerData[stickerSet][stickerID];
        
    }
    public void LoadAllStickersFromSet(StickerSet setToLoad)
    {
        var dictionary = new Dictionary<int, StickerData>();
        currentLoadedSetStickerData.Add(setToLoad, dictionary);
        
        string imageSetName = setToLoad.ToString();
        string[] splitedImageSetName = imageSetName.Split("_");
        
        string name = splitedImageSetName[0];
        string type = splitedImageSetName[1];
        int totalStickersInSet;
        int.TryParse(splitedImageSetName[2], out totalStickersInSet);

        string path = "";
        Sprite loadedSprite = null;
        int loadingStickerID;
        
        switch (type) {
            case "SPRITESHEET":
                path = imageSetName + "/" + name;
                //loadedSprite = Load(path, name + "_" + stickerID);
                Sprite[] allSprites = Resources.LoadAll<Sprite>(path);
                if (allSprites == null) throw new Exception("Sprites not found"); 
                loadingStickerID = 0;
                /*foreach (var s in allSprites)
                {
                    currentLoadedSetStickerData.Add(loadingStickerID,AssembleStickerData(loadingStickerID, s));
                    loadingStickerID++;
                }*/
                Debug.Log(imageSetName+" has this many strpites "+allSprites.Length);
                for (loadingStickerID = 0; loadingStickerID < totalStickersInSet; loadingStickerID++)
                {
                    
                    dictionary.Add(loadingStickerID,AssembleStickerData(setToLoad, loadingStickerID, allSprites[loadingStickerID]));
            
                }
                
                break;
            case "IMAGES":
                for (loadingStickerID = 0; loadingStickerID < totalStickersInSet; loadingStickerID++)
                {
                    path = imageSetName + "/(" + loadingStickerID + ")";
                    loadedSprite = Resources.Load<Sprite>(path);
                    
                    if (loadedSprite == null) throw new Exception("ImageID not found in spritesFromSet"); 
                    
                    dictionary.Add(loadingStickerID,AssembleStickerData(setToLoad, loadingStickerID, loadedSprite));
            
                }
                break;
            default:
                throw new Exception("INVALID IMAGESET TYPE");
        }


        Debug.Log("PATH: " + path);
        
    }

    public StickerData AssembleStickerData(StickerSet stickerSet, int stickerID, Sprite sprite)
    {
        int amountOfDulpicates = 0;
        if (PersistanceManager.Instance.userData.imageDuplicates.ContainsKey(stickerID)) {
            amountOfDulpicates = PersistanceManager.Instance.userData.imageDuplicates[stickerID];
        }

        var additionalData = GetStickerAdditionalData(stickerSet, stickerID+1);
        return new StickerData(
            stickerID,
            sprite,
            amountOfDulpicates,
            GetStickerLevelByAmountOfDuplicates(amountOfDulpicates),
            //sacar estos datos de csv pokemonlist, cambiar nombre y hacerlo generico para todos, algo como stickersadditionalinfo
            additionalData.name,
            additionalData.color,
            additionalData.type
        );
    }
    

    public int GetStickerLevelByAmountOfDuplicates(int amountOfDuplicates)
    {
        Debug.Log("GetStickerLevelByAmountOfDuplicates");
        Debug.Log("amount of duplicates "+amountOfDuplicates);
        int level = 0;
        Debug.Log("stikcerlevels count "+PersistanceManager.Instance.StickerLevels.Count);
        foreach (var VARIABLE in PersistanceManager.Instance.StickerLevels)
        {
            
            Debug.Log("level "+VARIABLE.Key+" duplicates required "+VARIABLE.Value.amountRequired);
            if (amountOfDuplicates < VARIABLE.Value.amountRequired) break;
            level = VARIABLE.Key;
        }
        Debug.Log("resulitng level "+level);
        return level;
    }
    private void ReadAdditionalDataCSV(StickerSet stickerSet)
    {            
        if (!stickersAdditionalData.ContainsKey(stickerSet))
        {
            stickersAdditionalData.Add(stickerSet,new Dictionary<int, (string name, string type, Color color)>());
        }
        TextAsset csvData = Resources.Load<TextAsset>(stickerSet.ToString()+"/"+"additionalInfo");

        string[] lines = csvData.text.Split('\n');

        for (int i = 1; i < lines.Length; i++) // Empieza en 1 para omitir la cabecera
        {
            string[] fields = lines[i].Split(',');
            int id = int.Parse(fields[0]);
            string name = fields[1];
            string type = fields[2];
            Color color = Color.white;
            Debug.Log("color unparsed " + fields[3]);
            if (ColorUtility.TryParseHtmlString(fields[3], out Color colorValue))
            {
                color = colorValue;
            }


            stickersAdditionalData[stickerSet].Add(id, (name, type, color));
        }
    }
        
    public (string name, string type, Color color) GetStickerAdditionalData(StickerSet stickerSet, int id)
    {
        if (stickersAdditionalData.Count == 0) {
            ReadAdditionalDataCSV(stickerSet);
        }
        
        if (stickersAdditionalData[stickerSet].TryGetValue(id, out var data))
        {
            return data;
        }
        else
        {
            Debug.LogError("ID no encontrado: " + id);
            return ("null", null, new Color());
        }
    }
}


public class StickerData
{
    public int stickerID;
    public Sprite sprite;
    public int amountOfDuplicates;
    public int level;
    public string name;
    public Color color;
    public string type;
    
    //Just for game
    public int amountOfAppearences;
    public StickerData(int stickerID, Sprite sprite, int amountOfDuplicates, int level, string name, Color color, string type)
    {
        this.stickerID = stickerID;
        this.sprite = sprite;
        this.amountOfDuplicates = amountOfDuplicates;
        this.level = level;
        this.name = name;
        this.color = color;
        this.type = type;
    }
    

}

