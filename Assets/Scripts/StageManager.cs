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

    public SelectStageCanvas selectStageCanvas;
    
    public int selectedStage = 0;
    public int selectedDifficulty = 3;
    public (string name, Color color) selectedStickerGroup = ("",Color.white);
    public List<string> forbiddenGroups = new();
    public GameObject stageHolder => selectStageCanvas.stageHolder;
    public GameObject stageDisplayPrefab;

    public bool unlockAllStages;
    public int maxLevel = 13;

    //public GameObject stickerHolder;
    //public GameObject stickerPanel;
    public UserData userData => PersistanceManager.Instance.userData;
    //public Dictionary<int, StageData> stages => PersistanceManager.Instance.stages;
    public StickerSet gameVersion;
    public string difficultyDisplayLabel;
    public int difficultyDisplayOffset;
    
    public Dictionary<(int level, int difficulty),Stage> stages = new ();
    public void InitializeStages() {
        CustomDebugger.Log("called initialize stages");

        //Si el stage holder tiene stages, osea que esta vacio, topmoststage es null, si no topmoststage es el child 0 de stageholder transform
        Stage topmostStage = (stageHolder.transform.childCount == 0)? null :stageHolder.transform.GetChild(0)?.gameObject.GetComponent<Stage>();
        if (topmostStage is not null) topmostStage.shining = false;
        //stages = LoadStages();
        // Parseo en cada Stage el color
    
        // Recorro las dificultades, arrancando por 3, hasta 9, creando los stages para cada una, y cortando cuando la dificultad este bloqueada
        // que es cuando la dificultad anterior no tenga por lo menos 1 achievement
        bool nextStageUnlocked = true;
        int absoluteStageNumber = 0;
        //for (int level = 4; level < maxLevel; level++){
        for (int difficulty = 2; difficulty < 10; difficulty++) {            
            if (!nextStageUnlocked) break;
            //for (int difficulty = 2; difficulty < 10; difficulty++) { 
            for (int level = 4; level < maxLevel; level++){
                if (!nextStageUnlocked) break;
               // if (stageIndexAndData.Key != 0) break;
                //Evita crear stages que ya estan creados. Si el que esta arriba, que es el de (dificultad,stageid) mas alto 
                
                absoluteStageNumber++;
                UserStageData currentUserStageData = userData.GetUserStageData(level, difficulty);
                nextStageUnlocked = unlockAllStages || (currentUserStageData is not null && currentUserStageData.achievements.Count > 0);
                if (topmostStage is not null &&
                     (topmostStage.difficulty > difficulty || 
                      (topmostStage.difficulty == difficulty && topmostStage.level >= level))) continue;
                /*(topmostStage.level > level 
                 || 
                     (topmostStage.level == level && topmostStage.difficulty >= difficulty))) continue;*/
                
                GameObject stageDisplay = Instantiate(stageDisplayPrefab, stageHolder.transform);
                stageDisplay.transform.SetSiblingIndex(0);
                Stage newStage = stageDisplay.GetComponent<Stage>();
                //stageData.stageObject = newStage;

                //newStage.SetTitle(level+" "+difficultyDisplayLabel+ (difficulty - difficultyDisplayOffset));
                newStage.SetTitle(absoluteStageNumber.ToString());
                //newStage.SetColor(stageData.ColorValue);
                newStage.SetStage(level, difficulty);
                stages.Add((level, difficulty),newStage);
                
                if (userData.GetUserStageData(level, difficulty) is not null)
                {
                    newStage.SetScore( userData.GetUserStageData(level, difficulty).highScore);
                    
             
                    //TODO: mover esto a otro laod que tenga mas sentido
                    if (ColorUtility.TryParseHtmlString("#00d561", out Color green)){}
                    if (ColorUtility.TryParseHtmlString("#fedd30", out Color yellow)){}
                    if (ColorUtility.TryParseHtmlString("#d60149", out Color red)){}
                    
                    newStage.difficultyButton.gameObject.GetComponent<Image>().color = 
                        MyExtensions.GetLerpColor(difficulty-2,9-2,new List<Color>()
                        //MyExtensions.GetLerpColor(level,maxLevel,new List<Color>()
                            {green, yellow, red});
                    newStage.difficultyButton.stars
                        .SetAchievements(userData.GetUserStageData(level, difficulty).achievements);
                }
                else
                {
                    newStage.SetScore(0);
                }
            }
        }
        topmostStage = (stageHolder.transform.childCount == 0)? null :stageHolder.transform.GetChild(0)?.gameObject.GetComponent<Stage>();
        if (topmostStage is not null) topmostStage.shining = true;
        
        //scrollRect.normalizedPosition = new Vector2(0, 1);
    }
    public void SetStageAndDifficulty(int stage, int difficulty)
    {
        selectedDifficulty = difficulty;
        selectedStage = stage;
    }

    private (string randomGroupSelectedName, Color randomGroupSelectedColor) SelectRandomGroup() {
        if (!StickerManager.Instance.currentLoadedStickerGroups.ContainsKey((gameVersion,userData.language)))
        {
            StartCoroutine(StickerManager.Instance.LoadAllStickersFromSet(gameVersion));
        }
        
        int randomGroupSelectedIndex = Random.Range(0,
            StickerManager.Instance.currentLoadedStickerGroups[(gameVersion,userData.language)].Keys.Count-forbiddenGroups.Count);
        string randomGroupSelectedName = "";
        Color randomGroupSelectedColor = Color.white;
        
        int currentGroup = 0;
        foreach (var VARIABLE in StickerManager.Instance.currentLoadedStickerGroups[(gameVersion,userData.language)]) {
            if (forbiddenGroups.Contains(VARIABLE.Key)) continue;
            
            if (currentGroup == randomGroupSelectedIndex) {
                randomGroupSelectedName = VARIABLE.Key;
                randomGroupSelectedColor = VARIABLE.Value.color;
                CustomDebugger.Log("sticker color = "+VARIABLE.Value.color);
                break;
            }
            currentGroup++;
        }

        return (randomGroupSelectedName,randomGroupSelectedColor);
    }

    public IEnumerator SelectRandomGroupRoulette() {
        //open roulette panel
        AssignRandomStickerGroup();
        //close roulette panel
        //change canvas to game
        // gamemanager.reset
        yield return null;
    }

    public void AssignRandomStickerGroup() {
        selectedStickerGroup = SelectRandomGroup();
        forbiddenGroups.Add(selectedStickerGroup.name);
        while (forbiddenGroups.Count > 2) {
            forbiddenGroups.RemoveAt(0);
        }
    }
    // public void OpenStickerPanel(int stage) {
    //     CloseStickerPanel();
    //     
    //     stickerHolder.transform.GetComponentsInChildren<Sticker>();
    //     foreach (var stickerID in stages[stage].stickers) {
    //         Sticker display = StickerManager.Instance.GetStickerHolder();
    //         StickerData stickerData = StickerManager.Instance.GetStickerDataFromSetByStickerID(stages[stage].stickerSet,stickerID);
    //         display.SetStickerData(stickerData);
    //         // esto era para cuando tenias que desbloquear los stages, los lockeados eran los stickers que no tenias, pero ahora no se loquean asi que el boton
    //         // trae todos los stickers siempre
    //         /*if (stickerData.amountOfDuplicates == 0) {
    //             display.ConfigureLocked();
    //         }
    //         else {*/
    //         //}
    //         display.ConfigureForPack();
    //         
    //         display.transform.SetParent(stickerHolder.transform);
    //         display.transform.localScale = Vector3.one;
    //     }
    //     stickerPanel.SetActive(true);
    // }

    // public void CloseStickerPanel() {
    //     stickerPanel.SetActive(false);
    //     foreach (var sticker in stickerHolder.transform.GetComponentsInChildren<Sticker>()) {
    //         StickerManager.Instance.RecycleSticker(sticker);
    //         sticker.transform.SetParent(StickerManager.Instance.gameObject.transform);
    //     }
    // }
}
