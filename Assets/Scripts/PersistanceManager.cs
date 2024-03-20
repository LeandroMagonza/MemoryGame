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
    
    public Dictionary<int, StageData> stages;
    private Dictionary<int, StickerLevelsData> stickerLevels = new Dictionary<int, StickerLevelsData>();

    public Dictionary<int, StickerLevelsData> StickerLevels
    {
        get { return stickerLevels; }
        private set { stickerLevels = value; }
    }
    
    public PacksData packs = new PacksData();
    public UserData userData;
    private int eraseCounter = 3;

    void Start() {
        StartCoroutine(LoadDataAndSetup());
    }

    public IEnumerator LoadDataAndSetup() {
        yield return StartCoroutine(LoadUserData());
        yield return StartCoroutine(LoadStages());
        yield return StartCoroutine(LoadStickerLevels());
        yield return StartCoroutine(LoadPacks());
        yield return StartCoroutine(LoadUIMenuConfiguration());


    }

    private IEnumerator LoadUIMenuConfiguration()
    {
        string setName = StageManager.Instance.gameVersion.ToString();
        string filePath = Path.Combine(Application.persistentDataPath, setName, "stages.json");
        string json;
        if (!File.Exists(filePath))
        {
            CustomDebugger.Log("No saved stages found at " + filePath);
            yield return StartCoroutine(GetJson("stages"));
        }

        CustomDebugger.Log(filePath);
        json = File.ReadAllText(filePath);

        // Parse the JSON into a JObject
        JObject jsonData = JObject.Parse(json);

        if (!jsonData.ContainsKey("config"))
        {
            CustomDebugger.Log("No config found");
            yield break;
        }
        JObject mainMenuConfigJson = jsonData["config"].ToObject<JObject>();
        if (mainMenuConfigJson is JObject)
        {
            string hexColor = mainMenuConfigJson["backgroundColor"].Value<string>();
            CanvasManager.Instance.SetMainMenuCanvasColor(hexColor);
        }
        else
        {
            CustomDebugger.LogError("Not Set Configuration");
        }
        yield return null;
    }

    private void OnApplicationQuit()
    {
        //SaveStages(stages);
        SaveUserData();
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
        string setName = StageManager.Instance.gameVersion.ToString();
        string filePath = Path.Combine(Application.persistentDataPath, setName, "stages.json");
        string json;
        CustomDebugger.Log(filePath);
        if (dataLocation == DataLocation.LocalStorage)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError("No saved stages found at " + filePath);
                yield return StartCoroutine(GetJson("stages"));
            }
            json = File.ReadAllText(filePath);
        }
        else if (dataLocation == DataLocation.ResourcesFolder)
        {
            TextAsset file = Resources.Load<TextAsset>("Storage/"+setName+"/stages");
            if (file == null)
            {
                Debug.LogError("No stages found in Resources.");
                yield break;
            }
            json = file.text;
        }
        else
        {
            Debug.LogError("Invalid data location.");
            yield break;
        }

        Serialization<StageData> stageList = JsonConvert.DeserializeObject<Serialization<StageData>>(json);
        if (stageList == null || stageList.items == null)
        {
            Debug.LogError("Failed to deserialize stages.");
            yield break;
        }

        foreach (var stageData in stageList.items)
        {
            stageData.ConvertColorStringToColorValue();
        }

        this.stages = stageList.items.ToDictionary(stage => stage.stageID, stage => stage);
        CustomDebugger.Log("Stages loaded, stages count: " + stages.Count);
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
        string filePath = Path.Combine(Application.persistentDataPath, setName, "userData.json");
        
        // Utilizar SerializeUserData para serializar los datos
        string json = SerializeUserData();
    
        File.WriteAllText(filePath, json);
        CustomDebugger.Log("UserData saved to " + filePath);
    }

    public IEnumerator LoadUserData()
    {
        string setName = StageManager.Instance.gameVersion.ToString();
        string filePath = Path.Combine(Application.persistentDataPath, setName, "userData.json");
        string json;

        if (dataLocation == DataLocation.LocalStorage)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError("No userdata found at " + filePath);
                yield return StartCoroutine(GetJson("userData"));
            }
            json = File.ReadAllText(filePath);
        }
        else if (dataLocation == DataLocation.ResourcesFolder)
        {
            TextAsset file = Resources.Load<TextAsset>("Storage/" + setName + "/userData");
            if (file == null)
            {
                Debug.LogError("No userdata found in Resources.");
                yield break;
            }
            json = file.text;
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
//        userData.stickerDuplicates;
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
        string filePath = Path.Combine(Application.persistentDataPath, setName, "stages.json");
        string json;
        if (!File.Exists(filePath))
        {
            CustomDebugger.Log("No saved stages found at " + filePath);
            yield return StartCoroutine(GetJson("stages"));
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
            yield return StartCoroutine(GetJson("stages"));
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

    IEnumerator GetJson(string file_name)
    {
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
                string filePath = Path.Combine(directoryPath, file_name + ".json");
        
                // Escribe el texto en el archivo
                File.WriteAllText(filePath, www.downloadHandler.text);
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
        yield return StartCoroutine(GetJson("userData"));
        yield return StartCoroutine(LoadUserData());
    }
    public int GetStickerDuplicates(StickerSet stickerSet,int stickerID)
    {
        CustomDebugger.Log(stickerSet+" sticker id "+stickerID,DebugCategory.STICKERLOAD);
        if (userData.stickerDuplicates.ContainsKey((stickerSet,stickerID)))
        {
            return userData.stickerDuplicates[(stickerSet, stickerID)];
        }

        return 0;
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