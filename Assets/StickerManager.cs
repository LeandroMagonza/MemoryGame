using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private Sticker stickerPrefab;
    private List<Sticker> _stickerPool;

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

    public void ReturnSticker(Sticker stickerToReturn)
    {
        _stickerPool.Add(stickerToReturn);
    }
    public StickerData GetStickerDataFromSetByStickerID(string imageSetName,int stickerID) {
        string[] splitedImageSetName = imageSetName.ToString().Split("_");
        
        string name = splitedImageSetName[0];
        string type = splitedImageSetName[1];
        int amount;
        int.TryParse(splitedImageSetName[2], out amount);
        
        string path;
        Sprite loadedSprite = null;
        switch (type) {
            case "SPRITESHEET":
                path = imageSetName + "/" + name;
                loadedSprite = Load(path, name + "_" + stickerID);
                break;
            case "IMAGES":
                path = imageSetName + "/(" + stickerID + ")";
                loadedSprite = Resources.Load<Sprite>(path);
                break;
            default:
                throw new Exception("INVALID IMAGESET TYPE");
        }

        Debug.Log("PATH: " + path);
        if (loadedSprite != null)
        {
            return new StickerData(
                stickerID,
                loadedSprite,
                PersistanceManager.Instance.userData.imageDuplicates[stickerID],
                GetStickerLevelByAmountOfDuplicates(PersistanceManager.Instance.userData.imageDuplicates[stickerID]),
                //sacar estos datos de csv pokemonlist, cambiar nombre y hacerlo generico para todos, algo como stickersadditionalinfo
                "name",
                Color.white,
                "type"
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
        int level = 0;
        foreach (var VARIABLE in PersistanceManager.Instance.stickerLevels)
        {
            if (amountOfDuplicates < VARIABLE.Value.amountRequired) break;
            level = VARIABLE.Key;
        }
        return level;
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

