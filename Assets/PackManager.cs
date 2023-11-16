using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PackManager : MonoBehaviour {
    public GameObject stickerHolder;
    public GameObject stickerPrefab;
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
    
    public void OpenPack(int packStage = 0) {
        
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
            Debug.Log("set has "+GameManager.Instance.stages[stickerStage].images.Count+" images");
            int selectedStickerIndex = Random.Range(0, GameManager.Instance.stages[stickerStage].images.Count);
            Debug.Log("selected sticker from index "+selectedStickerIndex);
            int selectedStickerImageID = GameManager.Instance.stages[stickerStage].images[selectedStickerIndex];
            Debug.Log("corresponds to imageID "+selectedStickerImageID);
            
            GameObject newSticker = Instantiate(stickerPrefab, stickerHolder.transform);
            
            GameObject stickerGameObject = newSticker.transform.GetComponentInChildren<Image>().gameObject;
            Image imageComponent = stickerGameObject.transform.Find("Image").GetComponent<Image>();
            imageComponent.sprite = GameManager.Instance.GetSpriteFromSetByImageID(selectedStickerImageID);
            
            if (GameManager.Instance.userData.imageDuplicates.ContainsKey(selectedStickerImageID)) {
                GameManager.Instance.userData.imageDuplicates[selectedStickerImageID]++;
            }
            else {
                GameManager.Instance.userData.imageDuplicates.Add(selectedStickerImageID,1);
            }
            
        }
    }
}
