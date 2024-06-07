using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance;

    private Dictionary<string, Dictionary<GameText, string>> languages = new ();
    private Dictionary<string, Sprite> languageIcons = new ();
    public string currentLanguage;
    public List<string> languagesList;
    public List<LocalizedText> localizedTexts = new();

    private UserData userData => PersistanceManager.Instance.userData;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public IEnumerator InitializeLocalizationManager()
    {
        CustomDebugger.Log("found language: "+userData.language);
        currentLanguage = userData.language;
        localizedTexts = FindObjectsOfType<LocalizedText>(true).ToList();
        yield return StartCoroutine(PersistanceManager.Instance.LoadLanguageList());
        foreach (var language in languagesList)
        {
            yield return StartCoroutine(PersistanceManager.Instance.LoadLanguageFiles(language));
        }

        PopulateLanguageIcons(languagesList);
        
        CanvasManager.Instance.GetCanvas(CanvasName.LANGUAGE).GetComponent<LanguageCanvas>().UpdateLanguageButtons();
        
        ChangeLanguage(userData.language);
    }

 
 private void PopulateLanguageIcons(List<string> languagesList)
    {
        foreach (var language in languagesList)
        {
            string iconPath = Path.Combine(Application.persistentDataPath, "Languages", $"{language}_icon.png");
            byte[] iconData = File.ReadAllBytes(iconPath);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(iconData);
            Sprite iconSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            languageIcons[language] = iconSprite;
        }
    }

    private void LoadLanguage(string language)
    {
        string languagePath = Path.Combine(Application.persistentDataPath, "Languages", $"{language}.json");

        if (File.Exists(languagePath))
        {
            string json = File.ReadAllText(languagePath);
            CustomDebugger.Log(language+" raw json  gameTexts:"+json+"");

            var texts = JsonConvert.DeserializeObject<Dictionary<GameText, string>>(json);

            languages[language] = texts;
            CustomDebugger.Log(language+" has "+texts.Count+" gameTexts");
            foreach (var VARIABLE in texts) {
                CustomDebugger.Log(VARIABLE.Key+"key - value " + VARIABLE.Value);
            }
        }
        else {
            Debug.LogError($"Language file not found: {language}.json");
        }
    }

    public void ChangeLanguage(string newLanguage)
    {
        if (!languages.ContainsKey(newLanguage))
        {
            languages.Add(newLanguage,new ());
            LoadLanguage(newLanguage);
        }
        currentLanguage = newLanguage;
        userData.language = newLanguage;

        StickerManager.Instance.LoadAllStickersFromSet(StageManager.Instance.gameVersion);

        CanvasManager.Instance.GetCanvas(CanvasName.LANGUAGE).GetComponent<LanguageCanvas>()
            .UpdateActiveLanguageButtonMarker(newLanguage);
        
        CanvasManager.Instance.GetCanvas(CanvasName.MENU).GetComponent<MenuCanvas>().languagesButton
            .GetComponent<Image>().sprite = languageIcons[currentLanguage];
        
        UpdateGameTexts();

    }

    private void UpdateGameTexts()
    {
        CustomDebugger.Log("Update game texts in "+currentLanguage+". Found texts:"+localizedTexts.Count);
        foreach (var localizedText in localizedTexts) {
            localizedText.UpdateText();
        }
    }

    public Sprite GetLanguageIcon(string language)
    {
        if (languageIcons.TryGetValue(language, out Sprite icon))
        {
            return icon;
        }
        else
        {
            Debug.LogError($"Icon not found for language: {language}");
            return null;
        }
    }

    public string GetGameText(GameText gameTextToGet, string language = "") {
        if (language == "") {
            language = currentLanguage;
        }
        if (!languages.ContainsKey(language)) {
            LoadLanguage(language);
        }
        if (languages[language].ContainsKey(gameTextToGet)) {
            return languages[language][gameTextToGet];
        }
        return "";
    }

    public void SetGameTitle(string language, string title) {
        languages[language][GameText.GameTitle] = title;
    }
}
public enum GameText
{
    GameTitle,
    Music,
    SoundEffects,
    UnlockedUpgrades,
    SelectUpgrade,
    HoursAbb,
    MinutesAbb,
    ItemClueName,
    ItemRemoveName,
    ItemBombName,
    ItemCutName,
    ItemPeekName,
    ItemHighlightName,
    ItemClueDescription,
    ItemRemoveDescription,
    ItemBombDescription,
    ItemCutDescription,
    ItemPeekDescription,
    ItemHighlightDescription,
    ItemEnergyPotionName,
    ItemEnergyPotionDescription,
    UpgradeFactoryDescription,
    UpgradeFactoryStorage,
    UpgradeFactoryClueName,
    UpgradeFactoryRemoveName,
    UpgradeFactoryPeekName,
    UpgradeFactoryHighlightName,
    UpgradeFactoryBombName,
    UpgradeExtraLifeName,
    UpgradeLifeProtectorName,
    UpgradeBetterClueName,
    UpgradeHealOnClearName,
    UpgradeFactoryCutName,
    UpgradeBetterCutName,
    UpgradeBlockMistakeName,
    UpgradeDeathDefyName,
    UpgradeStickerMasterName,
    UpgradeConsumableSlotName,
    UpgradeExtraLifeDescription,
    UpgradeLifeProtectorDescription,
    UpgradeBetterClueDescription,
    UpgradeHealOnClearDescription,
    UpgradeFactoryCutDescription,
    UpgradeBetterCutDescription,
    UpgradeBlockMistakeDescription,
    UpgradeDeathDefyDescription,
    UpgradeStickerMasterDescription,
    UpgradeConsumableSlotDescription,
    
    
    

}