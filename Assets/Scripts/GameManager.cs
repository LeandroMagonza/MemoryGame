using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Purchasing.Extension;
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
                CustomDebugger.LogError("Singleton<" + typeof(GameManager) + "> instance has been not found.");
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

    [Header("Config")]
    public GameMode currentGameMode = GameMode.QUIZ; 
    public float delayBetweenImages = .72f;
    public bool limitOptionsToStage = true;
    public bool increaseAmountOfAppearencesOnMistake;

    [Header("State")]
    public bool gameEnded = false;
    private Match _currentMatch;
    public bool pause = false;
    public int turnNumber = 1;
    public bool firstMistake = false;
    private StickerData _currentlySelectedSticker;
    private Dictionary<int,StickerData> _remainingStickersFromStage = new Dictionary<int, StickerData>();
    public Dictionary<StickerData,StickerMatchData> currentlyInGameStickers = new Dictionary<StickerData, StickerMatchData>();
    private float startMatchTime = 0;
    private float endMatchTime = 0; 
    public bool disableInput = false;
    
    [Header("Skills")]
    public bool protectedLife = false;
    public int protectedLifeOnComboAmount = 10;
    public int deathDefyCharges = 0;
    private int deathDefyMagnitude = 0;
    public int baseClearsToHeal = 5;
    public int currentClears = 0;

    [Header("Timer")]    
    public float timer = 3;
    public float maxTimer = 10;
    public float maxTimerSpeedrun = 2;
    
    [Header("Score")]
    public int score = 0;
    public int bonusMultiplicator = 1;
    
    [Header("Combo")]
    [SerializeField] private int _currentCombo = 0;
    [SerializeField] private int maxComboBonus = 5;
    
    [Header("End Game Panel")]
    public TextMeshProUGUI highScoreText; 
    public TextMeshProUGUI endGameScoreText;
    public TextMeshProUGUI currentCoins;
    public AchievementStars endGameAchievementStars;
    public GameObject endGameButtons;
    public ChangeCanvasButton nextStageButton;


    //[Header("Pause")] 
    public GameObject pausePanel => gameCanvas.pausePanel;
    
    [Header("NumpadQuizOptionsPad")]
    public GameObject numpad;
    
    [Header("QuizOptionsPad")]
    public GameObject quizOptionPad;
    public List<(string optionName, NumpadButton numpadButton)> quizOptionButtons = new List<(string optionName, NumpadButton numpadButton)>();

    
    [Header("Frame")]   
    public FadeOut amountOfAppearencesText; 
    public Sticker stickerDisplay;
    public Vector3 startScale;
    
    [Header("Tutorial")]
    public GameObject tutorialPanel;
    public List<GameObject> newPlayerTutorialTexts;
    public List<GameObject> firstMistakeTutorialTexts;
    
    [Header("Particles")]
    public ParticleSystem correctGuessParticle;
    public ParticleSystem incorrectGuessParticle;
        
    public float squashDelay = 0.2f;
    public float squashAmount = 0.1f;
    public float squashSpeed = 5f;
    
    public float shakeDelay = .2f;
    public float shakeAmount = 5;
    public float shakeSpeed = 80;
    
    [Header("Visualizations")]
    public int currentlyInGameStickersTotalAmount;
    public int currentlySelectedStickerID;
    public string currentlySelectedStickerName;
    public int currentlySelectedStickerAmountOfAppearences;
    public string[] currentlyInGameStickersNames;
    

    //[Header("References")]
    #region References
    #region Game Canvas Header
    
    public GameCanvas gameCanvas => CanvasManager.Instance.gameCanvas.GetComponent<GameCanvas>();
    public GameCanvasHeader gameCanvasHeader => gameCanvas.gameCanvasHeader;
    public StageGroupIntroPanel stageGroupIntroPanel => gameCanvas.stageGroupIntroPanel;
    public LifeCounter lifeCounter  => gameCanvasHeader.lifeCounter ;
    public TextMeshProUGUI timerText => gameCanvasHeader.timerText; 
    public TextMeshProUGUI scoreText => gameCanvasHeader.scoreText;
    public TextMeshProUGUI comboText => gameCanvasHeader.comboText;
    public TextMeshProUGUI comboBonusText => gameCanvasHeader.comboBonusText;

    #endregion
    public UserData userData => PersistanceManager.Instance.userData;
    public UserConsumableData userConsumableData => PersistanceManager.Instance.userConsumableData;
    //public Dictionary<int, StageData> stages => PersistanceManager.Instance.stages;
    private Dictionary<int, StickerLevelsData> stickerLevels => PersistanceManager.Instance.StickerLevels;
    public Dictionary<ConsumableID, (int current, int max, (int baseValue, int consumableValue) initial)> matchInventory = new ();
    public PacksData packs => PersistanceManager.Instance.packs;
    public int selectedLevel => StageManager.Instance.selectedStage;
    public int selectedDifficulty => StageManager.Instance.selectedDifficulty;
    #endregion

    [Header("Modes")]
    public bool perfectMode = false;

    public bool speedrunMode = false;
    public int startingStickerAmount = 4;
    public int amountOfTurnsBetweenAddingStickers = 10;
    public int flatAmountOfStickerAdded = 0;
    public int scalingAmountOfStickerAdded = 1;


    public void SetScoreTexts() {
        if (userData.GetUserStageData(selectedLevel, selectedDifficulty) is not null) {
            currentCoins.text = userData.coins.ToString();
            endGameScoreText.text = "0";
            highScoreText.text = userData.GetUserStageData(selectedLevel, selectedDifficulty).highScore.ToString();
        }
        else {
            CustomDebugger.Log("stage data not found when setting highscore on endmatchscreen");
        }
    }



    public IEnumerator ProcessTurnAction(int guessNumber, bool ranOutOfTime = false)
    {
        disableInput = true;
        StickerMatchData currentStickerMatchData = currentlyInGameStickers[_currentlySelectedSticker];
        CustomDebugger.Log("Guessed number " + guessNumber + ", amount of appearances " + currentStickerMatchData.amountOfAppearences);
        TurnAction turnAction;
        int scoreModification = 0;
        var turnSticker = _currentlySelectedSticker;
        bool defeat = false;
        if (guessNumber == GetCorrectGuess(turnSticker,currentStickerMatchData))
        {
            CustomDebugger.Log("CorrectGuess");
            scoreModification = OnCorrectGuess();
            turnAction = TurnAction.GuessCorrect;
        }
        else
        {
            CustomDebugger.Log("IncorrectGuess");
            int mistakeMagnitude = guessNumber - currentStickerMatchData.amountOfAppearences;
            deathDefyMagnitude = Mathf.Abs(mistakeMagnitude);
            defeat = OnIncorrectGuess(guessNumber);
            turnAction = TurnAction.GuessIncorrect;
            if (ranOutOfTime) {
                turnAction = TurnAction.RanOutOfTime;
            }
        }
        
        if (currentlyInGameStickers.ContainsKey(_currentlySelectedSticker))
        {
            currentlyInGameStickers[_currentlySelectedSticker].cutNumbers = new List<int>();
        }
        
        //usar el dato del sticker tomado antes de la funcion oncorrectguess
        yield return FinishProcessingTurnAction(guessNumber, turnAction, scoreModification, turnSticker,currentStickerMatchData,defeat);
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
        int scoreModification, StickerData stickerData, StickerMatchData stickerMatchData, bool defeat)
    {
        float timerModification = maxTimer - timer;
        ResetTimer();
        SaveTurn(number, timerModification, turnAction, scoreModification, stickerData, stickerMatchData);
        //Checkeamos si el sticker que adivinamos recien es el ultimo que queda en el pool, y de ser asi le damos todos los puntos de una y sacamos el sticker
        if ((turnAction == TurnAction.GuessCorrect || turnAction== TurnAction.UseClue)&& currentlyInGameStickers.Count == 1 && currentlyInGameStickers.ContainsKey(_currentlySelectedSticker))
        {
            //se repite este while hasta que el sticker salga del pool, para sumar sus puntos, el check amount of appearences lo saca del pool cuando llega
            //a la cant de apariciones de la dificultad 
            int amountOfAppearences;
            while (currentlyInGameStickers.ContainsKey(_currentlySelectedSticker)) {
                scoreModification = 0;
                currentlyInGameStickers[_currentlySelectedSticker].amountOfAppearences++;
                amountOfAppearences = currentlyInGameStickers[_currentlySelectedSticker].amountOfAppearences;
                scoreModification += CalculateCorrectGuessBasePointsAndCombo();
                SetCurrentCombo(_currentCombo+1);
                
                scoreModification += CheckAmountOfAppearences();
                score += scoreModification;
                SaveTurn(amountOfAppearences, 0, turnAction, scoreModification, stickerData, stickerMatchData);
                
            }
        }
        yield return new WaitForSeconds(delayBetweenImages);
        
        if (defeat)
        {
            StartCoroutine(EndGame(false));
        }
        else if (currentlyInGameStickers.Count == 0) {
            StartCoroutine(EndGame(true));
        }
        else if(turnAction != TurnAction.UseCut) {
            //yield return new WaitForSeconds(delayBetweenImages);
            
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
            lifeCounter.currentLives,
            _currentCombo,
            scoreModification
        );
    }

    private int CheckAmountOfAppearences(bool correct = true)
    {
        int scoreModificationBonus = 0;
        if (currentlyInGameStickers[_currentlySelectedSticker].amountOfAppearences == selectedDifficulty)
        {
            CustomDebugger.Log("Clear: " + _currentlySelectedSticker.name);
            if (correct) scoreModificationBonus = GainBonus();
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
        else if (turnNumber % amountOfTurnsBetweenAddingStickers == 0)
        {
            AddStickers(flatAmountOfStickerAdded + (turnNumber / amountOfTurnsBetweenAddingStickers) * scalingAmountOfStickerAdded);
        }

        SetRandomImage();
        gameCanvas.UpdateUI();
    }





    public bool OnIncorrectGuess(int number)
    {
        if (currentlyInGameStickers[_currentlySelectedSticker].cutNumbers.Count > 0 ) {
            currentlyInGameStickers[_currentlySelectedSticker].remainingCuts++;
        }
        if (userData.GetUpgradeLevel(UpgradeID.BlockMistake) > 0)
        {
            CustomDebugger.Log("Block");
            GetCurrentlySelectedSticker().matchData.remainingCuts += userData.GetUpgradeLevel(UpgradeID.BlockMistake);
        }
        IncorrectGuessFX();
        amountOfAppearencesText.SetAmountOfGuessesAndShowText(
            currentlyInGameStickers[_currentlySelectedSticker].amountOfAppearences,
            false);
        AudioManager.Instance.PlayClip(GameClip.incorrectGuess,1);
        if (increaseAmountOfAppearencesOnMistake) {
            CheckAmountOfAppearences(false);
        }
        else {
            currentlyInGameStickers[_currentlySelectedSticker].amountOfAppearences--;
        }

       
        SetCurrentCombo(0);
        bool hasDefiedDeath = GetDeathDefy(deathDefyMagnitude);

        if (hasDefiedDeath) {
            //TODO: Agregar death defy animation
            return false;
        }
            
        return lifeCounter.LoseLive(ref protectedLife);
    }

    private bool GetDeathDefy(float magnitude)
    {
        bool isSmallMistake = false;
        if (magnitude <= 1) 
        {
            isSmallMistake = true;
        }
        bool hasDefiedDeath = lifeCounter.currentLives == 1 && deathDefyCharges > 0 && isSmallMistake;
        if (hasDefiedDeath)
            deathDefyCharges--;
        return hasDefiedDeath;
    }

    public void SetCurrentCombo(int newCurrentComboAmount)
    {
        _currentCombo = newCurrentComboAmount;
        int calculatedComboBonus = CalculateScoreComboBonus();
        comboBonusText.text = calculatedComboBonus.ToString();
        if (newCurrentComboAmount == 0 )
        {
            comboText.text = "";  
        }
        else
        {
            comboText.text = _currentCombo.ToString() + " COMBO!"; 
        }
        
        if (!protectedLife &&
            _currentCombo >= protectedLifeOnComboAmount && 
            userData.unlockedUpgrades.ContainsKey(UpgradeID.LifeProtector) && 
            userData.unlockedUpgrades[UpgradeID.LifeProtector] > 0)
        {
            protectedLife = true;
            lifeCounter.ProtectHearts();
            AudioManager.Instance.PlayClip(GameClip.bonus); 
        }

        //TODO: add animation  y que ahga el ruido siempre que llegues al combo maximo, pero no despoues
    }
   
    public int OnCorrectGuess()
    {
        CorrectGuessFX();
        amountOfAppearencesText.SetAmountOfGuessesAndShowText(
            currentlyInGameStickers[_currentlySelectedSticker].amountOfAppearences,
            true);
        AudioManager.Instance.PlayClip(GameClip.correctGuess,1);
        // probe pitch shift pero no me convence, me distrae y hace que sea mas complicado con los otros sonidos
        // necesitaria un audio soucer especifico para este clip asi el resto no hace shift tmb mientras se reproduce este sonido
        // y podria tener otro igual para el bonus, y que se haga mas augudo mientras mas bonus tengas / menos imagenes te queden
        // y podria haber otro para los errores, que se haga mas grave mientras menos vida tengas
        //AudioManager.Instance.PlayClip(GameClip.correctGuess,Mathf.Pow(1.05946f, 2 * CalculateScoreComboBonus()));
        
        
        int scoreModification = CalculateCorrectGuessBasePointsAndCombo();
        SetCurrentCombo(_currentCombo+1);
        ModifyScore(scoreModification);
        //Reset blocked and cut numbers for next appearence

        scoreModification += CheckAmountOfAppearences();
        return scoreModification;
    }

    private int CalculateCorrectGuessBasePointsAndCombo()
    {
        return (int)MathF.Floor(selectedLevel / 2) +
               currentlyInGameStickers[_currentlySelectedSticker].amountOfAppearences
               + (_currentlySelectedSticker.level - 1)
               + CalculateScoreComboBonus();
    }

    private void CorrectGuessFX()
    {
        StartCoroutine(Squash(stickerDisplay.spriteHolder.transform.parent.transform, squashDelay, squashAmount, squashSpeed));
        correctGuessParticle.Play();
    }
    private void IncorrectGuessFX()
    {
        StartCoroutine(Shake(stickerDisplay.spriteHolder.transform, shakeDelay, shakeAmount, shakeSpeed));
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
        currentClears++;
        AudioManager.Instance.PlayClip(GameClip.bonus);
        if (userData.GetUpgradeLevel(UpgradeID.HealOnClear) > 0 && 
            (currentClears % (baseClearsToHeal - userData.GetUpgradeLevel(UpgradeID.HealOnClear)) ) == 0) {
            lifeCounter.GainLive();
        }
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

        _remainingStickersFromStage = new Dictionary<int, StickerData>();
        CustomDebugger.Log("Loading stickers for stage "+selectedLevel+" from set "+ StageManager.Instance.gameVersion);
        //david cantidad total stages[selectedStage].stickers.Count
        int amountOfStickersToSelect = selectedLevel;
        
        //StickerManager.Instance.currentLoadedSetStickerData
  
        
        var randomGroupSelected = StageManager.Instance.selectedStickerGroup;

        if (StickerManager.Instance.currentLoadedStickerGroups[StageManager.Instance.gameVersion][randomGroupSelected.name].stickers.Count < amountOfStickersToSelect) {
            Debug.LogError("Se intentaron seleccionar mas stickers que los que hay en el grupo");
        }

        var shuffledStickers = StickerManager.Instance.currentLoadedStickerGroups[StageManager.Instance.gameVersion][randomGroupSelected.name].stickers.ToList().Shuffle();

        for (int currentStickerSelectedIndex = 0; currentStickerSelectedIndex < amountOfStickersToSelect; currentStickerSelectedIndex++) {
            int selectedStickerID = shuffledStickers[currentStickerSelectedIndex];
            StickerData stickerData =
                StickerManager.Instance.GetStickerDataFromSetByStickerID(StageManager.Instance.gameVersion, selectedStickerID);
            _remainingStickersFromStage.Add(selectedStickerID, stickerData);
        }
        //de este grupo 
        //StickerManager.Instance.currentLoadedStickerGroups[StageManager.Instance.gameVersion][randomGroupSelectedName];
        //seleccionar amountOfStickersToSelect, ordenar random y quedar con los amountoftickestoselect primeros?
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

        #region QuizMode
        //TODO: Revisar porque hubo muchos cambios en como se hacen los stages que hay que actualizar en este modo
        /*if (currentGameMode == GameMode.QUIZ)
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

            foreach (var VARIABLE in StickerManager.Instance.currentLoadedSetStickerData[StageManager.Instance.gameVersion].ToList())
            {
                bool isStickerInStage = stages[selectedLevel].stickers.Contains(VARIABLE.Value.stickerID);
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
        */
        #endregion
        currentlyInGameStickers[_currentlySelectedSticker].amountOfAppearences++;
        
        //agrego un cut con 0 para que se apliquen los remaining cuts
        currentlyInGameStickers[_currentlySelectedSticker].AddCutEffect(
            currentlyInGameStickers[_currentlySelectedSticker].amountOfAppearences,
            selectedDifficulty,
            0);
    }

    public void RemoveStickerFromPool() {
        //currentlyInGameStickers[_currentlySelectedSticker].amountOfAppearences = 0;
        currentlyInGameStickers.Remove(_currentlySelectedSticker);
        gameCanvas.barController.IncreaseCounter();
        // aca se setea si la imagen vuelve al set general o no  
        //_remainingStickersFromStage.Remove(_currentlySelectedSticker.stickerID);
    }
  
    private bool AddStickers(int amountOfImages) {
        //para agregar una imagen al pool, se mezclan todos los sprites,
        //y se agrega el primer sprite que no este en el pool, al pool
        
        //devuelve verdadero si se pudieron agregar al pool todas las imagenes requeridas

        List<StickerData> shuffledStickers =
            new List<StickerData >();
        foreach (var sticker in _remainingStickersFromStage) {
            shuffledStickers.Add(sticker.Value);
        }

        shuffledStickers.Shuffle();

        foreach (var sticker in shuffledStickers) {
            if (!currentlyInGameStickers.ContainsKey(sticker)) {
                currentlyInGameStickers.Add(sticker,new StickerMatchData());
                _remainingStickersFromStage.Remove(sticker.stickerID);
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
        
        
        
        endGameButtons.transform.parent.transform.parent.gameObject.SetActive(true);
        gameEnded = true;
        
        CustomDebugger.Log("Match Ended");
        endMatchTime = Time.time;
        float elapsedTime = endMatchTime - startMatchTime;
        AdmobAdsManager.Instance.ReduceInstertitialTime(elapsedTime);
        int formerCoins = userData.coins;
        userData.experiencePoints += score;
        var firstTimeAchievements = userData.GetUserStageData(selectedLevel, selectedDifficulty).AddMatch(_currentMatch);


        //si no hay proximo stage desactivo el boton play next stage
        if ((selectedLevel < 13 || selectedDifficulty < 9)
            &&
            (userData.GetUserStageData(selectedLevel, selectedDifficulty).achievements
                .Contains(Achievement.BarelyClearedStage))
            ) {
            nextStageButton.gameObject.SetActive(true);
        }
        else {
            nextStageButton.gameObject.SetActive(false);
        }
        
        int lastHighScore = userData.GetUserStageData(selectedLevel, selectedDifficulty).highScore;
        if (lastHighScore < score)
        {
            userData.GetUserStageData(selectedLevel, selectedDifficulty).highScore = score;
        }
        UpdateInventoryAndSave();
        UpdateAchievementAndUnlockedLevels();
        //Aca puede que haya que actualizar las estrellas en el nivel
        StageManager.Instance.InitializeStages();
        //animationScore
        
        //Grant first time Achievemnts bonus
        yield return new WaitForSeconds(0.2f);

        const int steps = 10;
        int scoreIncrement = (score)/steps;
        for (int i = 0; i < steps; i++) {
            yield return new WaitForSeconds(.1f);
            delay -= .1f;
            endGameScoreText.text = (scoreIncrement * i).ToString();
            currentCoins.text = (formerCoins + scoreIncrement * i).ToString();
        }
        currentCoins.text = userData.coins.ToString();
        endGameScoreText.text = score.ToString();

        yield return endGameAchievementStars.SetAchievements(_currentMatch.achievementsFulfilled,.35f);
        
        delay -= .35f * firstTimeAchievements.Count;
        delay -= 5f;
        yield return new WaitForSeconds(delay);
        
        if (lastHighScore < score)
        {
            yield return StartCoroutine(SetHighScore(score));
        }
        
       
        //animation achievements

        AdmobAdsManager.Instance.ShowInterstitialAd();
    }

    public void UpdateAchievementAndUnlockedLevels()
    {
        foreach (var stageIndexAndData in StageManager.Instance.stages) {
            stageIndexAndData.Value.UpdateDifficultyUnlockedAndAmountOfStickersUnlocked();
        }
    }

    private void UpdateInventoryAndSave() {
        
        // Desactivado que el update de los consumibles se haga al final de las partidas para que se haga en cada turno
        // ya que no hay consumibles gratis por upgrade, si no que se generan cada x tiempo en factory
        
        // foreach (ConsumableID consumable in matchInventory.Keys)
        // {
        //     int result = matchInventory[consumable].initial.consumableValue - matchInventory[consumable].current;
        //     if (result > 0)
        //         userConsumableData.ModifyConsumable(consumable, -result);
        // }
        
        PersistanceManager.Instance.SaveUserConsumableData();
        PersistanceManager.Instance.SaveUserData();
    }
    private IEnumerator SetHighScore(int highScoreToSet)
    {
        float clipDuration = AudioManager.Instance.PlayClip(GameClip.highScore);
        float timeUntillHighScoreTextUpdate = 0.25f;
        StageManager.Instance.stages[(selectedLevel,selectedDifficulty)].SetScore(highScoreToSet);
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
        StartCoroutine(ProcessTurnAction(0, true));
    }

    public void PlayNextStage() {
        if (selectedLevel < 13) {
            StageManager.Instance.SetStageAndDifficulty(selectedLevel + 1, selectedDifficulty);
        }
        else if (selectedDifficulty<9) {
            StageManager.Instance.SetStageAndDifficulty(0, selectedDifficulty+1);
        }
        else {
            throw new Exception("Play Next stage called but there is no next stage");
        }
        Reset();
    }

    public void Reset() {
        CustomDebugger.Log("Amount of clues "+userConsumableData.GetConsumableData(ConsumableID.Clue).amount);
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
        
        pause = true;
        startMatchTime = Time.time;
        endMatchTime = 0;
        AdmobAdsManager.Instance.LoadInterstitialAd();
        SetScoreTexts();
        if (stickerDisplay == null) {
            stickerDisplay = StickerManager.Instance.GetStickerHolder();
        }
        startScale = stickerDisplay.spriteHolder.transform.parent.transform.localScale;
        stickerDisplay.ConfigureForGame(currentGameMode);
        SetMatchInventory();
        protectedLife = userData.GetUpgradeLevel(UpgradeID.LifeProtector) > 0; 
        deathDefyCharges = userData.GetUpgradeLevel(UpgradeID.DeathDefy); 
        gameCanvas.numpad.SetNumpadByDifficulty(selectedDifficulty);
        gameEnded = false;
        stickerDisplay.gameObject.SetActive(true);
        endGameButtons.transform.parent.transform.parent.gameObject.SetActive(false);
        if (AudioManager.Instance.playMusic) AudioManager.Instance.audioSourceMusic.Play();
        ResetTimer();
        SetScore(0);
        turnNumber = 1;
        SetCurrentCombo(0);
        bonusMultiplicator = 1;
        if (perfectMode)
        {
            lifeCounter.ResetLivesPerfectMode();
        }
        else
        {
            lifeCounter.ResetLives();
        }

        StageManager.Instance.AssignRandomStickerGroup();
        LoadStickers();
        gameCanvas.barController.SetBar(_remainingStickersFromStage.Count);
        currentlyInGameStickers = new Dictionary<StickerData, StickerMatchData>();
        AddStickers(startingStickerAmount);
        _currentlySelectedSticker = null;
        SetRandomImage();
        disableInput = false;
        _currentMatch = new Match(selectedLevel,selectedDifficulty,false);
        _currentCombo = 0;
        endGameAchievementStars.ResetStars();
        gameCanvas.UpdateUI(true);
        
        pausePanel.SetActive(false);
        tutorialPanel.SetActive(false);

        #region FirstMistake
        /*
        if (userData.stages[0].matches.Count == 0)
        {
            //OpenTutorialPanel(0);
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
        */
        #endregion

        stageGroupIntroPanel.gameObject.SetActive(true);
        stageGroupIntroPanel.SetGroup(StageManager.Instance.selectedStickerGroup);
        StartCoroutine(StartMatchCountdown());
    }

    private IEnumerator StartMatchCountdown() {
        for (int i = 3; i > 0; i--) {
            stageGroupIntroPanel.SetCountdown(i);
            AudioManager.Instance.PlayClip(GameClip.enterStages);
            yield return new WaitForSeconds(1f);
        }
        stageGroupIntroPanel.gameObject.SetActive(false);
        pause = false;
    }

    private void ResetTimer()
    {
        if (speedrunMode)
        {
            SetTimer(maxTimerSpeedrun);
        }
        else
        {
            SetTimer(maxTimer);
        }
    }


    private void FixedUpdate() {
        if (gameEnded || pause || disableInput) return;
        SetTimer(timer - Time.deltaTime);
    }

    private void Update()
    {
    #if UNITY_EDITOR
     currentlyInGameStickersTotalAmount = currentlyInGameStickers.Count;
     currentlyInGameStickersNames = new string[currentlyInGameStickers.Count];
     int stickerOrderNumber = 0;
     foreach (var currentSticker in currentlyInGameStickers)
     {
         currentlyInGameStickersNames[stickerOrderNumber] = currentSticker.Key.name;
         stickerOrderNumber++;
     }
     if (_currentlySelectedSticker is not null)
     {
        currentlySelectedStickerID = _currentlySelectedSticker.stickerID;
        currentlySelectedStickerName = _currentlySelectedSticker.name;
        if (currentlyInGameStickers.ContainsKey(_currentlySelectedSticker))
        {
            currentlySelectedStickerAmountOfAppearences = currentlyInGameStickers[_currentlySelectedSticker].amountOfAppearences;
        }
     }
     
     
     for (int i = 0; i <= 9; i++)
    {
        if (Input.GetKeyDown(i.ToString()))
        {
            CustomDebugger.Log("Number key " + i + " pressed");
             if (gameEnded || disableInput) {
                 return;
             }
             CustomDebugger.Log("Clicked number "+ i);
             StartCoroutine(ProcessTurnAction(i));
        }
    }

    // Check for numpad keys
    for (KeyCode key = KeyCode.Keypad1; key <= KeyCode.Keypad9; key++)
    {
        if (Input.GetKeyDown(key))
        {
            CustomDebugger.Log("Numpad key " + (key - KeyCode.Keypad0) + " pressed kp");
            if (gameEnded || disableInput) {
                return;
            }
            CustomDebugger.Log("Clicked number "+ (key - KeyCode.Keypad0));
            StartCoroutine(ProcessTurnAction((key - KeyCode.Keypad0)));
        }
    }
    #endif
        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            CustomDebugger.Log("Numpad key " + (KeyCode.KeypadEnter) + " pressed kp");
            if (gameEnded || disableInput) {
                return;
            }
            CustomDebugger.Log("Clicked number "+ (KeyCode.KeypadEnter));
            StartCoroutine(ProcessTurnAction(currentlyInGameStickers[_currentlySelectedSticker].amountOfAppearences));
        }
        if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            CustomDebugger.Log("Numpad key " + (KeyCode.Keypad0) + " pressed kp");
            if (gameEnded || disableInput) {
                return;
            }
            CustomDebugger.Log("Clicked number "+ (KeyCode.Keypad0));
            StartCoroutine(ProcessTurnAction((0)));
        }
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
        int calculatedComboBonus = (int)Math.Floor(((float)_currentCombo / 2));
        if (calculatedComboBonus >= maxComboBonus)
        {
            calculatedComboBonus = maxComboBonus;
        }
        return calculatedComboBonus;
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
    Pokemon,
    Landscapes,
    AnatomyFractures,
    AnatomyBones,
    WorldFlags
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