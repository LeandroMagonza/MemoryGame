using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

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
    public int currentHighScore => PlayerPrefs.GetInt("HighScore",0);
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
            LoadImages(imageSetName.ToString());
            AddImages(4);
            SetRandomImage();
            audioSource = GetComponent<AudioSource>();
        }
        else if (_instance != this)
            DestroySelf();
    }
    private void Start()
    {
        scoreText.text = score.ToString();
        buttonTextClue.text = currentClues.ToString();
        buttonTextRemove.text = currentRemoves.ToString();
        highScoreText.text = currentHighScore.ToString();
    }
    private void DestroySelf() {
        if (Application.isPlaying)
            Destroy(this);
        else
            DestroyImmediate(this);
    }

    public IEnumerator ProcessGuess(int number)
    {
        disableInput = true;
        Debug.Log("Guessed number " + number + ", amount of appearances " + _spritesFromSet[_currentlySelectedImage].amountOfAppearances);
        if (number == _spritesFromSet[_currentlySelectedImage].amountOfAppearances)
        {
            Debug.Log("CorrectGuess");
            OnCorrectGuess();
        }
        else
        {
            Debug.Log("IncorrectGuess");
            OnIncorrectGuess();
        }
        yield return new WaitForSeconds(delayBetweenImages);
        // si ya la imagen aparecio 9 veces, se la saca del pool
        CheckAmountOfAppearances();
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

    private void AddClue()
    {
        currentClues++;
        currentClues = currentClues > maxCluesAmount ? maxCluesAmount : currentClues;
    }

    private void AddRemove() 
    {
        currentRemoves++; 
        currentRemoves = currentRemoves > maxRemovesAmount ? maxRemovesAmount : currentRemoves;
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
            _spritesFromSet[_currentlySelectedImage].amountOfAppearances,
            false);
        audioSource.PlayOneShot(incorrectGuessClip);
        lifeCounter.LoseLive();
        _spritesFromSet[_currentlySelectedImage] = (
            _spritesFromSet[_currentlySelectedImage].sprite,
            _spritesFromSet[_currentlySelectedImage].amountOfAppearances - 1);
    }

   

    private void OnCorrectGuess()
    {
        CorrectGuessFX();
        amountOfAppearancesText.SetAmountOfGuessesAndShowText(
            _spritesFromSet[_currentlySelectedImage].amountOfAppearances,
            true);
        audioSource.PlayOneShot(correctGuessClip);
        SetTimer(timer + currentTimerGain);
        ModifyScore(_spritesFromSet[_currentlySelectedImage].amountOfAppearances);
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
        List<int> selection = new List<int>() { 0, 3, 6, 24, 132, 95, 138, 148, 33, 130  };
        // for (int imageID = 0; imageID < amount; imageID++) {
        //     AddImageFromSet(imageSetName, type, name, imageID);
        // }

        foreach (var imageID in selection) {
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
        if (currentHighScore < score)
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
        PlayerPrefs.SetInt("HighScore",highScoreToSet);
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
}

public enum ImageSet {
    Pokemons_SPRITESHEET_151,
    Landscapes_IMAGES_10
}
