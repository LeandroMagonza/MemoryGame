using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PackManager : MonoBehaviour {
    public GameObject stickerHolder;
    public GameObject stickerPrefab;
    public TextMeshProUGUI coins;
    private static PackManager _instance;
    
    public static PackManager Instance {
        get {
            if (_instance == null)
                _instance = FindObjectOfType<PackManager>();
            if (_instance == null)
                Debug.LogError("Singleton<" + typeof(PackManager) + "> instance has been not found.");
            return _instance;
        }
    }
    protected void Awake() {
        if (_instance == null) {
            _instance = this as PackManager;
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

    private void Start()
    {
        coins.text = GameManager.Instance.userData.coins.ToString();
    }

    public IEnumerator OpenPack(int packStage = 0)
    {
        GameManager.Instance.disableInput = true;
        int packCost = -(100 * packStage + 100);

        bool canBuyPack = GameManager.Instance.userData.ModifyCoins(packCost);
        if (!canBuyPack)
        {
            CustomDebugger.Log("Dont Have enough coins for pack");
            GameManager.Instance.disableInput = false;
            yield break;
        }
        
        coins.text = GameManager.Instance.userData.coins.ToString();
        
        //destruyo todas las figuras que haya anteriores
        for (int i = stickerHolder.transform.childCount - 1; i >= 0; i--) {
            StickerManager.Instance.RecycleSticker(stickerHolder.transform.GetChild(i).GetComponent<Sticker>());
            stickerHolder.transform.GetChild(i).gameObject.transform.parent = null;
        }
        
        CustomDebugger.Log("stickers per pack "+GameManager.Instance.packs.stickersPerPack);

        List< (int stickerID,StickerSet stickerSet)> gainedStickers = new List<(int stickerID, StickerSet stickerSet)>();
        for (int stickerNumber = 0; stickerNumber < GameManager.Instance.packs.stickersPerPack; stickerNumber++) {
            CustomDebugger.Log("stickerNumber "+stickerNumber);
            //int rarityRandomizer = Random.Range(0, 101);
            
            //rarity para despues, normal, raro legendario en instance packs legendary y rare chance
            //sumar rare / legendary amount si correspoende
            
            int stickerStageRandomizer = Random.Range(0, 101);
            int accumulator = 0;
            int stickerStage = 0;
            foreach (var VARIABLE in GameManager.Instance.stages[packStage].packOdds){
                stickerStage = VARIABLE.Key;
                accumulator += GameManager.Instance.stages[packStage].packOdds[VARIABLE.Key];
                if (accumulator > stickerStageRandomizer) {
                    break;
                }
            }
            CustomDebugger.Log("sticker randomizer "+stickerStageRandomizer+" stage "+stickerStage);
            CustomDebugger.Log("set has "+GameManager.Instance.stages[stickerStage].stickers.Count+" stickerStickers");
            int selectedStickerIndex = Random.Range(0, GameManager.Instance.stages[stickerStage].stickers.Count);
            CustomDebugger.Log("selected sticker from index "+selectedStickerIndex);
            int selectedStickerImageID = GameManager.Instance.stages[stickerStage].stickers[selectedStickerIndex];
            CustomDebugger.Log("corresponds to imageID "+selectedStickerImageID);
            
            gainedStickers.Add((selectedStickerImageID, GameManager.Instance.stages[stickerStage].stickerSet));
            
            if (GameManager.Instance.userData.stickerDuplicates.ContainsKey((GameManager.Instance.stages[stickerStage].stickerSet, selectedStickerImageID))) {
                GameManager.Instance.userData.stickerDuplicates[(GameManager.Instance.stages[stickerStage].stickerSet, selectedStickerImageID)]++;
            }
            else {
                GameManager.Instance.userData.stickerDuplicates.Add((GameManager.Instance.stages[stickerStage].stickerSet, selectedStickerImageID),1);
            }

        }
        foreach (var sticker in gainedStickers) {
            Sticker newSticker = StickerManager.Instance.GetStickerHolder();
            newSticker.transform.SetParent(stickerHolder.transform);
            //Instantiate(stickerHolderPrefab, stickerHolder.transform);
            StickerData stickerData = StickerManager.Instance.GetStickerDataFromSetByStickerID(sticker.stickerSet,sticker.stickerID);
            CustomDebugger.Log(stickerData.color);
            newSticker.SetStickerData(stickerData);
            newSticker.ConfigureForPack();
            newSticker.transform.localScale = Vector3.one;
        }
        GameManager.Instance.UpdateAchievementAndUnlockedLevels();
        GameManager.Instance.disableInput = false;
    }
}