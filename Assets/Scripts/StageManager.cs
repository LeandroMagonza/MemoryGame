using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

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
    public string difficultyDisplayLabel;
    public int difficultyDisplayOffset;

    public void InitializeStages() {
        CustomDebugger.Log("called initialize stages");
        //Si el stage holder tiene stages, osea que esta vacio, topmoststage es null, si no topmoststage es el child 0 de stageholder transform
        Stage topmostStage = (stageHolder.transform.childCount == 0)? null :stageHolder.transform.GetChild(0)?.gameObject.GetComponent<Stage>();
        if (topmostStage is not null) topmostStage.shining = false;
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
            foreach (var stageIndexAndData in stages) {
                if (!nextStageUnlocked) break;
               // if (stageIndexAndData.Key != 0) break;
                StageData stageData = stageIndexAndData.Value;

                //Evita crear stages que ya estan creados. Si el que esta arriba, que es el de (dificultad,stageid) mas alto 
                
                
                UserStageData currentUserStageData = userData.GetUserStageData(stageData.stageID, difficulty);

                nextStageUnlocked = (currentUserStageData is not null && currentUserStageData.achievements.Count > 0);
                //nextStageUnlocked = true;
                if (topmostStage is not null &&
                    (topmostStage.difficulty > difficulty || 
                     (topmostStage.difficulty == difficulty && topmostStage.stage >= stageData.stageID))) continue;
                
                GameObject stageDisplay = Instantiate(stageDisplayPrefab, stageHolder.transform);
                stageDisplay.transform.SetSiblingIndex(0);
                Stage newStage = stageDisplay.GetComponent<Stage>();
                stageData.stageObject = newStage;

                newStage.name = stageData.title;
                newStage.SetTitle(stageData.title+" "+difficultyDisplayLabel+ (difficulty - difficultyDisplayOffset));
                newStage.SetColor(stageData.ColorValue);
                newStage.SetStage(stageData.stageID, difficulty);

                if (userData.GetUserStageData(stageData.stageID, difficulty) is not null)
                {
                    newStage.SetScore( userData.GetUserStageData(stageData.stageID, difficulty).highScore);
                    
             
                    
                    if (ColorUtility.TryParseHtmlString("#00d561", out Color green))
                         
                    if (ColorUtility.TryParseHtmlString("#fedd30", out Color yellow))
                         
                    if (ColorUtility.TryParseHtmlString("#d60149", out Color red))
                    
                    newStage.difficultyButton.gameObject.GetComponent<Image>().color = 
                        MyExtensions.GetLerpColor(difficulty-3,9-3,new List<Color>()
                            {green, yellow, red});
                    newStage.difficultyButton.stars
                        .SetAchievements(userData.GetUserStageData(stageData.stageID, difficulty).achievements);
                }
                else
                {
                    newStage.SetScore(0);
                }
            }
        }
        topmostStage = (stageHolder.transform.childCount == 0)? null :stageHolder.transform.GetChild(0)?.gameObject.GetComponent<Stage>();
        if (topmostStage is not null) topmostStage.shining = true;
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
            sticker.transform.SetParent(StickerManager.Instance.gameObject.transform);
        }
    }
}
