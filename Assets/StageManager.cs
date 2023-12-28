using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class StageManager : MonoBehaviour
{
    #region Singleton
    
    private static StageManager _instance;
    
    public static StageManager Instance {
        get {
            if (_instance == null)
                _instance = FindObjectOfType<StageManager>();
            if (_instance == null)
                Debug.LogError("Singleton<" + typeof(StageManager) + "> instance has been not found.");
            return _instance;
        }
    }
    protected void Awake() {
        if (_instance == null) {
            _instance = this as StageManager;
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
    
    public GameObject stageHolder;
    public int selectedStage = 0;
    public int selectedDifficulty = 0;
    public GameObject stageDisplayPrefab;

    public GameObject stickerHolder;
    public GameObject stickerPanel;
    public UserData userData => PersistanceManager.Instance.userData;
    public Dictionary<int, StageData> stages => PersistanceManager.Instance.stages;
    public StickerSet gameVersion;

    public void InitializeStages()
    {
        foreach (var stageIndexAndData in stages)
        {
            //stages = LoadStages();
            foreach (var stage in stages.Values)
            {
                if (ColorUtility.TryParseHtmlString("#" + stage.color, out Color colorValue))
                {
                    stage.ColorValue = colorValue;
                }
            }

            //.ToArray().Select((data, index) => new { index, data })
            StageData stageData = stageIndexAndData.Value;

            GameObject stageDisplay = Instantiate(stageDisplayPrefab, stageHolder.transform);
            Stage newStage = stageDisplay.GetComponent<Stage>();
            stageData.stageObject = newStage;


            newStage.name = stageData.title;
            newStage.SetTitle(stageData.title);
            newStage.SetColor(stageData.ColorValue);
            newStage.SetStage(stageData.stageID,stageData.stickerSet);

            for (int difficulty = 0; difficulty < 3; difficulty++)
            {
                if (userData.GetUserStageData(stageData.stageID, difficulty) is not null)
                {
                    newStage.SetScore(difficulty, userData.GetUserStageData(stageData.stageID, difficulty).highScore);
                    StartCoroutine(newStage.difficultyButtons[difficulty]
                        .SetAchievements(userData.GetUserStageData(stageData.stageID, difficulty).achievements, 0f));
                }
                else
                {
                    newStage.SetScore(difficulty, 0);
                }
            }
        }

        GameManager.Instance.SetScoreTexts();
    }
    public void SetStageAndDifficulty(int stage, int difficulty, StickerSet stickerSetName)
    {
        selectedDifficulty = difficulty;
        selectedStage = stage;
    }


    public void OpenStickerPanel(int stage) {
        CloseStickerPanel();
        
        stickerHolder.transform.GetComponentsInChildren<Sticker>();
        foreach (var stickerID in stages[stage].stickers) {
            Sticker display = StickerManager.Instance.GetStickerHolder();
            StickerData stickerData = StickerManager.Instance.GetStickerDataFromSetByStickerID(stages[stage].stickerSet,stickerID);
            display.SetStickerData(stickerData);
            if (stickerData.amountOfDuplicates == 0) {
                display.ConfigureLocked();
            }
            else {
                display.ConfigureForPack();
            }
            display.transform.SetParent(stickerHolder.transform);
            display.transform.localScale = Vector3.one;
        }
        stickerPanel.SetActive(true);
    }

    public void CloseStickerPanel() {
        stickerPanel.SetActive(false);
        foreach (var sticker in stickerHolder.transform.GetComponentsInChildren<Sticker>()) {
            StickerManager.Instance.RecycleSticker(sticker);
            sticker.transform.SetParent(null);
        }
    }
}
