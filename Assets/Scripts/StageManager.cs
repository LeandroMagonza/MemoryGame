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
    public int selectedDifficulty = 3;
    public GameObject stageDisplayPrefab;

    public GameObject stickerHolder;
    public GameObject stickerPanel;
    public UserData userData => PersistanceManager.Instance.userData;
    public Dictionary<int, StageData> stages => PersistanceManager.Instance.stages;
    public StickerSet gameVersion;

    public void InitializeStages()
    {
        //stages = LoadStages();
        // Parseo en cada Stage el color
        foreach (var stage in stages.Values)
        {
            if (ColorUtility.TryParseHtmlString("#" + stage.color, out Color colorValue))
            {
                stage.ColorValue = colorValue;
            }
        }
        // Recorro las dificultades, arrancando por 3, hasta 9, creando los stages para cada una, y cortando cuando la dificultad este bloqueada
        // que es cuando la dificultad anterior no tenga por lo menos 1 achievement
        bool nextStageUnlocked = true;
        for (int difficulty = 3; difficulty < 10; difficulty++)
        {
            if (!nextStageUnlocked) break;
            foreach (var stageIndexAndData in stages)
            {
                if (!nextStageUnlocked) break;
                StageData stageData = stageIndexAndData.Value;
                UserStageData currentUserStageData = userData.GetUserStageData(stageData.stageID, difficulty);

                nextStageUnlocked = (currentUserStageData is not null && currentUserStageData.achievements.Count > 0);

                //.ToArray().Select((data, index) => new { index, data })
                

                GameObject stageDisplay = Instantiate(stageDisplayPrefab, stageHolder.transform);
                stageDisplay.transform.SetSiblingIndex(0);
                Stage newStage = stageDisplay.GetComponent<Stage>();
                stageData.stageObject = newStage;

                newStage.name = stageData.title;
                newStage.SetTitle(stageData.title);
                newStage.SetColor(stageData.ColorValue);
                newStage.SetStage(stageData.stageID, difficulty);

                if (userData.GetUserStageData(stageData.stageID, difficulty) is not null)
                {
                    newStage.SetScore( userData.GetUserStageData(stageData.stageID, difficulty).highScore);
                    newStage.difficultyButton.stars
                        .SetAchievements(userData.GetUserStageData(stageData.stageID, difficulty).achievements);
                }
                else
                {
                    newStage.SetScore(0);
                }
            }
        }


        GameManager.Instance.SetScoreTexts();
    }
    public void SetStageAndDifficulty(int stage, int difficulty)
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
            // esto era para cuando tenias que desbloquear los stages, los lockeados eran los stickers que no tenias, pero ahora no se loquean asi que el boton
            // trae todos los stickers siempre
            /*if (stickerData.amountOfDuplicates == 0) {
                display.ConfigureLocked();
            }
            else {*/
            //}
            display.ConfigureForPack();
            
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
