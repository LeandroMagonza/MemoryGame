using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
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

    public Sticker stickerPrefab;
    private List<Sticker> _stickerPool = new List<Sticker>();
    private Dictionary<int, (string name, string type, Color color)> stickersAdditionalData = new Dictionary<int, (string, string, Color)>();

    private ImageSet? currentLoadedSetName = null;
    private Dictionary<int,StickerData> currentLoadedSetStickerData = new Dictionary<int, StickerData>();

    // Update is called once per frame
    public Sticker GetSticker()
    {
        if (_stickerPool.Count == 0)
        {
            //Create stickerPrefab
            return Instantiate(stickerPrefab, transform);
        }

        Sticker stickerToReturn = _stickerPool[0];
        _stickerPool.Remove(stickerToReturn);
        
        return stickerToReturn;
    }

    public void RecycleSticker(Sticker stickerToReturn)
    {
        _stickerPool.Add(stickerToReturn);
    }
    public StickerData GetStickerDataFromSetByStickerID(ImageSet imageSet,int stickerID)
    {
        if (currentLoadedSetName != imageSet)
        {
            currentLoadedSetName = imageSet;
            LoadAllStickersFromSet(imageSet);
        }

        return currentLoadedSetStickerData[stickerID];
        
    }
    public void LoadAllStickersFromSet(ImageSet setToLoad)
    {
            
        string imageSetName = setToLoad.ToString();
        string[] splitedImageSetName = imageSetName.Split("_");
        
        string name = splitedImageSetName[0];
        string type = splitedImageSetName[1];
        int totalStickersInSet;
        int.TryParse(splitedImageSetName[2], out totalStickersInSet);

        //TODO: Segurir modificacndo esta funcion para que lea todos los stickers del set de una y los guarde al principio, en vez de guardarkis cuando se cambia el stage
        for (int loadingStickerID = 0; loadingStickerID < totalStickersInSet; loadingStickerID++)
        {
            
        }

        string path;
        Sprite loadedSprite = null;
        switch (type) {
            case "SPRITESHEET":
                path = imageSetName + "/" + name;
                //loadedSprite = Load(path, name + "_" + stickerID);
                Sprite[] all = Resources.LoadAll<Sprite>(path);

                foreach (var s in all)
                {
        
                }
                
                break;
            case "IMAGES":
                path = imageSetName + "/(" + stickerID + ")";
                loadedSprite = Resources.Load<Sprite>(path);
                break;
            default:
                throw new Exception("INVALID IMAGESET TYPE");
        }


        Debug.Log("PATH: " + path);
        if (loadedSprite != null) {

            int amountOfDulpicates = 0;
            if (PersistanceManager.Instance.userData.imageDuplicates.ContainsKey(stickerID)) {
                amountOfDulpicates = PersistanceManager.Instance.userData.imageDuplicates[stickerID];
            }

            var additionalData = GetStickerAdditionalData(stickerID+1);
            return new StickerData(
                stickerID,
                loadedSprite,
                amountOfDulpicates,
                GetStickerLevelByAmountOfDuplicates(amountOfDulpicates),
                //sacar estos datos de csv pokemonlist, cambiar nombre y hacerlo generico para todos, algo como stickersadditionalinfo
                additionalData.name,
                additionalData.color,
                additionalData.type
            );
        }
        else {
            throw new Exception("ImageID not found in spritesFromSet");
        }
    }
    
    public Sprite Load(string resourcePath, string spriteName)
    {
        Sprite[] all = Resources.LoadAll<Sprite>(resourcePath);

        foreach (var s in all)
        {
            if (s.name == spriteName)
            {
                return s;
            }
        }
        return null;
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
    private void ReadAdditionalDataCSV()
    {
        TextAsset csvData = Resources.Load<TextAsset>(StageManager.Instance.stickerSetName+"/"+"additionalInfo");

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
            stickersAdditionalData.Add(id, (name, type, color));
        }
    }
        
    public (string name, string type, Color color) GetStickerAdditionalData(int id)
    {
        if (stickersAdditionalData.Count == 0) {
            ReadAdditionalDataCSV();
        }
        
        if (stickersAdditionalData.TryGetValue(id, out var data))
        {
            return data;
        }
        else
        {
            Debug.LogError("ID no encontrado: " + id);
            return (null, null, new Color());
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

