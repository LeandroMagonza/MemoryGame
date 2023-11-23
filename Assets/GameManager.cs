using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ColorUtility = UnityEngine.ColorUtility;
using Random = UnityEngine.Random;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour {
    #region Singleton
    private static GameManager _instance;
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
    private void DestroySelf() {
        if (Application.isPlaying)
            Destroy(this);
        else
            DestroyImmediate(this);
    }
    

    #endregion
    
    public bool gameEnded = false;
    public LifeCounter lifeCounter;
    public float timer = 3;
    public float maxTimer = 10;
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

    private int _currentlySelectedSticker;
    private Dictionary<int,(StickerData stickerData, int amountOfAppearances)> _stickersFromStage = new Dictionary<int, (StickerData stickerData, int amountOfAppearances)>();

    private List<int> currentlyInGameImages =new List<int>();

    public AudioSource audioSource;
    public AudioClip correctGuessClip;
    public AudioClip incorrectGuessClip;
    public AudioClip endGameClip;
    public AudioClip bonusClip;
    public AudioClip winClip;
    public AudioClip highScoreClip;

    public ParticleSystem correctGuessParticle;
    public ParticleSystem incorrectGuessParticle;

    public GameObject endGameButtons;
    public float delayBetweenImages = .72f;

    public bool disableInput = false;

    #region PersistanceReferences
    public UserData userData => PersistanceManager.Instance.userData;
    public Dictionary<int, StageData> stages => PersistanceManager.Instance.stages;
    private Dictionary<int, StickerLevelsData> stickerLevels => PersistanceManager.Instance.stickerLevels;
    public PacksData packs => PersistanceManager.Instance.packs;
    #endregion

    public int currentClues = 0;
    public int maxCluesAmount = 5;
    public int currentRemoves = 0;
    public int maxRemovesAmount = 5;
    public int selectedStage = 0;
    public int selectedDifficulty = 0;

    [FormerlySerializedAs("selectStageAndDifficultyCanvas")] public GameObject stageHolder;
    public GameObject gameCanvas;

    public GameObject numpadRow0;
    public GameObject numpadRow1;
    public GameObject numpadRow2;

    public GameObject stageDisplayPrefab;

    private Match _currentMatch;
    private int _currentCombo = 0;
    public TextMeshProUGUI comboBonusText;
    public TextMeshProUGUI comboText;

    

    private void Start()
    {
        //scoreText.text = score.ToString();
    }

    //TODO: Esta se va para manager stages o algo asi 
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



    public IEnumerator ProcessTurnAction(int number)
    {
        disableInput = true;
        Debug.Log("Guessed number " + number + ", amount of appearances " + _stickersFromStage[_currentlySelectedSticker].amountOfAppearances);
        TurnAction turnAction;
        int scoreModification = 0;
        var turnSticker = GetCurrentlySelectedSticker();
        if (number == turnSticker.amountOfAppearances)
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
        //usar el dato del sticker tomado antes de la funcion oncorrectguess
        yield return FinishProcessingTurnAction(number, turnAction, scoreModification, (turnSticker.stickerData,turnSticker.amountOfAppearances));
    }

    public (int stickerID,StickerData stickerData,int amountOfAppearances) GetCurrentlySelectedSticker()
    {
        return (_currentlySelectedSticker,
            _stickersFromStage[_currentlySelectedSticker].stickerData,
            _stickersFromStage[_currentlySelectedSticker].amountOfAppearances
            );
    }
    public IEnumerator FinishProcessingTurnAction(int number, TurnAction turnAction,
        int scoreModification, (StickerData stickerData,int amountOfAppearances) turnSticker)
    {
        float timerModification = maxTimer - timer;
        SetTimer(maxTimer);
        yield return new WaitForSeconds(delayBetweenImages);
        SaveTurn(number, timerModification, turnAction, scoreModification, turnSticker);
        if (!gameEnded) NextTurn();
    }

    private void SaveTurn(int number, float timerModification, TurnAction turnAction, int scoreModification,(StickerData stickerData,int amountOfAppearances) turnSticker)
    {
        _currentMatch.AddTurn(
            turnSticker.stickerData.stickerID,
            turnSticker.amountOfAppearances,
            timerModification,
            number,
            turnAction,
            lifeCounter.lives,
            _currentCombo,
            scoreModification
        );
    }

    private void CheckAmountOfAppearances()
    {
        if (_stickersFromStage[_currentlySelectedSticker].amountOfAppearances == bonusOnAmountOfAppearences)
        {
            Debug.Log("Clear: " + _stickersFromStage[_currentlySelectedSticker].stickerData.name);
            GainBonus();
            RemoveStickerFromPool();
        }
    }
    public void NextTurn() {
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



   

    public void OnIncorrectGuess()
    {
        IncorrectGuessFX();
        amountOfAppearancesText.SetAmountOfGuessesAndShowText(
            _stickersFromStage[_currentlySelectedSticker].amountOfAppearances,
            false);
        audioSource.PlayOneShot(incorrectGuessClip);
        lifeCounter.LoseLive();
        _stickersFromStage[_currentlySelectedSticker] = (
            _stickersFromStage[_currentlySelectedSticker].stickerData,
            _stickersFromStage[_currentlySelectedSticker].amountOfAppearances - 1);
        SetCurrentCombo(0);
    }

    public void SetCurrentCombo(int newCurrentComboAmount)
    {
        _currentCombo = newCurrentComboAmount;
        comboBonusText.text = CalculateScoreComboBonus().ToString();
        comboText.text = _currentCombo.ToString(); 
        //TODO: add animation
    }
   
    public int OnCorrectGuess()
    {
        CorrectGuessFX();
        amountOfAppearancesText.SetAmountOfGuessesAndShowText(
            _stickersFromStage[_currentlySelectedSticker].amountOfAppearances,
            true);
        audioSource.PlayOneShot(correctGuessClip);
        int scoreModification = stages[selectedStage].basePoints +
                                _stickersFromStage[_currentlySelectedSticker].amountOfAppearances
                                + CalculateScoreComboBonus();
        ModifyScore(scoreModification);
        SetCurrentCombo(_currentCombo+1);
        CheckAmountOfAppearances();
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
        ModifyScore(_stickersFromStage[_currentlySelectedSticker].amountOfAppearances * bonusMultiplicator);
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

    private void LoadStickers(string imageSetName) {
        _stickersFromStage = new Dictionary<int, (StickerData stickerData, int amountOfAppearances)>();
        
        string[] splitedImageSetName = imageSetName.Split("_");
        
        string name = splitedImageSetName[0];
        string type = splitedImageSetName[1];
        int amount;
        int.TryParse(splitedImageSetName[2], out amount);

        foreach (var stickerID in stages[selectedStage].stickers) {
            _stickersFromStage.Add(stickerID, (StickerManager.Instance.GetStickerDataFromSetByStickerID(imageSetName,stickerID), 0));
        }
    }


    
    private void SetRandomImage() {
        Image image = imageOnDisplay.GetComponent<Image>();
        
        if (currentlyInGameImages.Count == 0) {
            Win();
            return;
        }    
        int nextImageIndex = Random.Range(0, currentlyInGameImages.Count);
        int nextImageID = currentlyInGameImages[nextImageIndex];

        while (_currentlySelectedSticker == nextImageID && currentlyInGameImages.Count > 1) {
            nextImageIndex = Random.Range(0, currentlyInGameImages.Count);
            nextImageID = currentlyInGameImages[nextImageIndex];
        }
        
        image.sprite = _stickersFromStage[nextImageID].stickerData.sprite;
        
        _currentlySelectedSticker = nextImageID;
        _stickersFromStage[_currentlySelectedSticker]= (
            _stickersFromStage[_currentlySelectedSticker].stickerData,
            _stickersFromStage[_currentlySelectedSticker].amountOfAppearances+1);
        imageIDText.text = (nextImageID+1).ToString();
        // image.sprite = _spritesFromSet[Random.Range(0, _spritesFromSet.Count)].sprite;
    }

    public void RemoveStickerFromPool()
    {
        _stickersFromStage[_currentlySelectedSticker] = (
            _stickersFromStage[_currentlySelectedSticker].stickerData,
            0);
        currentlyInGameImages.Remove(_currentlySelectedSticker);
        // aca se setea si la imagen vuelve al set general o no  
        _stickersFromStage.Remove(_currentlySelectedSticker);
    }
    private void Win() {
        audioSource.PlayOneShot(winClip);
        Debug.Log("Win");
        StartCoroutine(EndGame(winClip.length));
    }
    private bool AddImages(int amountOfImages) {
        //para agregar una imagen al pool, se mezclan todos los sprites,
        //y se agrega el primer sprite que no este en el pool, al pool

        List<(int id, StickerData stickerData, int amountOfAppearances)> shuffledSprites =
            new List<(int id, StickerData stickerData, int amountOfAppearances)>();
        foreach (var VARIABLE in _stickersFromStage) {
            shuffledSprites.Add((VARIABLE.Key, VARIABLE.Value.stickerData, VARIABLE.Value.amountOfAppearances));
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
        endGameButtons.transform.parent.gameObject.SetActive(true);
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

    public void UpdateAchievementAndUnlockedLevels()
    {
        foreach (var stageIndexAndData in stages) {
                stageIndexAndData.Value.stageObject.UpdateDifficultyUnlockedAndAmountOfStickersUnlocked();
        }
    }

    private void SaveMatch() {
        PersistanceManager.Instance.SaveUserData();
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
            _stickersFromStage[_currentlySelectedSticker] = (
                _stickersFromStage[_currentlySelectedSticker].stickerData,
                _stickersFromStage[_currentlySelectedSticker].amountOfAppearances - 1);
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
        SetNumpadByDifficulty(selectedDifficulty);
        bonusOnAmountOfAppearences = DifficultyToAmountOfAppearences(selectedDifficulty);
        gameEnded = false;
        endGameButtons.transform.parent.gameObject.SetActive(false);
        audioSource.Play();
        SetTimer(15);
        SetScore(0);
        turnNumber = 1;
        SetCurrentCombo(0);
        bonusMultiplicator = 1;
        lifeCounter.ResetLives();
        LoadStickers(imageSetName.ToString());
        currentlyInGameImages = new List<int>();
        AddImages(4);
        //TODO: Arreglar este hardcodeo horrible, ver dentro de set random image como dividir la funcion
        _currentlySelectedSticker = 0;
        SetRandomImage();
        disableInput = false;
        _currentMatch = new Match(selectedStage,selectedDifficulty,false);
        _currentCombo = 0;

    }

    public int DifficultyToAmountOfAppearences(int difficulty)
    {
        return (selectedDifficulty + 1)*3;
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
    public Canvas GetGameCanvas() {
        return gameCanvas.GetComponent<Canvas>();
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
