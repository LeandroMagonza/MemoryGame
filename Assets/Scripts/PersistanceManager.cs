using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;


public class PersistanceManager : MonoBehaviour
{
    #region Singleton

    private static PersistanceManager _instance;

    public static PersistanceManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<PersistanceManager>();
            if (_instance == null)
                Debug.LogError("Singleton<" + typeof(PersistanceManager) + "> instance has been not found.");
            return _instance;
        }
    }

    protected void Awake()
    {
        if (_instance == null)
        {
            _instance = this as PersistanceManager;
        }
        else if (_instance != this)
            DestroySelf();
    }

    private void DestroySelf()
    {
        if (Application.isPlaying)
            Destroy(this);
        else
            DestroyImmediate(this);
    }

    #endregion
    public DataLocation dataLocation = DataLocation.ResourcesFolder;
    
    //public Dictionary<int, StageData> stages;
    private Dictionary<int, StickerLevelsData> stickerLevels = new Dictionary<int, StickerLevelsData>();
    private Dictionary<string, string> loadedFiles = new Dictionary<string, string>();

    public Dictionary<int, StickerLevelsData> StickerLevels
    {
        get { return stickerLevels; }
        private set { stickerLevels = value; }
    }
    
    public PacksData packs = new PacksData();
    public UserData userData;
    private ConfigData configData;
    public UserConsumableData userConsumableData;
    private int eraseCounter = 3;
    
    //private int stagesVersion = 0;
    private readonly string fileHostUrl = "https://leandromagonza.github.io/MemoGram/";


    [System.Serializable]
    private class LanguagesList {
        public int version = 0;
        public List<string> languages;
    }
        /*
    #if UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern void _JS_FileSystem_Sync();
    #endif
    */
    void Start() {
        #if UNITY_WEBGL
        dataLocation = DataLocation.ResourcesFolder;
        #endif
        #if UNITY_ANDROID
        dataLocation = DataLocation.CloudAndLocalStorage;
        #endif
        StartCoroutine(LoadDataAndSetup());
    }

    public IEnumerator LoadDataAndSetup() {
        
        yield return StartCoroutine(LoadUIMenuConfiguration());
        yield return StartCoroutine(LoadUserData());
        // LOAD UI TIENE QUE LLAMARSE PRIMERO PORQUE AHI VIENE LA VERSION DEL STAGES
        // SE FIJA SI HAY UNO MAS NUEVO Y LO REEMPLAZA
        
        yield return StartCoroutine(LocalizationManager.Instance.InitializeLocalizationManager());
        yield return StartCoroutine(LoadStickerLevels());
        yield return StartCoroutine(LoadPacks());

        // initialize tiene que ir dsp de loaduserdata porque usa el idioma de userdata para saber cual cargar
        ;
        yield return StartCoroutine(LoadStages());
        yield return StartCoroutine(LoadUserConsumableData());
        PlayerLevelManager.Instance.UpdatePlayerLevelButtons();
        
        GameManager.Instance.CheckStreakStart();

        
        CanvasManager.Instance.ChangeCanvas(CanvasName.MENU);
        // loading screen ends
        yield return StartCoroutine(ConsumableFactoryManager.Instance.GenerateAllConsumablesNextGenerationTimes());
    }

   private IEnumerator LoadUIMenuConfiguration()
{
    string setName = StageManager.Instance.gameVersion.ToString();

    yield return StartCoroutine(LoadFile(FileName.Stages,"json"));
    string json = GetLoadedFile(FileName.Stages);
    JObject jsonData = JObject.Parse(json);
    ConfigData currentConfig = jsonData["config"].ToObject<ConfigData>();

    
    yield return StartCoroutine(LoadFile(FileName.Stages,"json","",true));
    string latestJson = GetLoadedFile(FileName.Stages);
        Debug.Log(latestJson);
    JObject latestJsonData = JObject.Parse(latestJson);
    ConfigData latestConfig = latestJsonData["config"].ToObject<ConfigData>();

    // Inicializar finalConfig con la configuración actual
    ConfigData finalConfig = currentConfig;

    // Verificar si existe una última configuración y si su versión es más reciente
    if (latestConfig != null && latestConfig.version > currentConfig.version)
    {
        // Si la última configuración es más reciente, utilizar esa
        finalConfig = latestConfig;
    }

    configData = finalConfig;
    
    // Aplicar la configuración
    if (!string.IsNullOrEmpty(finalConfig.backgroundColor))
    {
        CanvasManager.Instance.SetMainMenuCanvasColor(finalConfig.backgroundColor);
    }
    else
    {
        CustomDebugger.LogError("No valid configuration found.");
    }

    // Opcional: reemplazar la configuración actual por la última si es más nueva
    if (finalConfig == latestConfig)
    {
        // Borrar todos los archivos que contengan additionalInfo en el nombre dentro de la carpeta con el mismo nombre que el setName
        string setFolderPath = Path.Combine(Application.persistentDataPath, setName);
        foreach (var file in Directory.GetFiles(setFolderPath, "additionalInfo*"))
        {
            File.Delete(file);
        }
    }

    PlayerLevelManager.Instance.UpdatePlayerLevelButtons();
    yield return null;
}




    private void OnApplicationQuit()
    {
        //SaveStages(stages);
        SaveUserData();
        SaveUserConsumableData();
    }

    /*public void SaveStages(Dictionary<int, StageData> stagesToSave)
    {
        List<StageData> stageList = new List<StageData>(stagesToSave.Values);
        string json = JsonConvert.SerializeObject(stageList, Formatting.Indented);
        string setName = StageManager.Instance.gameVersion.ToString();
        string filePath = Path.Combine(Application.persistentDataPath, setName, "stages.json");
        File.WriteAllText(filePath, json);
        CustomDebugger.Log("Stages saved to " + filePath);
    }*/

    public IEnumerator LoadStages()
    {
        yield return null;
        StageManager.Instance.InitializeStages();
    }


    public void SaveUserData()
    {
        if (string.IsNullOrEmpty(userData.language))
        {
            CustomDebugger.LogError("Language was empty, user data save aborted");
            return;
        }
        userData.ConvertStickerDictionaryToList();
        string setName = StageManager.Instance.gameVersion.ToString();
        string filePath = Path.Combine(Application.persistentDataPath, setName, FileName.UserData+".json");
        
        // Utilizar SerializeUserData para serializar los datos
        string json = SerializeUserData();
    
        File.WriteAllText(filePath, json);
        CustomDebugger.Log("UserData saved to " + filePath);
    }

    public IEnumerator LoadUserData()
    {
        yield return StartCoroutine(LoadFile(FileName.UserData,"json"));
        string json = GetLoadedFile(FileName.UserData);

        // Utilizar DeserializeUserData para deserializar los datos
 
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogError("Failed to load UserData.");
            userData = new UserData();
        }
        else
        {
            userData = DeserializeUserData(json);
            userData.ConvertStickerListToDictionary();
        }
        CustomDebugger.Log("UserData language: " + userData.language);

        #region MigrateFromCoinsVersion
        // un lv es un upgrade
        //si los puntos de exp que tiene corresponden a un lv menor que la cantidad de upgrades que tiene
        //hubo un error, o viene de la version anterior donde en vez de tener exp tenias monedas
        //entonces seteamos su exp a la cantidad de exp que corresponde al lv x la cant de upgrades, mas la cantidad de monedas restantes
        int levelAccordingToExp = userData.ExperiencePointsToLevelUp().calculatedLv;
        int levelAccordingToUpgrades = userData.GetAmountOfUpgrades();
        //si esta condicion es verdadera, el usuario tiene un userdata que es anterior al cambio de monedas a lv
        if (levelAccordingToUpgrades > levelAccordingToExp) {
            userData.experiencePoints = userData.ExperienceAccordingToLevel(levelAccordingToUpgrades) + userData.coins;
        }
        #endregion

        #region MigrateFromMatchesInUserDataVersion
        MigrateMatchesFromOldFormat(json);
        #endregion
        
        yield return null;
    }
    private void MigrateMatchesFromOldFormat(string json)
    {
        if (string.IsNullOrEmpty(json)) return;
        // Utiliza JsonConvert para deserializar el JSON a un JObject para un mejor manejo de las claves y valores
        var oldUserData = JsonConvert.DeserializeObject<JObject>(json);
    
        // Verifica si la clave "stages" existe y no es null
        if (oldUserData.TryGetValue("stages", out JToken stagesToken) && stagesToken.Type != JTokenType.Null)
        {
            var stages = stagesToken as JArray;

            foreach (var stageToken in stages)
            {
                var stage = stageToken as JObject;

                // Verifica si la clave "matches" existe y no es null
                if (stage.TryGetValue("matches", out JToken matchesToken) && matchesToken.Type != JTokenType.Null)
                {
                    var matches = matchesToken as JArray;

                    foreach (var matchToken in matches)
                    {
                        string matchJson = matchToken.ToString();
                        Match matchData = JsonConvert.DeserializeObject<Match>(matchJson);
                        SaveMatch(matchData);
                    }

                    // Elimina la clave "matches" del stage después de migrar los datos
                    stage.Remove("matches");
                }
            }
        }
    }

    public string SerializeUserData()
    {
        foreach (var stageData in userData.stages)
        {
            stageData.achievementsUnparsed = stageData.achievements
                .Select(a => a.ToString())
                .ToList();
        }
        

        return JsonConvert.SerializeObject(userData);
    } 

    public UserData DeserializeUserData(string json)
    {
        var userData = JsonConvert.DeserializeObject<UserData>(json);

        // Convertir strings a enums después de deserializar
        foreach (var stageData in userData.stages)
        {
            stageData.achievements = stageData.achievementsUnparsed
                .Select(a => Enum.Parse(typeof(Achievement), a))
                .Cast<Achievement>()
                .ToList();
        }

        return userData;
    }


    public IEnumerator LoadStickerLevels()
    {
        string setName = StageManager.Instance.gameVersion.ToString();
        string filePath = Path.Combine(Application.persistentDataPath, setName, FileName.Stages+".json");
        
        yield return StartCoroutine(LoadFile(FileName.Stages,"json"));
        string json = GetLoadedFile(FileName.Stages);

        // Parse the JSON into a JObject
        JObject jsonData = JObject.Parse(json);

        // Deserialize the stickerLevels data
        JObject stickerLevelsJson = jsonData["stickerLevels"].ToObject<JObject>();
        stickerLevels = new Dictionary<int, StickerLevelsData>();
        foreach (var item in stickerLevelsJson)
        {
            int level = int.Parse(item.Key);
            StickerLevelsData data = item.Value.ToObject<StickerLevelsData>();
            stickerLevels.Add(level, data);
        }

        CustomDebugger.Log("StickerLevels loaded from " + filePath + " levels: " + stickerLevels.Count);

        // Do whatever you need with the stickerLevels dictionary

        yield return null;
    }

    // Función para cargar los datos de packs
    public IEnumerator LoadPacks()
    {
        string setName = StageManager.Instance.gameVersion.ToString();
        string filePath = Path.Combine(Application.persistentDataPath, setName, FileName.Stages+".json");
        string json;
        
        yield return StartCoroutine(LoadFile(FileName.Stages,"json"));
        json = GetLoadedFile(FileName.Stages);

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

        CustomDebugger.Log("Packs loaded from " + filePath);
        CustomDebugger.Log("stickersPerPack on load: " + packs.stickersPerPack);
        yield return null;
    }

    public string GetLoadedFile(FileName fileName) {
        if (!loadedFiles.ContainsKey(fileName.ToString()))
        {
            CustomDebugger.LogError("No " + fileName.ToString() + " found in loadedFiles");
            return null;
        }
        return loadedFiles[fileName.ToString()];
    }    
    public string GetLoadedFile(string fileName) {
        if (!loadedFiles.ContainsKey(fileName))
        {
            CustomDebugger.LogError("No " + fileName + " found in loadedFiles");
            return null;
        }
        return loadedFiles[fileName];
    }

    public IEnumerator LoadFile(string file_name, string extension, string subfolder = "", bool forceReload = false) {
        if (subfolder == "") subfolder = StageManager.Instance.gameVersion.ToString();
            
        if (loadedFiles.ContainsKey(file_name) && !forceReload) yield break;

        string directoryPath = Path.Combine(Application.persistentDataPath, subfolder);
        if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);
        
        if (dataLocation == DataLocation.ResourcesFolder) {
            yield return StartCoroutine(LoadFromResources(file_name,subfolder));
        }
        else if (dataLocation == DataLocation.CloudAndLocalStorage) {
            yield return StartCoroutine(LoadFromCloudAndLocalStorage(file_name, extension, subfolder, forceReload));
        }
    }

    private IEnumerator LoadFromResources(string file_name, string subfolder) {
        string path = "Storage/" + subfolder + "/" + file_name;
        TextAsset file = Resources.Load<TextAsset>(path);
        if (file is null) {
            Debug.LogError("No " + file_name + " found in resources "+path);
            yield break;
        }
        loadedFiles[file_name] = file.text;
    }

    private IEnumerator LoadFromCloudAndLocalStorage(string file_name, string extension, string subfolder, bool forceReload) {
        
        string directoryPath = Path.Combine(Application.persistentDataPath, subfolder);
        if (IsInternetAvailable()) { // Check for internet connectivity
            string url = fileHostUrl + subfolder + "/" + file_name + "."+extension;
            string filePath = Path.Combine(directoryPath, file_name + "." + extension);
            if (File.Exists(filePath) && !forceReload) {
                string fileContents = File.ReadAllText(filePath);
                loadedFiles[file_name] = fileContents;
            }
            else {
                yield return StartCoroutine(DownloadFile(url, file_name));
                if (loadedFiles[file_name] is not null) File.WriteAllText(filePath, loadedFiles[file_name]);
            }
        } else {
            // Use the current files if no internet
            string filePath = Path.Combine(directoryPath, file_name + "." + extension);
            if (File.Exists(filePath)) {
                string fileContents = File.ReadAllText(filePath);
                loadedFiles[file_name] = fileContents;
            } else {
                // If no local file, try to load from resources
                yield return StartCoroutine(LoadFromResources(file_name,subfolder));
            }
        }
    }

    IEnumerator LoadFile(FileName file_name,string extension,string subfolder = "", bool forceReload = false) {
        //modificar a patron builder si se siguen agregando parametros opcionales
        yield return StartCoroutine(LoadFile(file_name.ToString(), extension, subfolder, forceReload));
    }

    public void OnEraseDataButtonPressed() {
        eraseCounter--;
        if (eraseCounter<=0) {
            eraseCounter = 3;
            StartCoroutine(ResetUserData());
        }
    }

    public IEnumerator ResetUserData() {
        //TODO: Agregar chequeo/confirm
        yield return null;
        userData = new UserData();
    }

    public int GetStickerDuplicates(StickerSet stickerSet,int stickerID)
    {
        CustomDebugger.Log(stickerSet+" sticker id "+stickerID,DebugCategory.STICKERLOAD_AMOUNTOFCATEGORIES);
        if (userData.stickerDuplicates.ContainsKey((stickerSet,stickerID)))
        {
            return userData.stickerDuplicates[(stickerSet, stickerID)];
        }

        return 0;
    }
    
    public int GetUpgradeLevel(UpgradeID upgrade) {
        if (userData.unlockedUpgrades.ContainsKey(upgrade))
        {
            return userData.unlockedUpgrades[upgrade];
        }
        return 0; 
    }    
    public int GetAmountOfConsumables(ConsumableID consumable) {
        return userConsumableData.GetConsumableEntry(consumable).amount;
    }
    public IEnumerator LoadUserConsumableData()
    {
        yield return StartCoroutine(LoadFile(FileName.UserConsumableData,"json"));
        string json = GetLoadedFile(FileName.UserConsumableData);

        // Deserializar los datos JSON a un objeto UserConsumableData
        if (!string.IsNullOrEmpty(json)) {
            userConsumableData = JsonConvert.DeserializeObject<UserConsumableData>(json);
        }
        else {
            userConsumableData = new UserConsumableData();
        }
        if (userConsumableData != null)
        {
            Debug.Log("User consumable data loaded successfully.");
        }
        else
        {
            Debug.LogError("Failed to load user consumable data.");
        }

        yield return StartCoroutine(ConsumableFactoryManager.Instance.GenerateAllConsumablesNextGenerationTimes());
        yield return null;
    }

    public void SaveUserConsumableData() {
            CustomDebugger.Log("Cut next generation times beggining saving:"+userConsumableData.GetNextGenerationTimes(ConsumableID.Cut).Count);
            string setName = StageManager.Instance.gameVersion.ToString();
            string filePath = Path.Combine(Application.persistentDataPath, setName, FileName.UserConsumableData+".json");
        
            // Utilizar SerializeUserData para serializar los datos
            CustomDebugger.Log("Cut next generation times before saving:"+userConsumableData.GetNextGenerationTimes(ConsumableID.Cut).Count);
            string json = JsonConvert.SerializeObject(userConsumableData);
            CustomDebugger.Log("SaveUserConsumableData :"+json);
            File.WriteAllText(filePath, json);
            
            CustomDebugger.Log("UserConsumableData saved to " + filePath+ " Cuts: "+userConsumableData.GetConsumableEntry(ConsumableID.Cut).amount);
    }
    
    public IEnumerator LoadLanguagesList() {
        

        yield return StartCoroutine(LoadFile(FileName.Languages,"json", "Languages"));
        string localJson = GetLoadedFile(FileName.Languages);
        LanguagesList localLanguagesList = JsonUtility.FromJson<LanguagesList>(localJson);
        yield return StartCoroutine(LoadFile(FileName.Languages,"json", "Languages",true));
        string downloadedJson = GetLoadedFile(FileName.Languages);
        LanguagesList downloadedLanguagesList = JsonUtility.FromJson<LanguagesList>(downloadedJson);

        if (downloadedLanguagesList.version > localLanguagesList.version) {
            localLanguagesList = downloadedLanguagesList;
            foreach (string language in localLanguagesList.languages)
            {
                string directoryPath = Path.Combine(Application.persistentDataPath, "Languages");
                string languageFilePath = Path.Combine(directoryPath, language + ".json");
                if (File.Exists(languageFilePath))
                {
                    File.Delete(languageFilePath);
                }
            }
        }

        LocalizationManager.Instance.languagesList = localLanguagesList.languages;
    }


    public IEnumerator LoadLanguageIcon(string language) {
        string iconPath = Path.Combine(Application.persistentDataPath, "Languages", $"{language}_icon.png");

        if (!File.Exists(iconPath) && dataLocation == DataLocation.CloudAndLocalStorage)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(fileHostUrl + $"Languages/{language}_icon.png"))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    File.WriteAllBytes(iconPath, webRequest.downloadHandler.data);
                }
                else
                {
                    Debug.LogError($"Failed to download {language}_icon.png");
                    yield break;
                }
            }
        }
    }
    public IEnumerator DownloadFile(string path, string file_name) {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(path))
        {
            CustomDebugger.Log("Attempting to download file from");
            yield return webRequest.SendWebRequest();
            string text;
            if (webRequest.result == UnityWebRequest.Result.Success) {
                text = webRequest.downloadHandler.text;
            }
            else
            {
                text = null;
                Debug.LogError("Failed to download file from: " + path);
            }
            if (!loadedFiles.ContainsKey(file_name)) {
                loadedFiles.Add(file_name,text);
            }
            else {
                loadedFiles[file_name] = text;
            }
        }
    }

    public int GetBaseExperiencePoints() {
        if (configData == null) return 0;
        return configData.baseExperiencePoints;
    }

    public float GetExperienceIncreasePerLevel() {
        if (configData == null) return 0f;
        return configData.experienceIncreasePerLevel;
    } 
    public int GetFlatAmountOfStickerAdded() {
        if (configData == null) return 0;
        return configData.flatAmountOfStickerAdded;
    } 
    public float GetScalingAmountOfStickerAdded() {
        if (configData == null) return 1f;
        return configData.scalingAmountOfStickerAdded;
    }
    public void SaveMatch(Match match)
    {
        string matchesFilePath = GetMatchesFilePath(match.stage, match.difficulty);
        using (StreamWriter writer = new StreamWriter(matchesFilePath, true))
        {
            string json = JsonConvert.SerializeObject(match);
            writer.WriteLine(json);
        }
    }

    public List<Match> LoadMatches(int level, int difficulty)
    {
        List<Match> matches = new List<Match>();
        string matchesFilePath = GetMatchesFilePath(level, difficulty);
        if (File.Exists(matchesFilePath))
        {
            using (StreamReader reader = new StreamReader(matchesFilePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    Match match = JsonConvert.DeserializeObject<Match>(line);
                    matches.Add(match);
                }
            }
        }
        return matches;
    }

    private string GetMatchesFilePath(int level, int difficulty)
    {
        string setName = StageManager.Instance.gameVersion.ToString();
        string fileName = $"matches_{level}_{difficulty}.txt";
        return Path.Combine(Application.persistentDataPath, setName, fileName);
    }

    private bool IsInternetAvailable() {
    using (UnityWebRequest webRequest = UnityWebRequest.Get("https://www.google.com")) {
        var operation = webRequest.SendWebRequest();
        while (!operation.isDone) { }
        return webRequest.result == UnityWebRequest.Result.Success;
        }
    }
}

   
public class StickerSetConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(StickerSet);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        string enumString = (string)reader.Value;
        
        // Aquí puedes manejar casos especiales o conversiones de nombres si es necesario
        if (Enum.TryParse<StickerSet>(enumString, out var result))
        {
            return result;
        }
        
        throw new JsonSerializationException($"Error al convertir el valor '{enumString}' a StickerSet enum.");
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        // Implementa esto si también necesitas escribir el JSON desde el enum
        string enumString = value.ToString();
        writer.WriteValue(enumString);
    }
    

}
public enum DataLocation
{
    CloudAndLocalStorage,
    ResourcesFolder
}

public class ConfigData {
    public int version = 0; // Valor predeterminado 0 para versiones sin este campo
    public int baseExperiencePoints = 1000;
    public float experienceIncreasePerLevel = 1.2f;
    public string backgroundColor;
    public int flatAmountOfStickerAdded = 0;
    public float scalingAmountOfStickerAdded = 1;
}
public enum FileName {
    UserData,
    Stages,
    /*Matches*/
    UserConsumableData,
    Languages
}
[Serializable]
public class UserConsumableEntry
{
    public int amount; // Cantidad de consumibles que el jugador tiene
    [JsonProperty]
    private List<(DateTime scheduledTime, int notificationId)> nextGenerationTimes = new (); // Lista de tiempos para generar próximos consumibles

    public UserConsumableEntry(int initialAmount, List<(DateTime scheduledTime, int notificationId)> nextGenerationTimes)
    {
        amount = initialAmount;
        if (nextGenerationTimes is not null) {
            this.nextGenerationTimes = new List<(DateTime scheduledTime, int notificationId)>(nextGenerationTimes);
        }
    }

    public List<(DateTime scheduledTime, int notificationId)> GetGenerationTimes() {
        //CustomDebugger.Log("NextGenerationTimes: "+nextGenerationTimes.Count);
        return nextGenerationTimes;
    }

    public void ModifyConsumable(int amount, List<(DateTime scheduledTime, int notificationId)> nextGenerationTimes) {
        this.amount += amount;
        if (nextGenerationTimes != null) {
            this.nextGenerationTimes = nextGenerationTimes;
        }
    }
    public int GetGeneratedAmount(bool delete) {

        //CustomDebugger.Log("Called getgeneratedamount with delete:" + delete);

        int generatedAmount = 0;
        int index = 0;
        foreach (var VARIABLE in nextGenerationTimes.ToList()) { 
            if (VARIABLE.scheduledTime < DateTime.Now) {
                generatedAmount++;
                if (delete) {
                    nextGenerationTimes.Remove(VARIABLE);
                }
            }
            index++;
        }
        return generatedAmount;
    }
}

[Serializable]
public class UserConsumableData
{
    // Usa JsonProperty para serializar el diccionario de consumables
    [JsonProperty]
    private Dictionary<ConsumableID, UserConsumableEntry> consumables = new Dictionary<ConsumableID, UserConsumableEntry>();

    public void ModifyConsumable(ConsumableID type, int amount, List<(DateTime scheduledTime, int notificationId)> nextGenerationTimes = null)
    {
        CustomDebugger.Log("ModifyConsumable type:"+type+" amount: "+amount+" nextgenerationtimes: "+
                           ((nextGenerationTimes != null)?nextGenerationTimes.Count.ToString():"null"));
        
        var consumableEntry = GetConsumableEntry(type);
        consumableEntry.ModifyConsumable(amount,nextGenerationTimes);

    }

    public UserConsumableEntry GetConsumableEntry(ConsumableID type)
    {
        //CustomDebugger.Log("get consumable entry for type:"+type);
        if (consumables.ContainsKey(type))
        {
            //CustomDebugger.Log("found type:"+type+" generationTimes"+consumables[type].GetGenerationTimes().Count);
            
            return consumables[type];
        }
        else {
            //CustomDebugger.Log("created type:"+type);
            var newUserConsumableEntry = new UserConsumableEntry(0, new List<(DateTime scheduledTime, int notificationId)>());
            consumables.Add(type, newUserConsumableEntry);
            return newUserConsumableEntry;
        }
    }

    public void RemovePastGenerationsFromAllConsumables()
    {
        foreach (ConsumableID type in Enum.GetValues(typeof(ConsumableID)))
        {
            if (type == ConsumableID.NONE)
                continue;

            var entry = GetConsumableEntry(type);
            int generatedCount = entry.GetGeneratedAmount(true);
            Debug.Log($"Consumible {type}: {generatedCount} tiempos de generación pasados eliminados.");
        }
    }

    public List<(DateTime scheduledTime, int notificationId)> GetNextGenerationTimes(ConsumableID type)
    {
        var entry = GetConsumableEntry(type);
        //CustomDebugger.Log("getting next generation times from: "+type);
        return entry.GetGenerationTimes();
    }
}

