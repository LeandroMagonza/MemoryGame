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

    public Dictionary<int, StageData> stages;
    private Dictionary<int, StickerLevelsData> stickerLevels = new Dictionary<int, StickerLevelsData>();

    public Dictionary<int, StickerLevelsData> StickerLevels
    {
        get { return stickerLevels; }
        private set { stickerLevels = value; }
    }
    
    public PacksData packs = new PacksData();
    public UserData userData;

    void Start()
    {
        StartCoroutine(LoadUserData());
    }
    private void OnApplicationQuit()
    {
        //SaveStages(stages);
        SaveUserData();
    }

    public void SaveStages(Dictionary<int, StageData> stagesToSave)
    {
        List<StageData> stageList = new List<StageData>(stagesToSave.Values);
        string json = JsonConvert.SerializeObject(stageList, Formatting.Indented);
        string setName = StageManager.Instance.gameVersion.ToString();
        string filePath = Path.Combine(Application.persistentDataPath, setName, "stages.json");
        File.WriteAllText(filePath, json);
        CustomDebugger.Log("Stages saved to " + filePath);
    }

    public IEnumerator LoadStages()
    {
        string setName = StageManager.Instance.gameVersion.ToString();
        string filePath = Path.Combine(Application.persistentDataPath, setName, "stages.json");
        string json;
        if (!File.Exists(filePath))
        {
            CustomDebugger.Log("No saved stages found at " + filePath,DebugCategory.LOAD);
            yield return StartCoroutine(GetJson("stages"));
        }

        CustomDebugger.Log(filePath,DebugCategory.LOAD);
        json = File.ReadAllText(filePath);

        // Deserialize the JSON to the intermediate object
        Serialization<StageData> stageList = JsonConvert.DeserializeObject<Serialization<StageData>>(json);
        // Después de deserializar

        if (stageList == null || stageList.items == null)
        {
            Debug.LogError("Failed to deserialize stages.");
        }

        foreach (var stageData in stageList.items)
        {
            stageData.ConvertColorStringToColorValue();
            CustomDebugger.Log("Found stage:"+stageData.stickerSet+" stage:"+stageData.title,DebugCategory.LOAD);
        }

        // Convert the list to a dictionary
        Dictionary<int, StageData> stages = stageList.items.ToDictionary(stage => stage.stageID, stage => stage);
        CustomDebugger.Log("Stages loaded from " + filePath + " stages: " + stages.Count);
        this.stages = stages;
        yield return null;
        StageManager.Instance.InitializeStages();
    }

    public void SaveUserData()
    {
        if (userData.stickerDuplicates.Count == 0)
        {
            return;
        }
        string setName = StageManager.Instance.gameVersion.ToString();
        string filePath = Path.Combine(Application.persistentDataPath, setName, "userData.json");
        string json = JsonConvert.SerializeObject(userData, Formatting.Indented);
        File.WriteAllText(filePath, json);
        CustomDebugger.Log("UserData saved to " + filePath);
    }

    public IEnumerator LoadUserData()
    {
        string setName = StageManager.Instance.gameVersion.ToString();
        string filePath = Path.Combine(Application.persistentDataPath, setName, "userData.json");
        if (!File.Exists(filePath))
        {
            CustomDebugger.Log("No userdata found at " + filePath);
            yield return StartCoroutine(GetJson("userData"));
        }

        string json = File.ReadAllText(filePath);
        // Define la configuración del JsonSerializer con tu convertidor personalizado
        //JsonSerializerSettings settings = new JsonSerializerSettings();
        //settings.Converters.Add(new StickerSetConverter());

        // Deserializa el JSON utilizando los ajustes
        UserData userData = JsonConvert.DeserializeObject<UserData>(json);


        if (userData != null)
        { 
            userData.ConvertListToDictionary();
            CustomDebugger.Log("UserData loaded from " + filePath + " stages:" + userData.stages.Count,DebugCategory.LOAD);
        }
        else
        {
            Debug.LogError("Failed to load UserData from " + filePath);
        }


        this.userData = userData;
        yield return null;
        yield return StartCoroutine(LoadStages());
        yield return StartCoroutine(LoadStickerLevels());
        yield return StartCoroutine(LoadPacks());
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
        string url = "https://leandromagonza.github.io/MemoGram/"+setName+"/" + file_name + ".json";
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError ||
                www.result == UnityWebRequest.Result.ProtocolError)
            {
                CustomDebugger.Log(www.error + url);
            }
            else
            {
                string filePath = Path.Combine(Application.persistentDataPath, setName, file_name + ".json");
                File.WriteAllText(filePath, www.downloadHandler.text);
            }
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