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

public class GameManager : MonoBehaviour {
    private static GameManager _instance;

    public bool gameEnded = false;
    public LifeCounter lifeCounter;
    public float timer = 3;
    public float maxTimer = 15;
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
    private Dictionary<int,(Sprite sprite, int amountOfAppearances)> _spritesFromSet = new Dictionary<int, (Sprite sprite, int amountOfAppearances)>();

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

    public Button buttonReplay;
    public float delayBetweenImages = .72f;

    public bool disableInput = false;

    public int selectedStage = 0;
    public int selectedDifficulty = 0;

    private Dictionary<int, StageData> _stages;

    public UserData userData = new UserData();
    public GameObject selectStageAndDifficultyCanvas;
    public GameObject gameCanvas;

    public GameObject numpadRow0;
    public GameObject numpadRow1;
    public GameObject numpadRow2;

    public GameObject stageDisplayPrefab;

    private Match _currentMatch;
    private int _currentStreak = 0;
    
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
        _stages = LoadStages();
        userData = LoadUserData();
        foreach (var stageIndexAndData in _stages) {
            //_stages = LoadStages();
            foreach (var stage in _stages.Values)
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
                if (userData.GetUserStageData(stageData.stageID,difficulty) is not null)
                {
                    newStage.SetScore(difficulty,userData.GetUserStageData(stageData.stageID,difficulty).highScore);
                }
                else
                {
                    newStage.SetScore(difficulty,0);
                }
                
            }
        }
        
        
        SetScoreTexts();

    }

    private void SetScoreTexts() {
        scoreText.text = userData.GetUserStageData(selectedStage, selectedDifficulty).highScore.ToString();
        highScoreText.text = userData.GetUserStageData(selectedStage, selectedDifficulty).highScore.ToString();
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
        Debug.Log("Guessed number " + number + ", amount of appearances " + _spritesFromSet[_currentlySelectedImage].amountOfAppearances);
        TurnAction turnAction;
        int scoreModification = 0;
        if (number == _spritesFromSet[_currentlySelectedImage].amountOfAppearances)
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
        CheckAmountOfAppearances();
        _currentMatch.AddTurn(
            _currentlySelectedImage,
            _spritesFromSet[_currentlySelectedImage].amountOfAppearances,
            0f,
            number,
            turnAction,
            lifeCounter.lives,
            _currentStreak,
            scoreModification
            );
        if (!gameEnded) NextTurn();
    }
   

    private void CheckAmountOfAppearances()
    {
        if (_spritesFromSet[_currentlySelectedImage].amountOfAppearances == bonusOnAmountOfAppearences)
        {
            Debug.Log("Clear: " + _spritesFromSet[_currentlySelectedImage].sprite.name);
            GainBonus();
            RemoveStickerFromPool();
        }
    }

    private void NextTurn() {
        turnNumber++;
        if (turnNumber % 10 == 0) {
            AddImages(turnNumber/10);
        }
        SetRandomImage();
        disableInput = false;

    }
    [ContextMenu("UseClue")]
    public void UseClue() {
        Debug.Log("USE CLUE");
        OnCorrectGuess();
        NextTurn();
    }

        [ContextMenu("UseRemove")]
    public void UseRemove()
    {
        Debug.Log("USE REMOVE");
        RemoveStickerFromPool();
        NextTurn();
    }
    private void OnIncorrectGuess()
    {
        IncorrectGuessFX();
        amountOfAppearancesText.SetAmountOfGuessesAndShowText(
            _spritesFromSet[_currentlySelectedImage].amountOfAppearances,
            false);
        audioSource.PlayOneShot(incorrectGuessClip);
        lifeCounter.LoseLive();
        _spritesFromSet[_currentlySelectedImage] = (
            _spritesFromSet[_currentlySelectedImage].sprite,
            _spritesFromSet[_currentlySelectedImage].amountOfAppearances - 1);
        SetCurrentStreak(0);
    }

    public void SetCurrentStreak(int newCurrentStreakAmount)
    {
        _currentStreak = newCurrentStreakAmount;
        //TODO: set currentStreak display and multiplayer display, and update here
    }
   

    private int OnCorrectGuess()
    {
        CorrectGuessFX();
        amountOfAppearancesText.SetAmountOfGuessesAndShowText(
            _spritesFromSet[_currentlySelectedImage].amountOfAppearances,
            true);
        audioSource.PlayOneShot(correctGuessClip);
        SetTimer(timer + currentTimerGain);
        int scoreModification = _stages[selectedStage].basePoints +
                                _spritesFromSet[_currentlySelectedImage].amountOfAppearances
                                + CalculateScoreStreakBonus();
        ModifyScore(scoreModification);
        SetCurrentStreak(_currentStreak+1);
        return scoreModification;
               ;
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
        ModifyScore(_spritesFromSet[_currentlySelectedImage].amountOfAppearances * bonusMultiplicator);
        bonusMultiplicator++;
        audioSource.PlayOneShot(bonusClip);
        lifeCounter.GainLive();
    }

    private void ModifyScore(int modificationAmount) {
        score += modificationAmount;
        scoreText.text = score.ToString();
        
    } 
    private void SetScore(int newScore) {
        score = newScore;
        scoreText.text = score.ToString();
    }

    private void LoadImages(string imageSetName) {
        _spritesFromSet = new Dictionary<int, (Sprite sprite, int amountOfAppearances)>();
        
        string[] splitedImageSetName = imageSetName.Split("_");
        
        string name = splitedImageSetName[0];
        string type = splitedImageSetName[1];
        int amount;
        int.TryParse(splitedImageSetName[2], out amount);

  
        foreach (var imageID in _stages[selectedStage].images) {
            AddImageFromSet(imageSetName, type, name, imageID);
        }
    }

  

    private void AddImageFromSet(string imageSetName, string type, string name, int imageID) {
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
            _spritesFromSet.Add(imageID, (loadedSprite, 0));
        }
        else {
            Debug.LogError("No se pudo cargar el sprite.");
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
        
        image.sprite = _spritesFromSet[nextImageID].sprite;
        _currentlySelectedImage = nextImageID;
        _spritesFromSet[_currentlySelectedImage]= (
            _spritesFromSet[_currentlySelectedImage].sprite,
            _spritesFromSet[_currentlySelectedImage].amountOfAppearances+1);
        imageIDText.text = (nextImageID+1).ToString();
        // image.sprite = _spritesFromSet[Random.Range(0, _spritesFromSet.Count)].sprite;
    }
    private void RemoveStickerFromPool()
    {
        _spritesFromSet[_currentlySelectedImage] = (
            _spritesFromSet[_currentlySelectedImage].sprite,
            0);
        currentlyInGameImages.Remove(_currentlySelectedImage);
        // aca se setea si la imagen vuelve al set general o no  
        _spritesFromSet.Remove(_currentlySelectedImage);
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
        foreach (var VARIABLE in _spritesFromSet) {
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
        buttonReplay.transform.parent.gameObject.SetActive(true);
        gameEnded = true;
        Debug.Log("Match Ended");
        yield return new WaitForSeconds(delay);
        if (userData.GetUserStageData(selectedStage, selectedDifficulty).highScore < score)
        {
            StartCoroutine(SetHighScore(score));
        }
    }

    private IEnumerator SetHighScore(int highScoreToSet)
    {
        audioSource.Pause();
        audioSource.PlayOneShot(highScoreClip);
        yield return new WaitForSeconds(.25f);
        //Todo: Add highscore animation
        userData.GetUserStageData(selectedStage, selectedDifficulty).highScore = highScoreToSet;

        //oasar esto a userdata que llame automaticamente cuando se modificque el highscore en user data, agregar funcionn en vez de seteo directo
        _stages[selectedStage].stageObject.SetScore(selectedDifficulty,highScoreToSet);
        
        highScoreText.text = highScoreToSet.ToString();
    }

    public void SetTimer(float timer) {
        this.timer = Mathf.Clamp(timer,0,maxTimer);
        timerText.text = ((int)this.timer).ToString();
        
        if (this.timer<=1)
        {
            Lose();
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
        bonusOnAmountOfAppearences = (selectedDifficulty+1)*3;
        gameEnded = false;
        buttonReplay.transform.parent.gameObject.SetActive(false);
        audioSource.Play();
        SetTimer(15);
        SetScore(0);
        turnNumber = 1;
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
        _currentStreak = 0;

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
        //SaveStages(_stages);
        userData.GetUserStageData(0, 0).highScore = 500;
        SaveUserData(userData);
    }
    
  
    public void SaveStages(Dictionary<int, StageData> stagesToSave)
    {
        List<StageData> stageList = stagesToSave.Values.ToList();
        string json = JsonUtility.ToJson(new Serialization<StageData>(stageList), true);
        string filePath = Path.Combine(Application.persistentDataPath, "_stages.json");
        File.WriteAllText(filePath, json);
        Debug.Log("Stages saved to " + filePath);
    }
    public void SaveUserData(UserData userData)
    {
        string filePath = Path.Combine(Application.persistentDataPath, "userData.json");
        string json = JsonConvert.SerializeObject(userData, Formatting.Indented);
        File.WriteAllText(filePath, json);
        Debug.Log("UserData saved to " + filePath);
    }
    public UserData LoadUserData()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "userData.json");
        if (!File.Exists(filePath))
        {
            Debug.Log("No saved userData found at " + filePath);
            return new UserData(); // O puedes devolver null o una nueva instancia con valores predeterminados
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
        return userData;
    }
    public Dictionary<int, StageData> LoadStages()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "stages.json");
        if (!File.Exists(filePath))
        {
            Debug.Log("No saved stages found at " + filePath);
            return new Dictionary<int, StageData>();
        }

        string json = File.ReadAllText(filePath);
        Serialization<StageData> stageList = JsonUtility.FromJson<Serialization<StageData>>(json);
        Dictionary<int, StageData> stages = stageList.items.ToDictionary(stage => stage.stageID, stage => stage);
        Debug.Log("Stages loaded from " + filePath);
        return stages;
    }
    public int CalculateScoreStreakBonus()
    {
        int maxStreakBonus = 5;
        int calculatedStreakBonus = (int)Math.Floor(((float)_currentStreak / 2));
        if (calculatedStreakBonus > maxStreakBonus)
        {
            calculatedStreakBonus = maxStreakBonus;
        }
        return calculatedStreakBonus;
    }

}

public enum ImageSet {
    Pokemons_SPRITESHEET_151,
    Landscapes_IMAGES_10
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
