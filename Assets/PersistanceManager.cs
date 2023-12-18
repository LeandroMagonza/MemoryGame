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

    // Update is called once per frame
    void Update()
    {

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
        string filePath = Path.Combine(Application.persistentDataPath, "stages.json");
        File.WriteAllText(filePath, json);
        Debug.Log("Stages saved to " + filePath);
    }

    public IEnumerator LoadStages()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "stages.json");
        string json;
        if (!File.Exists(filePath))
        {
            Debug.Log("No saved stages found at " + filePath);
            yield return StartCoroutine(GetJson("stages"));
        }

        Debug.Log(filePath);
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
        }

        // Convert the list to a dictionary
        Dictionary<int, StageData> stages = stageList.items.ToDictionary(stage => stage.stageID, stage => stage);
        Debug.Log("Stages loaded from " + filePath + " stages: " + stages.Count);
        this.stages = stages;
        yield return null;
        StageManager.Instance.InitializeStages();
    }

    public void SaveUserData()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "userData.json");
        string json = JsonConvert.SerializeObject(userData, Formatting.Indented);
        File.WriteAllText(filePath, json);
        Debug.Log("UserData saved to " + filePath);
    }

    public IEnumerator LoadUserData()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "userData.json");
        if (!File.Exists(filePath))
        {
            Debug.Log("No userdata found at " + filePath);
            yield return StartCoroutine(GetJson("userData"));
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


        this.userData = userData;
        yield return null;
        yield return StartCoroutine(LoadStages());
        yield return StartCoroutine(LoadStickerLevels());
        yield return StartCoroutine(LoadPacks());
    }

    public IEnumerator LoadStickerLevels()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "stages.json");
        string json;
        if (!File.Exists(filePath))
        {
            Debug.Log("No saved stages found at " + filePath);
            yield return StartCoroutine(GetJson("stages"));
        }

        Debug.Log(filePath);
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

        Debug.Log("StickerLevels loaded from " + filePath + " levels: " + stickerLevels.Count);

        // Do whatever you need with the stickerLevels dictionary

        yield return null;
    }

    // Función para cargar los datos de packs
    public IEnumerator LoadPacks()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "stages.json");
        string json;
        if (!File.Exists(filePath))
        {
            Debug.Log("No saved stages found at " + filePath);
            yield return StartCoroutine(GetJson("stages"));
        }

        Debug.Log(filePath);
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

        Debug.Log("Packs loaded from " + filePath);
        Debug.Log("stickersPerPack on load: " + packs.stickersPerPack);
        yield return null;
    }

    IEnumerator GetJson(string file_name)
    {
        string url = "https://leandromagonza.github.io/MemoGram.Pokemon/" + file_name + ".json";
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError ||
                www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(www.error);
            }
            else
            {
                string filePath = Path.Combine(Application.persistentDataPath, file_name + ".json");
                File.WriteAllText(filePath, www.downloadHandler.text);
            }
        }
    }
}

   
