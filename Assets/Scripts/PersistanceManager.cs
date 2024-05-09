using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    private Dictionary<FileName, string> loadedFiles = new Dictionary<FileName, string>();

    public Dictionary<int, StickerLevelsData> StickerLevels
    {
        get { return stickerLevels; }
        private set { stickerLevels = value; }
    }
    
    public PacksData packs = new PacksData();
    public UserData userData;
    public UserConsumableData userConsumableData;
    private int eraseCounter = 3;
    
    private int stagesVersion = 0;

    void Start() {
        StartCoroutine(LoadDataAndSetup());
    }

    public IEnumerator LoadDataAndSetup() {
        yield return StartCoroutine(LoadUserData());
        // LOAD UI TIENE QUE LLAMARSE PRIMERO PORQUE AHI VIENE LA VERSION DEL STAGES
        // SE FIJA SI HAY UNO MAS NUEVO Y LO REEMPLAZA
        yield return StartCoroutine(LoadUIMenuConfiguration());
        yield return StartCoroutine(LoadStages());
        yield return StartCoroutine(LoadStickerLevels());
        yield return StartCoroutine(LoadPacks());
        yield return StartCoroutine(LoadUserConsumableData());


    }

    private IEnumerator LoadUIMenuConfiguration()
    {
        string setName = StageManager.Instance.gameVersion.ToString();
        string filePath = Path.Combine(Application.persistentDataPath, setName, FileName.Stages+".json");
        string latestFilePath = Path.Combine(Application.persistentDataPath, setName, "latestStages.json");
        string json;

        if (!File.Exists(filePath))
        {
            CustomDebugger.Log("No saved stages found at " + filePath);
            yield return StartCoroutine(GetJson(FileName.Stages));
        }
        else
        {
            yield return StartCoroutine(GetJson(FileName.Stages, "latestStages"));
        }

        // Cargar la configuración actual
        json = File.ReadAllText(filePath);
        JObject jsonData = JObject.Parse(json);
        ConfigData currentConfig = jsonData["config"].ToObject<ConfigData>();

        // Cargar la última configuración, si está disponible
        ConfigData latestConfig = null;
        if (File.Exists(latestFilePath))
        {
            json = File.ReadAllText(latestFilePath);
            JObject latestJsonData = JObject.Parse(json);
            latestConfig = latestJsonData["config"].ToObject<ConfigData>();
        }

        // Inicializar finalConfig con la configuración actual
        ConfigData finalConfig = currentConfig;

        // Verificar si existe una última configuración y si su versión es más reciente
        if (latestConfig != null && latestConfig.version > currentConfig.version)
        {
            // Si la última configuración es más reciente, utilizar esa
            finalConfig = latestConfig;
        }


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
           File.Copy(latestFilePath, filePath, true);
        }

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
        // se comenta todo porque ya no hay stages para cargar, se generan de 4 hasta 12 niveles, y de 0 (2 botones) a 7 (9 botones)
        
        // string setName = StageManager.Instance.gameVersion.ToString();
        // string filePath = Path.Combine(Application.persistentDataPath, setName, "stages.json");
        // string json;
        // CustomDebugger.Log(filePath);
        // if (dataLocation == DataLocation.LocalStorage)
        // {
        //     if (!File.Exists(filePath))
        //     {
        //         Debug.LogError("No saved stages found at " + filePath);
        //         yield return StartCoroutine(GetJson("stages"));
        //     }
        //     json = File.ReadAllText(filePath);
        // }
        // else if (dataLocation == DataLocation.ResourcesFolder)
        // {
        //     TextAsset file = Resources.Load<TextAsset>("Storage/"+setName+"/stages");
        //     if (file == null)
        //     {
        //         Debug.LogError("No stages found in Resources.");
        //         yield break;
        //     }
        //     json = file.text;
        // }
        // else
        // {
        //     Debug.LogError("Invalid data location.");
        //     yield break;
        // }
        // //jsonData["stickerLevels"]        
        // Serialization<StageData> stageList = JsonConvert.DeserializeObject<Serialization<StageData>>(json);
        // if (stageList == null || stageList.items == null)
        // {
        //     Debug.LogError("Failed to deserialize stages.");
        //     yield break;
        // }
        //
        // foreach (var stageData in stageList.items)
        // {
        //     stageData.ConvertColorStringToColorValue();
        // }
        //
        // this.stages = stageList.items.ToDictionary(stage => stage.stageID, stage => stage);
        // CustomDebugger.Log("Stages loaded, stages count: " + stages.Count);
        yield return null;
        StageManager.Instance.InitializeStages();
    }


    public void SaveUserData()
    {
        /*if (userData.stickerDuplicates.Count == 0)
        {
            throw new Exception("Sticker duplicates was empty, user data save aborted");
        }*/
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
        string setName = StageManager.Instance.gameVersion.ToString();
        string filePath = Path.Combine(Application.persistentDataPath, setName, FileName.UserData+".json");
        string json;

        if (dataLocation == DataLocation.LocalStorage)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError("No userdata found at " + filePath);
                yield return StartCoroutine(GetJson(FileName.UserData));
            }
            json = File.ReadAllText(filePath);
        }
        else if (dataLocation == DataLocation.ResourcesFolder)
        {
            TextAsset file = Resources.Load<TextAsset>("Storage/" + setName + "/userData");
            if (file == null)
            {
                yield return StartCoroutine(GetJson(FileName.UserData));
                json = loadedFiles[FileName.UserData];
                Debug.LogError("No userdata found in Resources.");
            }
            else {
                json = file.text;
            }
        }
        else
        {
            Debug.LogError("Invalid data location.");
            yield break;
        }

        // Utilizar DeserializeUserData para deserializar los datos
        UserData userData = DeserializeUserData(json);
        if (userData != null)
        {
            userData.ConvertStickerListToDictionary();
            CustomDebugger.Log("UserData loaded, stages count: " + userData.stages.Count);
        }
        else
        {
            Debug.LogError("Failed to load UserData.");
        }

        this.userData = userData;
        yield return null;
    }

    public string SerializeUserData()
    {
        // Clonar userData para no modificar el original

        // Convertir enums a strings antes de serializar
        //userData.stickerDuplicates;
        foreach (var stageData in userData.stages)
        {
            //CustomDebugger.Log("stage "+stageData.stage+" has achievements: "+stageData.achievements.Count);
            //CustomDebugger.Log("stage "+stageData.stage+" has achievementsUnparsed: "+stageData.achievements.Count);
            stageData.achievementsUnparsed = stageData.achievements
                .Select(a => a.ToString())
                .ToList();
            //CustomDebugger.Log("stage "+stageData.stage+" has achievements: "+stageData.achievements.Count);
            //CustomDebugger.Log("stage "+stageData.stage+" has achievementsUnparsed: "+stageData.achievements.Count);
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
        string json;
        if (!File.Exists(filePath))
        {
            CustomDebugger.Log("No saved stages found at " + filePath);
            yield return StartCoroutine(GetJson(FileName.Stages));
        }

        CustomDebugger.Log(filePath);
        json = File.ReadAllText(filePath);

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
        string filePath = Path.Combine(Application.persistentDataPath, setName, "stages.json");
        string json;
        if (!File.Exists(filePath))
        {
            CustomDebugger.Log("No saved stages found at " + filePath);
            yield return StartCoroutine(GetJson(FileName.Stages));
        }

        CustomDebugger.Log(filePath);
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

        CustomDebugger.Log("Packs loaded from " + filePath);
        CustomDebugger.Log("stickersPerPack on load: " + packs.stickersPerPack);
        yield return null;
    }

    IEnumerator GetJson(FileName file_name,string save_name = "")
    {
        if (save_name == "") {
            save_name = file_name.ToString();
        }
        string setName = StageManager.Instance.gameVersion.ToString();
        string url = "https://leandromagonza.github.io/MemoGram/" + setName + "/" + file_name + ".json";

        // Construye la ruta completa del directorio donde se guardará el archivo
        string directoryPath = Path.Combine(Application.persistentDataPath, setName);

        // Verifica si el directorio existe; si no, créalo
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError ||
                www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(www.error + " at " + url);
            }
            else
            {
                // Construye la ruta completa del archivo, incluyendo el nombre del archivo
                string filePath = Path.Combine(directoryPath, save_name + ".json");
        
                // Escribe el texto en el archivo
                File.WriteAllText(filePath, www.downloadHandler.text);
                if (!loadedFiles.ContainsKey(file_name)) {
                    loadedFiles.Add(file_name,www.downloadHandler.text);
                }
                else {
                    loadedFiles[file_name] = www.downloadHandler.text;
                }
                Debug.Log("File saved to " + filePath);
            }
        }
        
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
        yield return StartCoroutine(GetJson(FileName.UserData));
        yield return StartCoroutine(LoadUserData());
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
        return userConsumableData.GetConsumableData(consumable).amount;
    }
    public IEnumerator LoadUserConsumableData()
    {
        string setName = StageManager.Instance.gameVersion.ToString();
        string filePath = Path.Combine(Application.persistentDataPath, setName, FileName.UserConsumableData+".json");
        string json = String.Empty;

        if (dataLocation == DataLocation.LocalStorage)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError("No consumable data found at " + filePath);
                yield return StartCoroutine(GetJson(FileName.UserConsumableData));
            }

            if (File.Exists(filePath)) {
                json = File.ReadAllText(filePath);
            }
        }
        else if (dataLocation == DataLocation.ResourcesFolder)
        {
            TextAsset file = Resources.Load<TextAsset>("Storage/" + setName + "/userConsumableData");
            if (file == null)
            {
                yield return StartCoroutine(GetJson(FileName.UserConsumableData));
                json = loadedFiles[FileName.UserConsumableData];
                Debug.LogError("No consumable data found in Resources.");
            }
            else
            {
                json = file.text;
            }
        }
        else
        {
            Debug.LogError("Invalid data location.");
            yield break;
        }

        // Deserializar los datos JSON a un objeto UserConsumableData
        if (json != string.Empty) {
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

        yield return null;
    }

    public void SaveUserConsumableData() {
            string setName = StageManager.Instance.gameVersion.ToString();
            string filePath = Path.Combine(Application.persistentDataPath, setName, FileName.UserConsumableData+".json");
        
            // Utilizar SerializeUserData para serializar los datos
            string json = JsonConvert.SerializeObject(userConsumableData);
            CustomDebugger.Log("SaveUserConsumableData :"+json);
            File.WriteAllText(filePath, json);
            
            CustomDebugger.Log("UserConsumableData saved to " + filePath+ " Clues: "+userConsumableData.GetConsumableData(ConsumableID.Clue).amount);
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
    LocalStorage,
    ResourcesFolder
}

public class ConfigData {
    public int version = 0; // Valor predeterminado 0 para versiones sin este campo
    public string backgroundColor;
}
public enum FileName {
    UserData,
    Stages,
    /*Matches*/
    UserConsumableData
}
[Serializable]
public class UserConsumableEntry
{
    public int amount; // Cantidad de consumibles que el jugador tiene
    public List<(DateTime scheduledTime, int notificationId)> nextGenerationTimes = new (); // Lista de tiempos para generar próximos consumibles

    public UserConsumableEntry(int initialAmount, List<(DateTime scheduledTime, int notificationId)> nextGenerationTimes)
    {
        amount = initialAmount;
        if (nextGenerationTimes is not null) {
            this.nextGenerationTimes = new List<(DateTime scheduledTime, int notificationId)>(nextGenerationTimes);
        }
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
        if (consumables.ContainsKey(type))
        {
            // Actualizar los valores existentes
            consumables[type].amount += amount;
            if (nextGenerationTimes is not null) {
                consumables[type].nextGenerationTimes = new List<(DateTime scheduledTime, int notificationId)>(nextGenerationTimes);
            }
            else {
                consumables[type].nextGenerationTimes = new List<(DateTime scheduledTime, int notificationId)>();
            }
        }
        else
        {
            // Añadir un nuevo tipo de consumible
            consumables[type] = new UserConsumableEntry(amount, nextGenerationTimes);
        }
    }

    public UserConsumableEntry GetConsumableData(ConsumableID type)
    {
        if (consumables.TryGetValue(type, out var entry))
        {
            return entry;
        }
        else {
            var newUserConsumableEntry = new UserConsumableEntry(0, new List<(DateTime scheduledTime, int notificationId)>());
            consumables.Add(type, newUserConsumableEntry);
            return newUserConsumableEntry;
        }
    }

    public int RemovePastGenerationTimes(ConsumableID type)
    {
        var entry = GetConsumableData(type);
        int removedCount = 0;

        if (entry != null)
        {
            removedCount = entry.nextGenerationTimes.RemoveAll(time => time.scheduledTime < DateTime.Now);
        }

        return removedCount;
    }

    public void RemovePastGenerationsFromAllConsumables()
    {
        foreach (ConsumableID type in Enum.GetValues(typeof(ConsumableID)))
        {
            if (type == ConsumableID.NONE)
                continue;

            int generatedCount = RemovePastGenerationTimes(type);
            Debug.Log($"Consumible {type}: {generatedCount} tiempos de generación pasados eliminados.");
        }
    }

    public List<(DateTime scheduledTime, int notificationId)> GetNextGenerationTimes(ConsumableID type)
    {
        var entry = GetConsumableData(type);
        return entry != null ? entry.nextGenerationTimes : new List<(DateTime scheduledTime, int notificationId)>();
    }
}

