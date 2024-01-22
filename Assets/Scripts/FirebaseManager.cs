/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FirebaseAuthCustom))]
[RequireComponent(typeof(FirebaseFirestoreCustom))]
public class FirebaseManager : MonoBehaviour {
    FirebaseAuthCustom authFirebase;
    FirebaseFirestoreCustom firestoreFirebase;
    
    protected void Start() {
        authFirebase = GetComponent<FirebaseAuthCustom>();
        firestoreFirebase = GetComponent<FirebaseFirestoreCustom>();
        StartCoroutine("TestFirestore");
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
        CustomDebugger.Log("read completed");
        CustomDebugger.Log(firestoreFirebase.fieldContents);
    }
}*/