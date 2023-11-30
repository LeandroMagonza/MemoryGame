using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using Firebase.Auth;
using System.Threading.Tasks;
using UnityEngine.Networking;

[RequireComponent(typeof(FirebaseAuthCustom))]
[RequireComponent(typeof(FirebaseFirestoreCustom))]
public class FirebaseManager : MonoBehaviour {
    #region Singleton

    private static FirebaseManager _instance;

    public static FirebaseManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<FirebaseManager>();
            if (_instance == null)
                Debug.LogError("Singleton<" + typeof(FirebaseManager) + "> instance has been not found.");
            return _instance;
        }
    }

    protected void Awake()
    {
        if (_instance == null)
        {
            _instance = this as FirebaseManager;
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
    FirebaseAuthCustom authFirebase;
    FirebaseFirestoreCustom firestoreFirebase;
    
    protected void Start() {
        authFirebase = GetComponent<FirebaseAuthCustom>();
        firestoreFirebase = GetComponent<FirebaseFirestoreCustom>();
    }

    public IEnumerator TestFirestore() {
        yield return new WaitForSeconds(2);
        // CRUD collection
        firestoreFirebase.collectionPath = "save-game";
        // Specific document (save with user id)
        firestoreFirebase.documentId = authFirebase.auth.CurrentUser.UserId;

        // Write document
        // var data = new Dictionary<string, object>{
        //     {"f1", "v1"},
        //     {"f2", 2},
        //     {"f3", true}
        // };
        // firestoreFirebase.WriteDoc(data);
        
        // Update document
        var data = new Dictionary<string, object>{
            {"f1", "v2"},
            {"f2", 3},
            {"f3", false}
        };
        firestoreFirebase.UpdateDoc(data);
        
        // Read document
        firestoreFirebase.ReadDoc();
        // TODO: wait for read to complete
        yield return new WaitForSeconds(2);
        Debug.Log("read completed");
        Debug.Log(firestoreFirebase.fieldContents);
    } 
    public IEnumerator SaveUserData(string userData)
    {
        SomeOtherFunction();
        yield break;
        
        Debug.Log("Saveuser data in firebase was called");
        yield return new WaitForSeconds(2);
        // CRUD collection
        firestoreFirebase.collectionPath = "save-game";
        // Specific document (save with user id)
        firestoreFirebase.documentId = authFirebase.auth.CurrentUser.UserId;
        Debug.Log(firestoreFirebase.documentId);

        // Write document
        // var dataField = new Dictionary<string, object>
        // {
        //     { "f1", "v1" },
        //     { "f2", 2 },
        //
        // };
        // var listField = new List<int>() { 1, 2, 3, 4, 5, 6 };
        // var listDict = new List<object>() { dataField, dataField };
        // var data = new Dictionary<string, object>{
        // {"f1", "v1"},
        // {"f2", 2},
        // {"f3", true},
        // {"f4", null},
        // {
        //     "f5", dataField
        // },
        // {
        //     "f6", listField
        // },
        // {
        //     "f7", listDict
        // }
        // };
        //
        // Update document

        
        // Debug.Log("Writing in firebase was called");
        // foreach (var VARIABLE in userData)
        // {
            // Debug.Log(VARIABLE.Key);
            // Debug.Log(VARIABLE.Value );
            
        // }
        // firestoreFirebase.SaveJson(userData);

        string content = File.ReadAllText( "Assets/Resources/Pokemons_SPRITESHEET_151/userData2.json");
        Debug.Log(content);
        //Dictionary<string, object> data1 = JsonConvert.DeserializeObject<Dictionary<string, object>>(content);
        
        var settings = new JsonSerializerSettings {
            TypeNameHandling = TypeNameHandling.Auto
        };
        var myData = JsonConvert.DeserializeObject<Dictionary<string, object>>(content, settings);
        
        Debug.Log(myData.ToString());
        firestoreFirebase.WriteDoc(myData);
        // Dictionary<string, object> data2 = JsonConvert.DeserializeObject<Dictionary<string, object>>("{ \"dni\": \"73746546557\"}");
        // firestoreFirebase.UpdateDoc(data2);
        
        // Read document
        //firestoreFirebase.ReadDoc();
        // TODO: wait for read to complete
        // yield return new WaitForSeconds(2);
        //Debug.Log("read completed");
        // Debug.Log(firestoreFirebase.fieldContents);
    }
    public IEnumerator GetUserTokenCoroutine(System.Action<string> callback) {
        var user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user != null) {
            var getTokenTask = user.TokenAsync(false);
            yield return new WaitUntil(() => getTokenTask.IsCompleted);

            if (getTokenTask.Exception != null) {
                Debug.LogError($"Failed to get token: {getTokenTask.Exception}");
                callback(null);
            } else {
                string token = getTokenTask.Result;
                callback(token);
            }
        } else {
            // Manejar el caso donde no hay un usuario autenticado
            Debug.LogWarning("No hay usuario autenticado.");
            callback(null);
        }
    }
    public void SomeOtherFunction() {
        string collectionId = firestoreFirebase.collectionPath = "save-game";
        string documentId = firestoreFirebase.documentId = authFirebase.auth.CurrentUser.UserId;
        StartCoroutine(GetUserTokenCoroutine((token) => {
            if (!string.IsNullOrEmpty(token))
            {
                string url =
                    $"https://firestore.googleapis.com/v1/projects/personaltest-381501/databases/(default)/documents/{collectionId}/{documentId}?key={}";
                // string url = $"https://personaltest-381501.firebaseio.com/collections/{collectionId}/{documentId}.json"; 
                Debug.Log("Token: " + token);
                StartCoroutine(SendJsonToFirebase(" lo lee del archivo userdata2 ",url, token));
            } else {
                // No se pudo obtener el token o no hay usuario autenticado
                Debug.LogError("No se pudo obtener el token de autenticación.");
            }
        }));
    }
    
    
    IEnumerator SendJsonToFirebase(string json, string url, string token) {
        
        var request = new UnityWebRequest(url, "POST");
        string content = File.ReadAllText( "Assets/Resources/Pokemons_SPRITESHEET_151/userData2.json");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(content);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {token}");

        yield return request.SendWebRequest();

        if (request.error != null) {
            Debug.LogError("Error: " + request.error);
        } else {
            Debug.Log("Response: " + request.downloadHandler.text);
        }
    }
    
}