using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;
using Newtonsoft.Json;
using UnityEngine;

#if (UNITY_IOS || UNITY_TVOS)
using UnityEngine.SocialPlatforms.GameCenter;
#endif

[RequireComponent(typeof(FirebaseManager))]
public class FirebaseFirestoreCustom : MonoBehaviour {
    // Path to the collection to query on.
    public string collectionPath = "col1";

    // DocumentID within the collection. Set to empty to use an autoid (which
    // obviously only works for writing new documents.)
    public string documentId = "user-id";
    public string fieldContents;
    private DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;
    protected bool isFirebaseInitialized = false;

    // Whether an operation is in progress.
    protected bool operationInProgress;

    // Cancellation token source for the current operation.
    protected CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();


    protected FirebaseFirestore db => FirebaseFirestore.DefaultInstance;

    protected void Start() {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available) {
                InitializeFirebase();
            }
            else {
                Debug.LogError(
                    "Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }

    protected virtual void InitializeFirebase() {
        isFirebaseInitialized = true;
    }

    // Cancel the currently running operation.
    protected void CancelOperation() {
        if (!operationInProgress || cancellationTokenSource == null) return;
        Debug.Log("*** Cancelling operation *** ...");
        cancellationTokenSource.Cancel();
        cancellationTokenSource = null;
    }

    private static string DictToString(IDictionary<string, object> d) {
        if (d == null)
        {
            return "{}";
        }
        return "{ " + d
            .Select(kv => "(" + kv.Key + ", " + kv.Value + ")")
            .Aggregate("", (current, next) => current + next + ", ") + "}";
    }
    
    private CollectionReference GetCollectionReference() {
        return db.Collection(collectionPath);
    }

    private DocumentReference GetDocumentReference() {
        if (documentId == "") {
            return GetCollectionReference().Document();
        }
        return GetCollectionReference().Document(documentId);
    }
    
    public void ReadDoc() {
        if (!operationInProgress) {
            StartCoroutine(ReadDoc(GetDocumentReference()));
        }
    }
    
    private IEnumerator ReadDoc(DocumentReference doc) {
        Task<DocumentSnapshot> getTask = doc.GetSnapshotAsync();
        yield return new WaitForTaskCompletion(getTask);
        if (!(getTask.IsFaulted || getTask.IsCanceled)) {
            DocumentSnapshot snap = getTask.Result;
            // TODO(rgowman): Handle `!snap.exists()` case.
            IDictionary<string, object> resultData = snap.ToDictionary();
            fieldContents = "Ok: " + DictToString(resultData);
        } else {
            fieldContents = "Error";
        }
    }
    
    protected IEnumerator GetKnownValue() {
        var doc1 = db.Collection("col1").Document("doc1");
        var task = doc1.GetSnapshotAsync();
        yield return new WaitForTaskCompletion(task);
        if (task.IsFaulted || task.IsCanceled) yield break;
        var snap = task.Result;
        IDictionary<string, object> dict = snap.ToDictionary();
        if (dict.ContainsKey("field1")) {
            fieldContents = dict["field1"].ToString();
        }
        else {
            Debug.LogError("ERROR: Successfully retrieved col1/doc1, but it doesn't contain 'field1' key");
        }
    }

    public void WriteDoc(Dictionary<string, object> data) {
        StartCoroutine(WriteDoc(GetDocumentReference(), data));
    }
    
    private IEnumerator WriteDoc(DocumentReference doc, IDictionary<string, object> data) {
        Task setTask = doc.SetAsync(data);
        yield return new WaitForTaskCompletion(setTask);
        if (!(setTask.IsFaulted || setTask.IsCanceled)) {
            // Update the collectionPath/documentId because:
            // 1) If the documentId field was empty, this will fill it in with the autoid. This allows
            //    you to manually test via a trivial 'click set', 'click get'.
            // 2) In the automated test, the caller might pass in an explicit docRef rather than pulling
            //    the value from the UI. This keeps the UI up-to-date. (Though unclear if that's useful
            //    for the automated tests.)
            collectionPath = doc.Parent.Id;
            documentId = doc.Id;

            fieldContents = "Ok";
        } else {
            fieldContents = "Error";
        }
    }
    
    
    public void UpdateDoc(Dictionary<string, object> data) {
        StartCoroutine(UpdateDoc(GetDocumentReference(), data));
    }

    
    private IEnumerator UpdateDoc(DocumentReference doc, IDictionary<string, object> data) {
        Task updateTask = doc.UpdateAsync(data);
        yield return new WaitForTaskCompletion(updateTask);
        if (!(updateTask.IsFaulted || updateTask.IsCanceled)) {
            // Update the collectionPath/documentId because:
            // 1) In the automated test, the caller might pass in an explicit docRef rather than pulling
            //    the value from the UI. This keeps the UI up-to-date. (Though unclear if that's useful
            //    for the automated tests.)
            collectionPath = doc.Parent.Id;
            documentId = doc.Id;

            fieldContents = "Ok";
        } else {
            fieldContents = "Error";
        }
    }

    public void SaveJson(string jsonString)
    {
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        DocumentReference docRef = db.Collection(collectionPath).Document();
        

        string content = File.ReadAllText(Path.Combine(Application.persistentDataPath, "userData.json"));
        Dictionary<string, object> data = JsonConvert.DeserializeObject<Dictionary<string, object>>(content);

        docRef.SetAsync(data).ContinueWith(task => {
            if (task.IsCompleted)
            {
                if (task.IsFaulted)
                {
                    Debug.Log("error completed");
                    return;
                }
                Debug.Log("Documento guardado exitosamente");
            }
            else
            {
                Debug.LogError("Error al guardar documento: " + task.Exception);
            }
        });
    }
}

// Wait for task completion, throwing an exception if the task fails.
// This could be typically implemented using
// yield return new WaitUntil(() => task.IsCompleted);
// however, since many procedures in this sample nest coroutines and we want any task exceptions
// to be thrown from the top level coroutine (e.g GetKnownValue) we provide this
// CustomYieldInstruction implementation wait for a task in the context of the coroutine using
// common setup and tear down code.
class WaitForTaskCompletion : CustomYieldInstruction {
    Task task;

    // Create an enumerator that waits for the specified task to complete.
    public WaitForTaskCompletion(Task task) {
        this.task = task;
    }

    // Wait for the task to complete.
    public override bool keepWaiting {
        get {
            if (task.IsCompleted) {
                Debug.Log("task completed");
                if (task.IsFaulted) {
                    Debug.Log("task faulted");
                }

                return false;
            }

            return true;
        }
    }
}