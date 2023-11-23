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
            Debug.Log("Dont Have enough coins for pack");
            GameManager.Instance.disableInput = false;
            yield break;
        }
        
        coins.text = GameManager.Instance.userData.coins.ToString();
        
        //destruyo todas las figuras que haya anteriores
        for (int i = stickerHolder.transform.childCount - 1; i >= 0; i--) {
            Destroy(stickerHolder.transform.GetChild(i).gameObject);
        }
        
        Debug.Log("stickers per pack "+GameManager.Instance.packs.stickersPerPack);
        for (int stickerNumber = 0; stickerNumber < GameManager.Instance.packs.stickersPerPack; stickerNumber++) {
            Debug.Log("stickerNumber "+stickerNumber);
            //int rarityRandomizer = Random.Range(0, 101);
            
            //rarity para despues, normal, raro legendario en instance packs legendary y rare chance
            //sumar rare / legendary amount si correspoende
            
            int stickerStageRandomizer = Random.Range(0, 101);
            int acumulator = 0;
            int stickerStage = 0;
            foreach (var VARIABLE in GameManager.Instance.stages[packStage].packOdds){
                stickerStage = VARIABLE.Key;
                acumulator += GameManager.Instance.stages[packStage].packOdds[VARIABLE.Key];
                if (acumulator > stickerStageRandomizer) {
                    break;
                }
            }
            Debug.Log("sticker randomizer "+stickerStageRandomizer+" stage "+stickerStage);
            Debug.Log("set has "+GameManager.Instance.stages[stickerStage].stickers.Count+" stickerstickers");
            int selectedStickerIndex = Random.Range(0, GameManager.Instance.stages[stickerStage].stickers.Count);
            Debug.Log("selected sticker from index "+selectedStickerIndex);
            int selectedStickerImageID = GameManager.Instance.stages[stickerStage].stickers[selectedStickerIndex];
            Debug.Log("corresponds to imageID "+selectedStickerImageID);
            
            GameObject newSticker = Instantiate(stickerPrefab, stickerHolder.transform);
            
            GameObject stickerGameObject = newSticker.transform.GetComponentInChildren<Image>().gameObject;
            Image imageComponent = stickerGameObject.transform.Find("Image").GetComponent<Image>();
            imageComponent.sprite = StickerManager.Instance.GetStickerDataFromSetByStickerID(GameManager.Instance.imageSetName.ToString(),selectedStickerImageID).sprite;
            
            if (GameManager.Instance.userData.imageDuplicates.ContainsKey(selectedStickerImageID)) {
                GameManager.Instance.userData.imageDuplicates[selectedStickerImageID]++;
            }
            else {
                GameManager.Instance.userData.imageDuplicates.Add(selectedStickerImageID,1);
            }
            
        }
        GameManager.Instance.UpdateAchievementAndUnlockedLevels();
        GameManager.Instance.disableInput = false;
    }
}
