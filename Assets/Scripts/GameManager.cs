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


//revisar el cut tal vez el continue cambia el valor del iterador

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
            

            quizOptionButtons = new List<(string, NumpadButton)>();
            foreach (Transform option in quizOptionPad.transform)
            {
                quizOptionButtons.Add(("EMPTY",option.GetComponent<NumpadButton>()));
            }
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

    public GameObject numpad;
    public GameObject quizOptionPad;
    public bool limitOptionsToStage = true;
    public List<(string optionName, NumpadButton numpadButton)> quizOptionButtons = new List<(string optionName, NumpadButton numpadButton)>();
        
    public GameMode currentGameMode = GameMode.QUIZ;
    public bool gameEnded = false;
    public LifeCounter lifeCounter;
    public float timer = 3;
    public float maxTimer = 10;
    public TextMeshProUGUI timerText; 
    public int score = 0;
    public TextMeshProUGUI scoreText; 
    public int turnNumber = 1;
    public int bonusMultiplicator = 1;
 
    public TextMeshProUGUI highScoreText; 
    public TextMeshProUGUI endGameScoreText;
    public AchievementStars endGameAchievementStars;
    
    public FadeOut amountOfAppearencesText; 
    public GameObject pausePanel;

    public bool firstMistake = false;
    
    //sticker
    public Sticker stickerDisplay;
    public Vector3 startScale;
    //public TextMeshProUGUI imageIDText; 
    //public GameObject imageOnDisplay;

    //stickerData?
    private StickerData _currentlySelectedSticker;
    
    private Dictionary<int,StickerData> _stickersFromStage = new Dictionary<int, StickerData>();

    public Dictionary<StickerData,StickerMatchData> currentlyInGameStickers = new Dictionary<StickerData, StickerMatchData>();

    //public AudioSource audioSource;
    // public AudioClip correctGuessClip;
    // public AudioClip incorrectGuessClip;
    // public AudioClip endGameClip;
    // public AudioClip bonusClip;
    // public AudioClip winClip;
    // public AudioClip highScoreClip;

    public ParticleSystem correctGuessParticle;
    public ParticleSystem incorrectGuessParticle;

    public GameObject endGameButtons;
    public float delayBetweenImages = .72f;

    public bool disableInput = false;

    #region PersistanceReferences
    public UserData userData => PersistanceManager.Instance.userData;
    public Dictionary<int, StageData> stages => PersistanceManager.Instance.stages;
    private Dictionary<int, StickerLevelsData> stickerLevels => PersistanceManager.Instance.StickerLevels;
    public Dictionary<ConsumableID, (int current, int max, (int baseValue, int consumableValue) initial)> matchInventory = new Dictionary<ConsumableID, (int current, int max, (int baseValue, int consumableValue) initial)>();
    public PacksData packs => PersistanceManager.Instance.packs;
    #endregion

    public int currentClues = 0;
    public int maxCluesAmount = 5;
    public int currentRemoves = 0;
    public int maxRemovesAmount = 5;
    
    public GameObject gameCanvas;


    public NumpadButton[] numpadButtons;
    //public GameObject numpadRow0;
    //public GameObject numpadRow1;
    //public GameObject numpadRow2;

    private Match _currentMatch;
    private int _currentCombo = 0;
    public TextMeshProUGUI comboBonusText;
    public TextMeshProUGUI comboText;


    public GameCanvas GameCanvas => GameCanvas.Instance;
    public int selectedStage => StageManager.Instance.selectedStage;
    public int selectedDifficulty => StageManager.Instance.selectedDifficulty;

    public bool protectedLife = false;
    public bool deathDefy = false;
    public bool blockChoice = false;
    private int deathDefyMagnitude = 0;

    public float timeToIntersticial = 1;
    private float currentTimeToIntersticial = 0;
    private float startMatchTime = 0;
    private float endMatchTime = 0;

    private bool pause = false;

    public GameObject tutorialPanel;
    public List<GameObject> newPlayerTutorialTexts;
    public List<GameObject> firstMistakeTutorialTexts;

    public void SetScoreTexts() {
        if (userData.GetUserStageData(selectedStage, selectedDifficulty) is not null) {
            scoreText.text = userData.GetUserStageData(selectedStage, selectedDifficulty).highScore.ToString();
            highScoreText.text = userData.GetUserStageData(selectedStage, selectedDifficulty).highScore.ToString();
        }
    }



    public IEnumerator ProcessTurnAction(int number)
    {
        disableInput = true;
        StickerMatchData currentStickerMatchData = currentlyInGameStickers[_currentlySelectedSticker];
        CustomDebugger.Log("Guessed number " + number + ", amount of appearances " + currentStickerMatchData.amountOfAppearences);
        TurnAction turnAction;
        int scoreModification = 0;
        var turnSticker = _currentlySelectedSticker;
        
        if (number == GetCorrectGuess(turnSticker,currentStickerMatchData))
        {
            CustomDebugger.Log("CorrectGuess");
            scoreModification = OnCorrectGuess();
            turnAction = TurnAction.GuessCorrect;
        }
        else
        {
            CustomDebugger.Log("IncorrectGuess");
            int mistakeMagnitude = number - currentStickerMatchData.amountOfAppearences;
            deathDefyMagnitude = Mathf.Abs(mistakeMagnitude);
            OnIncorrectGuess(number);
            turnAction = TurnAction.GuessIncorrect;
        }
        
        //usar el dato del sticker tomado antes de la funcion oncorrectguess
        yield return FinishProcessingTurnAction(number, turnAction, scoreModification, turnSticker,currentStickerMatchData);
    }

    public int GetCorrectGuess(StickerData turnSticker,StickerMatchData stickerMatchData)
    {
        switch (currentGameMode)
        {
            case GameMode.MEMORY:
                return stickerMatchData.amountOfAppearences;
            case GameMode.QUIZ:
                return quizOptionButtons[0].numpadButton.number;
            default:
                return -1;
        }
    }

    public IEnumerator FinishProcessingTurnAction(int number, TurnAction turnAction,
        int scoreModification, StickerData stickerData, StickerMatchData stickerMatchData)
    {
        float timerModification = maxTimer - timer;
        SetTimer(maxTimer);
        yield return new WaitForSeconds(delayBetweenImages);
        
        //Checkeamos si el sticker que adivinamos recien es el ultimo que queda en el pool, y de ser asi le damos todos los puntos de una y sacamos el sticker
        if (currentlyInGameStickers.Count == 1 && currentlyInGameStickers.ContainsKey(_currentlySelectedSticker))
        {
            //se repite este while hasta que el sticker salga del pool, para sumar sus puntos, el check amount of appearences lo saca del pool cuando llega
            //a la cant de apariciones de la dificultad 
            while (currentlyInGameStickers.ContainsKey(_currentlySelectedSticker))
            {
                currentlyInGameStickers[_currentlySelectedSticker].amountOfAppearences++;
                scoreModification += CalculateCorrectGuessBasePointsAndCombo();
                SetCurrentCombo(_currentCombo+1);
                
                scoreModification += CheckAmountOfAppearences();
            }
        }
        SaveTurn(number, timerModification, turnAction, scoreModification, stickerData, stickerMatchData);
        
        if (currentlyInGameStickers.Count == 0) {
            StartCoroutine(EndGame(true));
        }
        else if(turnAction != TurnAction.UseCut) {
            NextTurn();
        }

        disableInput = false;
    }

    private void SaveTurn(int number, float timerModification, TurnAction turnAction, int scoreModification,StickerData stickerData,StickerMatchData stickerMatchData)
    {
        _currentMatch.AddTurn(
            stickerData.stickerID,
            stickerMatchData.amountOfAppearences,
            timerModification,
            number,
            turnAction,
            lifeCounter.lives,
            _currentCombo,
            scoreModification
        );
    }

    private int CheckAmountOfAppearences()
    {
        int scoreModificationBonus = 0;
        if (currentlyInGameStickers[_currentlySelectedSticker].amountOfAppearences == selectedDifficulty)
        {
            CustomDebugger.Log("Clear: " + _currentlySelectedSticker.name);
            scoreModificationBonus = GainBonus();
            RemoveStickerFromPool();
        }

        return scoreModificationBonus;
    }
    public void NextTurn() 
    {
        turnNumber++;
        if (currentlyInGameStickers.Count < 3)
        {
            AddStickers(1);
        }
        else if (turnNumber % 10 == 0)
        {
            AddStickers(turnNumber / 10);
        }

        SetRandomImage();
        GameCanvas.UpdateUI();
    }





    public void OnIncorrectGuess(int number)
    {
        if (!firstMistake)
        {
            firstMistake = true;
            OpenTutorialPanel(1);
        }
        if (blockChoice)
        {
            Debug.Log("Block");
            if (!GetCurrentlySelectedSticker().matchData.blockedNumbers.Contains(number))
            {
                Debug.Log("Block In");
                GetCurrentlySelectedSticker().matchData.AddBlockEffect(number);

            }
        }
        IncorrectGuessFX();
        amountOfAppearencesText.SetAmountOfGuessesAndShowText(
            currentlyInGameStickers[_currentlySelectedSticker].amountOfAppearences,
            false);
        AudioManager.Instance.PlayClip(GameClip.incorrectGuess);

        currentlyInGameStickers[_currentlySelectedSticker].amountOfAppearences--;
        bool DeathDefy = GetDeathDefy(deathDefyMagnitude);
        lifeCounter.LoseLive(ref protectedLife, DeathDefy);

        SetCurrentCombo(0);
    }

    private bool GetDeathDefy(float magnitude)
    {
        bool canDefyDeath = false;
        if (magnitude <= 1) 
        {
            canDefyDeath = true;
        }
        bool DeathDefy = lifeCounter.lives == 1 && deathDefy && canDefyDeath;
        if (DeathDefy)
            deathDefy = false;
        return DeathDefy;
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
        amountOfAppearencesText.SetAmountOfGuessesAndShowText(
            currentlyInGameStickers[_currentlySelectedSticker].amountOfAppearences,
            true);
        AudioManager.Instance.PlayClip(GameClip.correctGuess);
        int scoreModification = CalculateCorrectGuessBasePointsAndCombo();
        SetCurrentCombo(_currentCombo+1);
        ModifyScore(scoreModification);
        //Reset blocked and cut numbers for next appearence
        if (currentlyInGameStickers.ContainsKey(_currentlySelectedSticker))
        {
            currentlyInGameStickers[_currentlySelectedSticker].blockedNumbers = new List<int>();
            currentlyInGameStickers[_currentlySelectedSticker].cutNumbers = new List<int>();
        }
        scoreModification += CheckAmountOfAppearences();
        return scoreModification;
    }

    private int CalculateCorrectGuessBasePointsAndCombo()
    {
        return stages[selectedStage].basePoints +
               currentlyInGameStickers[_currentlySelectedSticker].amountOfAppearences
               + (_currentlySelectedSticker.level - 1)
               + CalculateScoreComboBonus();
    }

    private void CorrectGuessFX()
    {
        StartCoroutine(Squash(stickerDisplay.spriteHolder.transform.parent.transform, .2f, 0.1f, 5));
        correctGuessParticle.Play();
    }
    private void IncorrectGuessFX()
    {
        StartCoroutine(Shake(stickerDisplay.spriteHolder.transform, .2f, 5, 80));
        incorrectGuessParticle.Play();
    }

    [ContextMenu("GainBonus")]
    private int GainBonus()
    {
        CustomDebugger.Log("gain bonus");
        int scoreModificationBonus = currentlyInGameStickers[_currentlySelectedSticker].amountOfAppearences * (
                Mathf.Clamp((int)Math.Floor((float)bonusMultiplicator / 2), 0, selectedDifficulty));
        ModifyScore(scoreModificationBonus);
        bonusMultiplicator++;
        AudioManager.Instance.PlayClip(GameClip.bonus);
        lifeCounter.GainLive();
        return scoreModificationBonus;
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

    private void LoadStickers() {

        _stickersFromStage = new Dictionary<int, StickerData>();
        CustomDebugger.Log("Loading stickers for stage "+stages[selectedStage].stageID+", name "+stages[selectedStage].title+" from set "+ stages[selectedStage].stickerSet);

        foreach (var stickerID in stages[selectedStage].stickers) {
            StickerData stickerData = StickerManager.Instance.GetStickerDataFromSetByStickerID(stages[selectedStage].stickerSet, stickerID);
            _stickersFromStage.Add(stickerID, stickerData);
        }
    }


    
    private void SetRandomImage() {
        
         
        int nextStickerIndex = Random.Range(0, currentlyInGameStickers.Count);
        StickerData nextSticker = currentlyInGameStickers.Keys.ToList()[nextStickerIndex];

        while (_currentlySelectedSticker == nextSticker && currentlyInGameStickers.Count > 1) {
            nextStickerIndex = Random.Range(0, currentlyInGameStickers.Count);
            nextSticker = currentlyInGameStickers.Keys.ToList()[nextStickerIndex];
        }
        
        stickerDisplay.SetStickerData(nextSticker);
        
        _currentlySelectedSticker = nextSticker;
        if (currentGameMode == GameMode.QUIZ)
        {
            quizOptionButtons.Shuffle();

            foreach (var VARIABLE in quizOptionButtons)
            {
                VARIABLE.numpadButton.gameObject.SetActive(false);
            }

            quizOptionButtons[0].numpadButton.gameObject.SetActive(true);
            quizOptionButtons[0].numpadButton.SetText(_currentlySelectedSticker.name);
            quizOptionButtons[0].numpadButton.transform.GetChild(1).GetComponent<Image>().sprite =
                _currentlySelectedSticker.sprite;
            quizOptionButtons[0] = (_currentlySelectedSticker.name, quizOptionButtons[0].numpadButton);

            int amountOfQuizOptions = 3;
            
            List<StickerData> listForRandomOptions = new List<StickerData>();

            foreach (var VARIABLE in StickerManager.Instance.currentLoadedSetStickerData[stages[selectedStage].stickerSet].ToList())
            {
                bool isStickerInStage = stages[selectedStage].stickers.Contains(VARIABLE.Value.stickerID);
                if (VARIABLE.Value.stickerID != _currentlySelectedSticker.stickerID &&
                    VARIABLE.Value.name.ToUpper() != _currentlySelectedSticker.name.ToUpper() &&
                    ((limitOptionsToStage && isStickerInStage) || !limitOptionsToStage)
                    )
                {
                    listForRandomOptions.Add(VARIABLE.Value);
                }
            }

            listForRandomOptions.Shuffle();
            
            for (int optionNumber = 1; optionNumber < amountOfQuizOptions; optionNumber++)
            {
                if (optionNumber-1 >= listForRandomOptions.Count)
                {
                    break;
                }
                quizOptionButtons[optionNumber].numpadButton.gameObject.SetActive(true);
                quizOptionButtons[optionNumber] = (listForRandomOptions[optionNumber-1].name,quizOptionButtons[optionNumber].numpadButton);
                quizOptionButtons[optionNumber].numpadButton.SetText(listForRandomOptions[optionNumber-1].name);
                quizOptionButtons[optionNumber].numpadButton.transform.Find("Image").GetComponent<Image>().sprite = listForRandomOptions[optionNumber-1].sprite;
            }

        }
        
        currentlyInGameStickers[_currentlySelectedSticker].amountOfAppearences++;
    }

    public void RemoveStickerFromPool() {
        //currentlyInGameStickers[_currentlySelectedSticker].amountOfAppearences = 0;
        currentlyInGameStickers.Remove(_currentlySelectedSticker);
        // aca se setea si la imagen vuelve al set general o no  
        _stickersFromStage.Remove(_currentlySelectedSticker.stickerID);
    }
  
    private bool AddStickers(int amountOfImages) {
        //para agregar una imagen al pool, se mezclan todos los sprites,
        //y se agrega el primer sprite que no este en el pool, al pool
        
        //devuelve verdadero si se pudieron agregar al pool todas las imagenes requeridas

        List<StickerData> shuffledStickers =
            new List<StickerData >();
        foreach (var sticker in _stickersFromStage) {
            shuffledStickers.Add(sticker.Value);
        }

        shuffledStickers.Shuffle();

        foreach (var sticker in shuffledStickers) {
            if (!currentlyInGameStickers.ContainsKey(sticker)) {
                currentlyInGameStickers.Add(sticker,new StickerMatchData());
                amountOfImages--;
                if (amountOfImages == 0) {
                    return true;
                }
            }
        }

        return false;
    }
    public IEnumerator EndGame(bool win) {
        float delay;
        if (win) {
            CustomDebugger.Log("Win");
            AudioManager.Instance.PlayClip(GameClip.win);
            delay = AudioManager.Instance.clips[GameClip.win].length;
        }
        else
        {
            CustomDebugger.Log("Lose");
            AudioManager.Instance.PlayClip(GameClip.endGame);
            delay = AudioManager.Instance.clips[GameClip.endGame].length;
        }

        endGameButtons.transform.parent.gameObject.SetActive(true);
        gameEnded = true;
        CustomDebugger.Log("Match Ended");
        endMatchTime = Time.time;
        float elapsedTime = endMatchTime - startMatchTime;
        currentTimeToIntersticial += elapsedTime;
        userData.coins += score;
        var firstTimeAchievements = userData.GetUserStageData(selectedStage, selectedDifficulty).AddMatch(_currentMatch);
        SaveMatch();
        UpdateAchievementAndUnlockedLevels();
        //animationScore
        
        //Grant first time Achievemnts bonus
        yield return new WaitForSeconds(0.2f);

        const int steps = 10;
        int scoreIncrement = (score)/steps;
        for (int i = 0; i < steps; i++) {
            yield return new WaitForSeconds(.1f);
            delay -= .1f;
            endGameScoreText.text = (scoreIncrement * i).ToString();
        }
        endGameScoreText.text = score.ToString();

        yield return endGameAchievementStars.SetAchievements(_currentMatch.achievementsFulfilled,.35f);
        
        delay -= .35f * firstTimeAchievements.Count;
        delay -= 3f;
        yield return new WaitForSeconds(delay);
        
        if (userData.GetUserStageData(selectedStage, selectedDifficulty).highScore < score)
        {
            yield return StartCoroutine(SetHighScore(score));
        }
        
       
        //animation achievements
        
        
        
            
        if (currentTimeToIntersticial > timeToIntersticial)
        {
            //CustomDebugger.Log("this is where we would show an ad");
            AdmobAdsScript.Instance.ShowInterstitialAd();
            currentTimeToIntersticial = 0;
        }
    }

    public void UpdateAchievementAndUnlockedLevels()
    {
        foreach (var stageIndexAndData in stages) {
            if (stageIndexAndData.Value.stageObject is not null) stageIndexAndData.Value.stageObject.UpdateDifficultyUnlockedAndAmountOfStickersUnlocked();
        }
    }

    private void SaveMatch() {


        foreach (ConsumableID consumable in matchInventory.Keys)
        {
            int result = matchInventory[consumable].initial.consumableValue - matchInventory[consumable].current;
            if (result > 0)
                userData.modifyConsumableObject(consumable, -result);
        }
        PersistanceManager.Instance.SaveUserData();
    }
    private IEnumerator SetHighScore(int highScoreToSet)
    {
        float clipDuration = AudioManager.Instance.PlayClip(GameClip.highScore);
        float timeUntillHighScoreTextUpdate = 0.25f;
        userData.GetUserStageData(selectedStage, selectedDifficulty).highScore = highScoreToSet;
        stages[selectedStage].stageObject.SetScore(highScoreToSet);
        yield return new WaitForSeconds(timeUntillHighScoreTextUpdate);
        
        //Todo: agregar animacion de texto estrellas colores whatever
        highScoreText.text = highScoreToSet.ToString();
        yield return new WaitForSeconds(clipDuration - timeUntillHighScoreTextUpdate);
    }
    public void SetTimer(float timer) {
        this.timer = Mathf.Clamp(timer,0,maxTimer);
        timerText.text = ((int)this.timer).ToString();
        
        if (this.timer<=1) {
            OnRanOutOfTime();
        }
    }

    private void OnRanOutOfTime() {
        disableInput = true;
        lifeCounter.LoseLive(ref protectedLife, false);
        IncorrectGuessFX();
        amountOfAppearencesText.SetAmountOfGuessesAndShowText(
            currentlyInGameStickers[_currentlySelectedSticker].amountOfAppearences,
            false);
        currentlyInGameStickers[_currentlySelectedSticker].amountOfAppearences--;
        AudioManager.Instance.PlayClip(GameClip.incorrectGuess);
        SetTimer(maxTimer);
        StartCoroutine(FinishProcessingTurnAction(0, TurnAction.RanOutOfTime, 0, _currentlySelectedSticker,
            currentlyInGameStickers[_currentlySelectedSticker]));
    }

    public void Reset() 
    {

        switch (currentGameMode)
        {
            case GameMode.MEMORY:
                numpad.SetActive(true);
                quizOptionPad.SetActive(false);
                break;
            case GameMode.QUIZ:
                numpad.SetActive(false);
                quizOptionPad.SetActive(true);
                break;
        }
        startMatchTime = Time.time;
        endMatchTime = 0;
        AdmobAdsScript.Instance.LoadInterstitialAd();
        Instance.SetScoreTexts();
        if (stickerDisplay == null) {
            stickerDisplay = StickerManager.Instance.GetStickerHolder();
        }
        startScale = stickerDisplay.spriteHolder.transform.parent.transform.localScale;
        stickerDisplay.ConfigureForGame(currentGameMode);
        SetMatchInventory();
        GameCanvas.Instance.UpdateUI();
        protectedLife = userData.upgrades.ContainsKey(UpgradeID.LifeProtector) && userData.upgrades[UpgradeID.LifeProtector] > 0;
        deathDefy = userData.upgrades.ContainsKey(UpgradeID.DeathDefy) && userData.upgrades[UpgradeID.DeathDefy] > 0;
        blockChoice = userData.upgrades.ContainsKey(UpgradeID.BlockMistake) && userData.upgrades[UpgradeID.BlockMistake] > 0;
        SetNumpadByDifficulty(selectedDifficulty);
        gameCanvas.GetComponent<GameCanvas>().UpdateUI();
        gameEnded = false;
        stickerDisplay.gameObject.SetActive(true);
        endGameButtons.transform.parent.gameObject.SetActive(false);
        if (AudioManager.Instance.playMusic) AudioManager.Instance.audioSource.Play();
        SetTimer(15);
        SetScore(0);
        turnNumber = 1;
        SetCurrentCombo(0);
        bonusMultiplicator = 1;
        lifeCounter.ResetLives();
        LoadStickers();
        currentlyInGameStickers = new Dictionary<StickerData, StickerMatchData>();
        AddStickers(4);
        //TODO: Arreglar este hardcodeo horrible, ver dentro de set random image como dividir la funcion
        //TODO: recordar para que era el comentario de arriba, poirque capaz esta arreglado ya y no me acuerdo
        //TODO: JAJAJ
        _currentlySelectedSticker = null;
        SetRandomImage();
        disableInput = false;
        _currentMatch = new Match(selectedStage,selectedDifficulty,false);
        _currentCombo = 0;
        endGameScoreText.text = "0";
        endGameAchievementStars.ResetStars();
        GameCanvas.UpdateUI();
        pause = false;
        pausePanel.SetActive(false);
        tutorialPanel.SetActive(false);
        
        if (userData.stages[0].matches.Count == 0)
        {
            OpenTutorialPanel(0);
        }
        foreach (var stageData in userData.stages)
        {
            if (firstMistake) break;
            foreach (var match in stageData.matches)
            {
                if (!match.achievementsFulfilled.Contains(Achievement.ClearedStageNoMistakes))
                {
                    firstMistake = true;
                    break;
                }
            }                
        }
    }

    private void SetNumpadByDifficulty(int difficulty)
    {
        for (int i = 0; i < numpadButtons.Length; i++)
        {
            numpadButtons[i].gameObject.transform.parent.gameObject.SetActive(false);
            numpadButtons[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < difficulty; i++)
        {
            numpadButtons[i].gameObject.SetActive(true);
            numpadButtons[i].gameObject.transform.parent.gameObject.SetActive(true);
        }
    }

    private void FixedUpdate() {
        if (gameEnded || pause) return;
        SetTimer(timer - Time.deltaTime);

    }
    private IEnumerator Squash(Transform squashedTransform, float delay, float amount, float speed)
    {
        Vector3 initialScale = squashedTransform.localScale;
        while (delay > 0)
        {
            float scale = 1.0f + Mathf.Sin(Time.time * speed) * amount;
            squashedTransform.localScale = new Vector3(initialScale.x * scale, initialScale.y / scale, initialScale.z);
            delay -= Time.deltaTime;
            yield return new WaitForSeconds(0.01f);
        }
        squashedTransform.localScale = startScale;
        //StopCoroutine(Squash(squashedTransform, delay: 0, amount: 0, speed: 0));
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
        //StopCoroutine(Wooble(squashedTransform, delay: 0, angleAmount: 0, angleSpeed: 0, verticalAmount: 0, verticalSpeed: 0));
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
        //StopCoroutine(Shake(squashedTransform, delay: 0, amount: 0, speed: 0));
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
                CustomDebugger.LogError("No se pudo convertir el string a bool: " + clear);
            }
        }
        // modificar para recibir tambien cantidad de imagenes en pack?
        // if (clears.Count != 3) {
        //     CustomDebugger.LogError("Cantidad incorrecta de clears " + allClears);
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
                CustomDebugger.LogError("No se pudo convertir el string a entero: " + score);
            }
        }

        if (highScores.Count != 3) {
                CustomDebugger.LogError("Cantidad incorrecta de puntajes " + allScores);
        }
        return highScores;
    }
    public int CalculateScoreComboBonus()
    {
        int maxComboBonus = 5;
        int calculatedComboBonus = (int)Math.Floor(((float)_currentCombo / 2));
        if (calculatedComboBonus >= maxComboBonus)
        {
            if (userData.upgrades.ContainsKey(UpgradeID.LifeProtector) && userData.upgrades[UpgradeID.LifeProtector] > 0)
            {
                protectedLife = true;
                lifeCounter.ProtectHearts();
            }
            calculatedComboBonus = maxComboBonus;
        }
        return calculatedComboBonus;
    }
    public Canvas GetGameCanvas() {
        return gameCanvas.GetComponent<Canvas>();
    }

    public void SetMatchInventory()
    {
        matchInventory = userData.GetMatchInventory();
    }

    public (StickerData sticker,StickerMatchData matchData) GetCurrentlySelectedSticker()
    {
        if (_currentlySelectedSticker is null)
        {
            return (null, null);
        }

        if (!currentlyInGameStickers.ContainsKey(_currentlySelectedSticker)) {
            return (_currentlySelectedSticker, null);
        }
        return (_currentlySelectedSticker, currentlyInGameStickers[_currentlySelectedSticker]);
    }

    public void TogglePause()
    {
        pause = !pause;
        pausePanel.SetActive(pause);
    }    
    public void OpenTutorialPanel(int tutorialNumber)
    {
        switch (tutorialNumber)
        {
            case 0:
                foreach (var VARIABLE in newPlayerTutorialTexts) VARIABLE.SetActive(true);
                foreach (var VARIABLE in firstMistakeTutorialTexts) VARIABLE.SetActive(false);
                break;
            case 1 :
                foreach (var VARIABLE in firstMistakeTutorialTexts) VARIABLE.SetActive(true);
                foreach (var VARIABLE in newPlayerTutorialTexts) VARIABLE.SetActive(false);
                break;
            default:
                    CustomDebugger.Log("tutorialNumber not found",DebugCategory.TUTORIAL);
                    return;
        }
        pause = true;
        tutorialPanel.SetActive(pause);
    }    
    public void CloseTutorialPanel()
    {
        pause = false;
        tutorialPanel.SetActive(pause);
    }
}
public enum StickerSet {
    Pokemons_SPRITESHEET_151,
    Landscapes_IMAGES_10,
    AnatomyFractures_SPRITESHEET_10,
    AnatomyBones_SPRITESHEET_10,
    WorldFlags_IMAGES_51
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

public enum GameMode
{
    MEMORY,
    QUIZ
}