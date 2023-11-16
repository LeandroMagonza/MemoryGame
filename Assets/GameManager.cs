using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using ColorUtility = UnityEngine.ColorUtility;
using Random = UnityEngine.Random;
using Newtonsoft.Json;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

public class GameManager : MonoBehaviour {
    private static GameManager _instance;

    public bool gameEnded = false;
    public LifeCounter lifeCounter;
    public float timer = 3;
    public float maxTimer = 10;
    public float currentTimerGain = 5;
    public TextMeshProUGUI timerText; 
    public int score = 0;
    public TextMeshProUGUI scoreText; 
    public int turnNumber = 1;
    public int bonusOnAmountOfAppearences = 9;
    public int bonusMultiplicator = 1;

    public TextMeshProUGUI highScoreText; 
    
    public FadeOut amountOfAppearancesText; 
    public TextMeshProUGUI imageIDText; 

    public ImageSet imageSetName = ImageSet.Landscapes_IMAGES_10;
    public GameObject imageOnDisplay;

    private int _currentlySelectedImage;
    private Dictionary<int,(Sprite sprite, int amountOfAppearances)> _spritesFromStage = new Dictionary<int, (Sprite sprite, int amountOfAppearances)>();

    private List<int> currentlyInGameImages =new List<int>();

    private Dictionary<ShopItemType, int> inventary = new Dictionary<ShopItemType, int>();
    private Dictionary<ShopItemType, int> matchInventary = new Dictionary<ShopItemType, int>();
    private Dictionary<ShopItemType, int> upgrades = new Dictionary<ShopItemType, int>();

    public AudioSource audioSource;
    public AudioClip correctGuessClip;
    public AudioClip incorrectGuessClip;
    public AudioClip endGameClip;
    public AudioClip bonusClip;
    public AudioClip winClip;
    public AudioClip highScoreClip;

    public ParticleSystem correctGuessParticle;
    public ParticleSystem incorrectGuessParticle;

    public Button buttonReplay;
    public float delayBetweenImages = .72f;

    public bool disableInput = false;

    [Header("Clue And Remove Settings")]
    [SerializeField] private Button buttonClue;
    [SerializeField] private Button buttonRemove;
    [SerializeField] private TextMeshProUGUI buttonTextClue;
    [SerializeField] private TextMeshProUGUI buttonTextRemove;
    [SerializeField] private AudioClip buttonClueAudioClip;
    [SerializeField] private AudioClip buttonRemoveAudioClip;

    public int currentClues = 0;
    public int maxCluesAmount = 5;
    public int currentRemoves = 0;
    public int maxRemovesAmount = 5;
    public int selectedStage = 0;
    public int selectedDifficulty = 0;

    public Dictionary<int, StageData> stages;
    private Dictionary<int, StickerLevelsData> stickerLevels = new Dictionary<int, StickerLevelsData>();
    public PacksData packs = new PacksData();
    
    public UserData userData = new UserData();
    public GameObject selectStageAndDifficultyCanvas;
    public GameObject gameCanvas;

    public GameObject numpadRow0;
    public GameObject numpadRow1;
    public GameObject numpadRow2;

    public GameObject stageDisplayPrefab;

    private Match _currentMatch;
    private int _currentCombo = 0;
    public TextMeshProUGUI comboBonusText;
    public TextMeshProUGUI comboText;

    public static GameManager Instance {
        get {
            if (_instance == null)
                _instance = FindObjectOfType<GameManager>();
            if (_instance == null)
                Debug.LogError("Singleton<" + typeof(GameManager) + "> instance has been not found.");
            return _instance;
        }
    }
    protected void Awake() {
        if (_instance == null) {
            _instance = this as GameManager;
            audioSource = GetComponent<AudioSource>();
        }
        else if (_instance != this)
            DestroySelf();
    }

    private void Start()
    {
        buttonClue.interactable = currentClues > 0;
        buttonRemove.interactable = currentRemoves > 0;
        buttonTextClue.text = currentClues.ToString();
        buttonTextRemove.text = currentRemoves.ToString();
        StartCoroutine(LoadUserData());
        
    }

    private void InitializeStages()
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

            GameObject stageDisplay = Instantiate(stageDisplayPrefab, selectStageAndDifficultyCanvas.transform);
            Stage newStage = stageDisplay.GetComponent<Stage>();
            stageData.stageObject = newStage;


            newStage.name = stageData.title;
            newStage.SetTitle(stageData.title);
            newStage.SetColor(stageData.ColorValue);
            newStage.SetAmountOfImages(stageData.images.Count);
            newStage.SetStage(stageData.stageID);

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

        SetScoreTexts();
    }

    private void SetScoreTexts() {
        if (userData.GetUserStageData(selectedStage, selectedDifficulty) is not null) {
            scoreText.text = userData.GetUserStageData(selectedStage, selectedDifficulty).highScore.ToString();
            highScoreText.text = userData.GetUserStageData(selectedStage, selectedDifficulty).highScore.ToString();
        }
    }

    private void DestroySelf() {
        if (Application.isPlaying)
            Destroy(this);
        else
            DestroyImmediate(this);
    }

    public IEnumerator ProcessTurnAction(int number)
    {
        disableInput = true;
        Debug.Log("Guessed number " + number + ", amount of appearances " + _spritesFromStage[_currentlySelectedImage].amountOfAppearances);
        TurnAction turnAction;
        int scoreModification = 0;
        float timerModification = maxTimer - timer;
        if (number == _spritesFromStage[_currentlySelectedImage].amountOfAppearances)
        {
            Debug.Log("CorrectGuess");
            scoreModification = OnCorrectGuess();
            turnAction = TurnAction.GuessCorrect;
        }
        else
        {
            Debug.Log("IncorrectGuess");
            OnIncorrectGuess();
            turnAction = TurnAction.GuessIncorrect;
        }
        yield return new WaitForSeconds(delayBetweenImages);
        // si ya la imagen aparecio 9 veces, se la saca del pool
        _currentMatch.AddTurn(
            _currentlySelectedImage,
            _spritesFromStage[_currentlySelectedImage].amountOfAppearances,
            timerModification,
            number,
            turnAction,
            lifeCounter.lives,
            _currentCombo,
            scoreModification
            );
        CheckAmountOfAppearances();
        if (!gameEnded) NextTurn();
    }
    private void CheckAmountOfAppearances()
    {
        if (_spritesFromStage[_currentlySelectedImage].amountOfAppearances == bonusOnAmountOfAppearences)
        {
            Debug.Log("Clear: " + _spritesFromStage[_currentlySelectedImage].sprite.name);
            GainBonus();
            RemoveStickerFromPool();
        }
    }
    private void NextTurn() {
        turnNumber++;
        if (currentlyInGameImages.Count < 3)
        {
            AddImages(1);
        }
        else if (turnNumber % 10 == 0)
        {
            AddImages(turnNumber / 10);
        }
        SetRandomImage();
        disableInput = false;

    }

    private void AddClue()
    {
        currentClues++;
        buttonClue.interactable = currentClues > 0;
        currentClues = currentClues > maxCluesAmount ? maxCluesAmount : currentClues;
        buttonTextClue.text = currentClues.ToString();
    }

    private void AddRemove() 
    {
        currentRemoves++;
        buttonRemove.interactable = currentRemoves > 0;
        currentRemoves = currentRemoves > maxRemovesAmount ? maxRemovesAmount : currentRemoves;
        buttonTextRemove.text = currentRemoves.ToString();
    }

    [ContextMenu("UseClue")]
    public void UseClue() {
        Debug.Log("USE CLUE: " + currentClues);

        if (currentClues < 1) return;

        audioSource.PlayOneShot(buttonClueAudioClip);
        // anim
        currentClues--;
        currentClues = currentClues < 0 ? 0 : currentClues;
        buttonClue.interactable = currentClues != 0;
        buttonTextClue.text = currentClues.ToString();
        OnCorrectGuess();
        NextTurn();
    }
    [ContextMenu("UseRemove")]
    public void UseRemove()
    {
        Debug.Log("USE REMOVE");
        if (currentRemoves < 1) return;

        audioSource.PlayOneShot(buttonRemoveAudioClip);
        //anim
        currentRemoves--;
        currentRemoves = currentRemoves < 0 ? 0 : currentRemoves;
        buttonRemove.interactable = currentRemoves != 0;
        buttonTextRemove.text = currentRemoves.ToString();
        RemoveStickerFromPool();
        NextTurn();
    }
    private void OnIncorrectGuess()
    {
        IncorrectGuessFX();
        amountOfAppearancesText.SetAmountOfGuessesAndShowText(
            _spritesFromStage[_currentlySelectedImage].amountOfAppearances,
            false);
        audioSource.PlayOneShot(incorrectGuessClip);
        lifeCounter.LoseLive();
        SetTimer(maxTimer);
        _spritesFromStage[_currentlySelectedImage] = (
            _spritesFromStage[_currentlySelectedImage].sprite,
            _spritesFromStage[_currentlySelectedImage].amountOfAppearances - 1);
        SetCurrentCombo(0);
    }

    public void SetCurrentCombo(int newCurrentComboAmount)
    {
        _currentCombo = newCurrentComboAmount;
        comboBonusText.text = CalculateScoreComboBonus().ToString();
        comboText.text = _currentCombo.ToString(); 
        //TODO: add animation
    }
   

    private int OnCorrectGuess()
    {
        CorrectGuessFX();
        amountOfAppearancesText.SetAmountOfGuessesAndShowText(
            _spritesFromStage[_currentlySelectedImage].amountOfAppearances,
            true);
        audioSource.PlayOneShot(correctGuessClip);
        SetTimer(maxTimer);
        int scoreModification = stages[selectedStage].basePoints +
                                _spritesFromStage[_currentlySelectedImage].amountOfAppearances
                                + CalculateScoreComboBonus();
        ModifyScore(scoreModification);
        SetCurrentCombo(_currentCombo+1);
        return scoreModification;
    }

    private void CorrectGuessFX()
    {
        StartCoroutine(Squash(imageOnDisplay.transform, .2f, 0.1f, 5));
        correctGuessParticle.Play();
    }
    private void IncorrectGuessFX()
    {
        StartCoroutine(Shake(imageOnDisplay.transform, .2f, 5, 80));
        incorrectGuessParticle.Play();
    }

    [ContextMenu("GainBonus")]
    private void GainBonus()
    {
        Debug.Log("gain bonus");
        ModifyScore(_spritesFromStage[_currentlySelectedImage].amountOfAppearances * bonusMultiplicator);
        bonusMultiplicator++;
        audioSource.PlayOneShot(bonusClip);
        lifeCounter.GainLive();
    }

    public void ModifyScore(int modificationAmount) 
    {
        score += modificationAmount;
        scoreText.text = score.ToString();
        
    } 
    private void SetScore(int newScore) {
        score = newScore;
        scoreText.text = score.ToString();
    }

    private void LoadImages(string imageSetName) {
        _spritesFromStage = new Dictionary<int, (Sprite sprite, int amountOfAppearances)>();
        
        string[] splitedImageSetName = imageSetName.Split("_");
        
        string name = splitedImageSetName[0];
        string type = splitedImageSetName[1];
        int amount;
        int.TryParse(splitedImageSetName[2], out amount);

        foreach (var imageID in stages[selectedStage].images) {
            _spritesFromStage.Add(imageID, (GetSpriteFromSetByImageID(imageID), 0));
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
    
    private void SetRandomImage() {
        Image image = imageOnDisplay.GetComponent<Image>();
        
        if (currentlyInGameImages.Count == 0) {
            Win();
            return;
        }    
        int nextImageIndex = Random.Range(0, currentlyInGameImages.Count);
        int nextImageID = currentlyInGameImages[nextImageIndex];

        while (_currentlySelectedImage == nextImageID && currentlyInGameImages.Count > 1) {
            nextImageIndex = Random.Range(0, currentlyInGameImages.Count);
            nextImageID = currentlyInGameImages[nextImageIndex];
        }
        
        image.sprite = _spritesFromStage[nextImageID].sprite;
        
        _currentlySelectedImage = nextImageID;
        _spritesFromStage[_currentlySelectedImage]= (
            _spritesFromStage[_currentlySelectedImage].sprite,
            _spritesFromStage[_currentlySelectedImage].amountOfAppearances+1);
        imageIDText.text = (nextImageID+1).ToString();
        // image.sprite = _spritesFromSet[Random.Range(0, _spritesFromSet.Count)].sprite;
    }

    public Sprite GetSpriteFromSetByImageID(int imageID) {
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
                loadedSprite = Load(path, name + "_" + imageID);
                break;
            case "IMAGES":
                path = imageSetName + "/(" + imageID + ")";
                loadedSprite = Resources.Load<Sprite>(path);
                break;
            default:
                throw new Exception("INVALID IMAGESET TYPE");
        }

        Debug.Log("PATH: " + path);
        if (loadedSprite != null) {
            return loadedSprite;
        }
        else {
            throw new Exception("ImageID not found in spritesFromSet");
        }
    }
    
    
    private void RemoveStickerFromPool()
    {
        _spritesFromStage[_currentlySelectedImage] = (
            _spritesFromStage[_currentlySelectedImage].sprite,
            0);
        currentlyInGameImages.Remove(_currentlySelectedImage);
        // aca se setea si la imagen vuelve al set general o no  
        _spritesFromStage.Remove(_currentlySelectedImage);
    }

    private void Win() {
        audioSource.PlayOneShot(winClip);
        Debug.Log("Win");
        StartCoroutine(EndGame(winClip.length));
    }

    private bool AddImages(int amountOfImages) {
        //para agregar una imagen al pool, se mezclan todos los sprites,
        //y se agrega el primer sprite que no este en el pool, al pool

        List<(int id, Sprite sprite, int amountOfAppearances)> shuffledSprites =
            new List<(int id, Sprite sprite, int amountOfAppearances)>();
        foreach (var VARIABLE in _spritesFromStage) {
            shuffledSprites.Add((VARIABLE.Key, VARIABLE.Value.sprite, VARIABLE.Value.amountOfAppearances));
        }

        shuffledSprites.Shuffle();

        foreach (var sprite in shuffledSprites) {
            if (!currentlyInGameImages.Contains(sprite.id)) {
                currentlyInGameImages.Add(sprite.id);
                amountOfImages--;
                if (amountOfImages == 0) {
                    return true;
                }
            }
        }

        return false;
    }

    public IEnumerator EndGame(float delay) {
        
        SaveMatch();
        buttonReplay.transform.parent.gameObject.SetActive(true);
        gameEnded = true;
        Debug.Log("Match Ended");
        
        if (userData.GetUserStageData(selectedStage, selectedDifficulty).highScore < score)
        {
            yield return StartCoroutine(SetHighScore(score));
        }
        
        yield return new WaitForSeconds(delay);
        //animation achievements
        var firstTimeAchievements = userData.GetUserStageData(selectedStage, selectedDifficulty).AddMatch(_currentMatch);
        
        yield return stages[selectedStage].stageObject.difficultyButtons[selectedDifficulty].SetAchievements(firstTimeAchievements,0.5f);

        UpdateAchievementAndUnlockedLevels();
        //animationScore
        userData.coins += score;
    }

    private void UpdateAchievementAndUnlockedLevels()
    {
        foreach (var stageIndexAndData in stages) {
            foreach (var difficultyButton in stageIndexAndData.Value.stageObject.difficultyButtons)
            {
                difficultyButton.UpdateDifficultyUnlocked();
            }
        }
    }

    private void SaveMatch() {
        SaveUserData();
    }

    private IEnumerator SetHighScore(int highScoreToSet)
    {
        audioSource.Pause();
        audioSource.PlayOneShot(highScoreClip);
        yield return new WaitForSeconds(.25f);
        //Todo: Add highscore animation
        userData.GetUserStageData(selectedStage, selectedDifficulty).highScore = highScoreToSet;

        //oasar esto a userdata que llame automaticamente cuando se modificque el highscore en user data, agregar funcionn en vez de seteo directo
        stages[selectedStage].stageObject.SetScore(selectedDifficulty,highScoreToSet);
        
        highScoreText.text = highScoreToSet.ToString();
    }

    public void SetTimer(float timer) {
        this.timer = Mathf.Clamp(timer,0,maxTimer);
        timerText.text = ((int)this.timer).ToString();
        
        if (this.timer<=1)
        {
            lifeCounter.LoseLive();
            _spritesFromStage[_currentlySelectedImage] = (
                _spritesFromStage[_currentlySelectedImage].sprite,
                _spritesFromStage[_currentlySelectedImage].amountOfAppearances - 1);
            NextTurn();
            SetTimer(maxTimer);
        }
    }

    public void Lose()
    {
        Debug.Log("Lose");
        audioSource.PlayOneShot(endGameClip);
        StartCoroutine(EndGame(endGameClip.length));
    }

    public void Reset() {
        //if (disableInput) return;
        selectStageAndDifficultyCanvas.SetActive(false);
        gameCanvas.SetActive(true);
        SetNumpadByDifficulty(selectedDifficulty);
        bonusOnAmountOfAppearences = DifficultyToAmountOfAppearences(selectedDifficulty);
        gameEnded = false;
        buttonReplay.transform.parent.gameObject.SetActive(false);
        audioSource.Play();
        SetTimer(15);
        SetScore(0);
        turnNumber = 1;
        SetCurrentCombo(0);
        bonusMultiplicator = 1;
        lifeCounter.ResetLives();
        LoadImages(imageSetName.ToString());
        currentlyInGameImages = new List<int>();
        AddImages(4);
        //TODO: Arreglar este hardcodeo horrible, ver dentro de set random image como dividir la funcion
        _currentlySelectedImage = 0;
        SetRandomImage();
        disableInput = false;
        _currentMatch = new Match(selectedStage,selectedDifficulty,false);
        _currentCombo = 0;

    }

    public int DifficultyToAmountOfAppearences(int difficulty)
    {
        return (selectedDifficulty + 1)*3;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            AddClue();
            AddRemove();
        }
    }

    private void SetNumpadByDifficulty(int difficulty)
    {
        numpadRow1.SetActive(false);
        numpadRow2.SetActive(false);
        if (difficulty > 0)
        {
            numpadRow1.SetActive(true);
            if (difficulty > 1) numpadRow2.SetActive(true);
        }
    }

    private void FixedUpdate() {
        if (gameEnded) return;
        SetTimer(timer - Time.deltaTime);

    }
    public void MoveItemFromInventaryToMatchInventary(ShopItemType item)
    {
        if (inventary.ContainsKey(item))
        {
            if (matchInventary.ContainsKey(item))
            {
                matchInventary[item] = matchInventary[item]++;
            }
            else
            {
                matchInventary.Add(item, 1);
            }
            inventary[item]--;
            if (inventary[item] == 0)
            {
                inventary.Remove(item);
            }
        }

    }
    public void UseItemFromMatchInventary(ShopItemType item)
    {
        if (matchInventary.ContainsKey(item))
        {
            Debug.Log($"You uses {item}");
            matchInventary[item]--;
            if (matchInventary[item] == 0)
            {
                matchInventary.Remove(item);
            }
        }
        else
        {
            Debug.Log($"You dont have any {item}");
        }
    }
    public void AddItemToInventary(ShopItemType item)
    {
        if (inventary.ContainsKey(item))
        {
            inventary[item] = inventary[item]++;
        }
        else
        {
            inventary.Add(item, 1);
        }
    }
    public void AddUpgrade(ShopItemType item)
    {
        if (upgrades.ContainsKey(item))
        {
            upgrades[item] = upgrades[item]++;
        }
        else
        {
            upgrades.Add(item, 1);
        }
    }
    private IEnumerator Squash(Transform transform, float delay, float amount, float speed)
    {
        Vector3 initialScale = transform.localScale;
        while (delay > 0)
        {
            float scale = 1.0f + Mathf.Sin(Time.time * speed) * amount;
            transform.localScale = new Vector3(initialScale.x * scale, initialScale.y / scale, initialScale.z);
            delay -= Time.deltaTime;
            yield return new WaitForSeconds(0.01f);
        }
        transform.localScale = initialScale;
        StopCoroutine(Squash(transform, delay: 0, amount: 0, speed: 0));
    }
    private IEnumerator Wooble(Transform transform, float delay, float angleAmount, float angleSpeed, float verticalAmount, float verticalSpeed)
    {
        Vector3 initialPosition = transform.localPosition;
        Vector3 initialRotation = transform.localEulerAngles;
        while (delay > 0)
        {
            float angle = 1.0f + Mathf.Sin(Time.time * angleSpeed) * angleAmount;
            float verticalOffset = 1.0f + Mathf.PingPong(Time.time * verticalSpeed, verticalAmount);
            transform.localPosition = new Vector3(initialPosition.x, initialPosition.y + verticalOffset, initialPosition.z);
            transform.localEulerAngles = new Vector3(initialRotation.x, initialRotation.y, initialRotation.z + angle);
            delay -= Time.deltaTime;
            yield return new WaitForSeconds(0.01f);
        }
        transform.localEulerAngles = initialRotation;
        transform.localPosition = initialPosition;
        StopCoroutine(Wooble(transform, delay: 0, angleAmount: 0, angleSpeed: 0, verticalAmount: 0, verticalSpeed: 0));
    }
    private IEnumerator Shake(Transform transform, float delay, float amount, float speed)
    {
        Vector3 initialPosition = transform.localPosition;
        while (delay > 0)
        {
            float scale = 1.0f + Mathf.Sin(Time.time * speed) * amount;
            transform.localPosition = new Vector3(initialPosition.x + scale, initialPosition.y, initialPosition.z);
            delay -= Time.deltaTime;
            yield return new WaitForSeconds(0.01f);
        }
        transform.localPosition = initialPosition;
        StopCoroutine(Shake(transform, delay: 0, amount: 0, speed: 0));
    }

    public void SetStageAndDifficulty(int stage, int difficulty)
    {
        selectedDifficulty = difficulty;
        selectedStage = stage;
        SetScoreTexts();
        Reset();
    }

    public void OpenStagesCanvas()
    {
        selectStageAndDifficultyCanvas.SetActive(true);
        gameCanvas.SetActive(false);
    }
    private List<bool> ParseSavedClears(string allClears) {
        string[] clearedArray = allClears.Split(';');
        List<bool> clears = new List<bool>();

        foreach (string clear in clearedArray)
        {
            if (bool.TryParse(clear, out bool parsedScore))
            {
                clears.Add(parsedScore);
            }
            else
            {
                Debug.LogError("No se pudo convertir el string a bool: " + clear);
            }
        }
        // modificar para recibir tambien cantidad de imagenes en pack?
        // if (clears.Count != 3) {
        //     Debug.LogError("Cantidad incorrecta de clears " + allClears);
        // }
        return clears;
    }
    public static List<int> ParseSavedScore(string allScores) {
        //string highScoreString = PlayerPrefs.GetString("HighScore", "0;0;0");
        string[] highScoreArray = allScores.Split(';');
        List<int> highScores = new List<int>();

        foreach (string score in highScoreArray)
        {
            if (int.TryParse(score, out int parsedScore))
            {
                highScores.Add(parsedScore);
            }
            else
            {
                Debug.LogError("No se pudo convertir el string a entero: " + score);
            }
        }

        if (highScores.Count != 3) {
                Debug.LogError("Cantidad incorrecta de puntajes " + allScores);
        }
        return highScores;
    }
    
  

    private void OnApplicationQuit()
    {
        //SaveStages(stages);
        SaveUserData();
    }
    
  
    public void SaveStages(Dictionary<int, StageData> stagesToSave)
    {
        List<StageData> stageList = new List<StageData>(stagesToSave.Values);
        string json = JsonConvert.SerializeObject(stageList, Formatting.Indented);
        string filePath = Path.Combine(Application.persistentDataPath, "stages.json");
        File.WriteAllText(filePath, json);
        Debug.Log("Stages saved to " + filePath);
    }

    public IEnumerator LoadStages()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "stages.json");
        string json;
        if (!File.Exists(filePath))
        {
            Debug.Log("No saved stages found at " + filePath);
            yield return StartCoroutine(GetJson("stages"));
        }
        Debug.Log(filePath);
        json = File.ReadAllText(filePath);

        // Deserialize the JSON to the intermediate object
        Serialization<StageData> stageList = JsonConvert.DeserializeObject<Serialization<StageData>>(json);
        // Después de deserializar
   
        if (stageList == null || stageList.items == null)
        {
            Debug.LogError("Failed to deserialize stages.");
        }
        foreach (var stageData in stageList.items)
        {
            stageData.ConvertColorStringToColorValue();
        }
        
        // Convert the list to a dictionary
        Dictionary<int, StageData> stages = stageList.items.ToDictionary(stage => stage.stageID, stage => stage);
        Debug.Log("Stages loaded from " + filePath + " stages: " + stages.Count);
        this.stages = stages;
        yield return null;
        InitializeStages();
    }


    public void SaveUserData()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "userData.json");
        string json = JsonConvert.SerializeObject(userData, Formatting.Indented);
        File.WriteAllText(filePath, json);
        Debug.Log("UserData saved to " + filePath);
    }
    public IEnumerator LoadUserData()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "userData.json");
        if (!File.Exists(filePath))
        {
            Debug.Log("No userdata found at " + filePath);
            yield return StartCoroutine(GetJson("userData"));
        }

        string json = File.ReadAllText(filePath);
        UserData userData = JsonConvert.DeserializeObject<UserData>(json);

        if (userData != null)
        {
            Debug.Log("UserData loaded from " + filePath + " stages:" + userData.stages.Count);
        }
        else
        {
            Debug.LogError("Failed to load UserData from " + filePath);
        }
        
        
        this.userData =  userData;
        yield return null;
        yield return StartCoroutine(LoadStages());
        yield return StartCoroutine(LoadStickerLevels());
        yield return StartCoroutine(LoadPacks());
    }
    public IEnumerator LoadStickerLevels()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "stages.json");
        string json;
        if (!File.Exists(filePath))
        {
            Debug.Log("No saved stages found at " + filePath);
            yield return StartCoroutine(GetJson("stages"));
        }
        Debug.Log(filePath);
        json = File.ReadAllText(filePath);

        // Parse the JSON into a JObject
        JObject jsonData = JObject.Parse(json);

        // Deserialize the stickerLevels data
        JObject stickerLevelsJson = jsonData["stickerLevels"].ToObject<JObject>();
        Dictionary<int, StickerLevelsData> stickerLevels = new Dictionary<int, StickerLevelsData>();

        foreach (var item in stickerLevelsJson)
        {
            int level = int.Parse(item.Key);
            StickerLevelsData data = item.Value.ToObject<StickerLevelsData>();
            stickerLevels.Add(level, data);
        }

        Debug.Log("StickerLevels loaded from " + filePath + " levels: " + stickerLevels.Count);

        // Do whatever you need with the stickerLevels dictionary

        yield return null;
    }


    // Función para cargar los datos de packs
    public IEnumerator LoadPacks()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "stages.json");
        string json;
        if (!File.Exists(filePath))
        {
            Debug.Log("No saved stages found at " + filePath);
            yield return StartCoroutine(GetJson("stages"));
        }
        Debug.Log(filePath);
        json = File.ReadAllText(filePath);

        // Parse the JSON into a JObject
        JObject jsonData = JObject.Parse(json);

        // Extract the "packs" object
        JObject packsJson = jsonData["packs"].ToObject<JObject>();

        // Create a PacksData object and set its properties
        PacksData packsData = new PacksData
        {
            rareChance = packsJson["rareChance"].Value<float>(),
            rareAmountOfStickers = packsJson["rareAmountOfStickers"].Value<int>(),
            legendaryChance = packsJson["legendaryChance"].Value<float>(),
            legendaryAmountOfStickers = packsJson["legendaryAmountOfStickers"].Value<int>(),
            stickersPerPack = packsJson["stickersPerPack"].Value<int>()
        };

        packs = packsData;
    
        Debug.Log("Packs loaded from " + filePath);
        Debug.Log("stickersPerPack on load: " + packs.stickersPerPack);
        yield return null;
    }

    public int CalculateScoreComboBonus()
    {
        int maxComboBonus = 5;
        int calculatedComboBonus = (int)Math.Floor(((float)_currentCombo / 2));
        if (calculatedComboBonus > maxComboBonus)
        {
            calculatedComboBonus = maxComboBonus;
        }
        return calculatedComboBonus;
    }
    
    IEnumerator GetJson(string file_name)
    {
        string url = "https://leandromagonza.github.io/MemoGram.Pokemon/" + file_name + ".json";
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(www.error);
            }
            else
            {
                string filePath = Path.Combine(Application.persistentDataPath, file_name + ".json");
                File.WriteAllText(filePath, www.downloadHandler.text);
            }
        }
    }


}

public enum ImageSet {
    Pokemons_SPRITESHEET_151,
    Landscapes_IMAGES_10
}

public enum ShopItemType
{
    Consumible_Clue,
    Consumible_Remove,
    Consumible_Cut,
    Consumible_Peek,

    Upgrade_ExtraLife,
    Upgrade_ProtectedLife,
    Upgrade_MaxClue,
    Upgrade_BetterClue,
    Upgrade_MaxRemove,
    Upgrade_MaxCut,
    Upgrade_BetterCut,
    Upgrade_BetterPeek,
    Upgrade_Block,
    Upgrade_DeathDefy
}

[Serializable] 
public class Serialization<T>
{
    [SerializeField]
    public List<T> items;
    
    public Serialization(List<T> items)
    {
        this.items = items;
    }
}
