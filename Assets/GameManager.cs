using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour {
    private static GameManager _instance;

    public bool gameEnded = false;
    public LifeCounter lifeCounter;
    public float timer = 3;
    public float maxTimer = 15;
    public float currentTimergain = 5;
    public TextMeshProUGUI timerText; 
    public int score = 0;
    public TextMeshProUGUI scoreText; 
    public int turnNumber = 1;
    
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

    public Button buttonReplay;
    

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
    private void Start() {
        scoreText.text = score.ToString();
    }
    private void DestroySelf() {
        if (Application.isPlaying)
            Destroy(this);
        else
            DestroyImmediate(this);
    }
    public void ProcessGuess(int number) {
        Debug.Log("Guessed number "+number+", amount of appearances "+_spritesFromSet[_currentlySelectedImage].amountOfAppearances);
        if (number == _spritesFromSet[_currentlySelectedImage].amountOfAppearances) {
            Debug.Log("CorrectGuess");
            OnCorrectGuess();
        }
        else {
            Debug.Log("IncorrectGuess");
            OnIncorrectGuess();
        }

        if (!gameEnded) NextTurn();
    }

    private void NextTurn() {
        turnNumber++;
        if (turnNumber % 10 == 0) {
            AddImages(turnNumber/10);
        }
        SetRandomImage();

    }

    private void OnIncorrectGuess() {
        amountOfAppearancesText.SetAmountOfGuessesAndShowText(
            _spritesFromSet[_currentlySelectedImage].amountOfAppearances,
            false);
        audioSource.PlayOneShot(incorrectGuessClip);
        lifeCounter.LoseLive();
    }
    private void OnCorrectGuess() {
        amountOfAppearancesText.SetAmountOfGuessesAndShowText(
            _spritesFromSet[_currentlySelectedImage].amountOfAppearances,
            true);
        audioSource.PlayOneShot(correctGuessClip);
        SetTimer(timer+currentTimergain);
        ModifyScore(1);
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
        
        for (int imageID = 0; imageID < amount; imageID++) {
            string path;
            Sprite loadedSprite = null;
            switch (type) {
                case "SPRITESHEET":
                    path = imageSetName + "/" + name;
                    loadedSprite = Load(path,name+"_"+imageID);
                    break;
                case "IMAGES":
                    path = imageSetName + "/(" + imageID + ")";
                    loadedSprite = Resources.Load<Sprite>(path);
                    break;
                default:
                    throw new Exception("INVALID IMAGESET TYPE");
            }
            Debug.Log("PATH: "+path);
            if (loadedSprite != null) {
                _spritesFromSet.Add(imageID,(loadedSprite,0));
            }
            else {
                Debug.LogError("No se pudo cargar el sprite.");
            }
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
        return null;
    }
    
    private void SetRandomImage() {
        Image image = imageOnDisplay.GetComponent<Image>();
        
        // si ya la imagen aparecio 9 veces, se la saca del pool
        if (_spritesFromSet[_currentlySelectedImage].amountOfAppearances == 9) {
            _spritesFromSet[_currentlySelectedImage]= (
                _spritesFromSet[_currentlySelectedImage].sprite,
                0);
            currentlyInGameImages.Remove(_currentlySelectedImage);
            //bonus?
            ModifyScore(5);
            audioSource.PlayOneShot(bonusClip);
        }
  
        
        int nextImageIndex = Random.Range(0, currentlyInGameImages.Count);
        int nextImageID = currentlyInGameImages[nextImageIndex];

        while (_currentlySelectedImage == nextImageID) {
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

    public void EndGame() {
        buttonReplay.transform.parent.gameObject.SetActive(true);
        audioSource.PlayOneShot(endGameClip);
        gameEnded = true;
        Debug.Log("Match Ended");
    }

    public void SetTimer(float timer) {
        this.timer = Mathf.Clamp(timer,0,maxTimer);
        timerText.text = ((int)this.timer).ToString();
        
        if (this.timer<=1) {
            EndGame();
        }
    }
    public void Reset() {
        gameEnded = false;
        buttonReplay.transform.parent.gameObject.SetActive(false);
        audioSource.Play();
        SetTimer(15);
        SetScore(0);
        turnNumber = 1;
        lifeCounter.ResetLives();
        LoadImages(imageSetName.ToString());
        currentlyInGameImages = new List<int>();
        AddImages(4);
        //TODO: Arreglar este hardcodeo horrible, ver dentro de set random image como dividir la funcion
        _currentlySelectedImage = 0;
        SetRandomImage();

    }

    private void FixedUpdate() {
        if (gameEnded) return;
        SetTimer(timer - Time.deltaTime);

    }
}

public enum ImageSet {
    Pokemons_SPRITESHEET_151,
    Landscapes_IMAGES_10
}
