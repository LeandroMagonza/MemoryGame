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
        
        // newStickers es para marcar cuando sacas la primera copia de un sticker, asi la mostramos al jugador con brillo y un cartel que diga new
        List< (StickerSet stickerSet,int stickerID)> newStickers = new List<(StickerSet stickerSet, int stickerID)>();
        
        for (int stickerNumber = 0; stickerNumber < GameManager.Instance.packs.stickersPerPack; stickerNumber++) {
            CustomDebugger.Log("stickerNumber "+stickerNumber);
            //int rarityRandomizer = Random.Range(0, 101);
            
            //rarity para despues, normal, raro legendario en instance packs legendary y rare chance
            //sumar rare / legendary amount si correspoende
            
            int stickerStageRandomizer = Random.Range(1, 101);
            // esto da un numero entre 0 y 100, y los pack odds dicen un porcentaje de chance por level, o 
            // hasta numero del random corresponde a cada pack. Si dice stage0: 50 y stage1: 50, es un 50% de chance cada uno,
            // pero los numeros del 1 al 50 correspondern al stage0, y del 51 al 100 correspondel al stage1. Por eso esta el acumulator,
            // que pasa de probablidad a los rangos por pack
            int accumulator = 0;
            int stickerStage = 0;
            foreach (var VARIABLE in GameManager.Instance.stages[packStage].packOdds){
                stickerStage = VARIABLE.Key;
                accumulator += GameManager.Instance.stages[packStage].packOdds[stickerStage];
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
                if (!newStickers.Contains((
                        GameManager.Instance.stages[stickerStage].stickerSet,
                            selectedStickerImageID
                        ))) {
                    CustomDebugger.Log("Adding to new stickers"+(GameManager.Instance.stages[stickerStage].stickerSet,selectedStickerImageID));
                    newStickers.Add((GameManager.Instance.stages[stickerStage].stickerSet,selectedStickerImageID));
                }
            }

        }
        foreach (var sticker in gainedStickers) {
            Sticker obtainedStickerHolder = StickerManager.Instance.GetStickerHolder();
            obtainedStickerHolder.transform.SetParent(stickerHolder.transform);
            //Instantiate(stickerHolderPrefab, stickerHolder.transform);
            StickerData stickerData = StickerManager.Instance.GetStickerDataFromSetByStickerID(sticker.stickerSet,sticker.stickerID);
            CustomDebugger.Log(stickerData.color);
            obtainedStickerHolder.SetStickerData(stickerData);
            obtainedStickerHolder.ConfigureForPack();
            obtainedStickerHolder.transform.localScale = Vector3.one;
            if (newStickers.Contains((sticker.stickerSet,sticker.stickerID))) {
                CustomDebugger.Log("calling display as new on "+stickerData.name+" "+(sticker.stickerSet,sticker.stickerID));
                obtainedStickerHolder.DisplayNewMarker(true);
            }
        }
        GameManager.Instance.UpdateAchievementAndUnlockedLevels();
        GameManager.Instance.disableInput = false;
    }
}
