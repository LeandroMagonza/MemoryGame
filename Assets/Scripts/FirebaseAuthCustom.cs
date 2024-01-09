using System;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine;

[RequireComponent(typeof(FirebaseManager))]
public class FirebaseAuthCustom : MonoBehaviour {
    public FirebaseAuth auth;

    private void Start() {
        // TODO: verify current user is signed in or not and sign in anonymously if not
        InitializeFirebase();
    }

    private void InitializeFirebase() {
        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }

    private void AuthStateChanged(object sender, EventArgs eventArgs) {
        if (auth.CurrentUser != null && !auth.CurrentUser.IsAnonymous) {
            // The user is already connected, you don't need to do anything else.
            CustomDebugger.Log("User is already signed in.");
        }
        else {
            // There is no user logged in, try to log in anonymously.
            SigninAnonymouslyAsync();
        }
    }

    public void SignOut() {
        if (auth.CurrentUser != null) {
            auth.SignOut();
            CustomDebugger.Log("User signed out successfully.");
        }
        else {
            CustomDebugger.Log("No user is currently signed in.");
        }
    }

    public Task SigninAnonymouslyAsync() {
        if (auth.CurrentUser == null) {
            CustomDebugger.Log("Attempting to sign in anonymously...");
            return auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(HandleSignInWithAuthResult);
        }

        // The user is already logged in, no need to log in again.
        CustomDebugger.Log("Already signed in anonymously.");
        return Task.CompletedTask;
    }

    protected void HandleSignInWithAuthResult(Task<AuthResult> task) {
        // EnableUI();
        if (!LogTaskCompletion(task, "Sign-in")) return;
        if (task.Result.User != null && task.Result.User.IsValid()) {
            DisplayAuthResult(task.Result, 1);
            CustomDebugger.Log(String.Format("{0} signed in", task.Result.User.DisplayName));
        }
        else {
            CustomDebugger.Log("Signed in but User is either null or invalid");
        }
    }

    protected void DisplayAuthResult(AuthResult result, int indentLevel) {
        string indent = new String(' ', indentLevel * 2);
        var metadata = result.User != null ? result.User.Metadata : null;
        if (metadata != null) {
            CustomDebugger.Log(String.Format("{0}Created: {1}", indent, metadata.CreationTimestamp));
            CustomDebugger.Log(String.Format("{0}Last Sign-in: {1}", indent, metadata.LastSignInTimestamp));
        }

        var info = result.AdditionalUserInfo;
        if (info != null) {
            CustomDebugger.Log(String.Format("{0}Additional User Info:", indent));
            CustomDebugger.Log(String.Format("{0}  User Name: {1}", indent, info.UserName));
            CustomDebugger.Log(String.Format("{0}  Provider ID: {1}", indent, info.ProviderId));
        }

        var credential = result.Credential;
        if (credential != null) {
            CustomDebugger.Log(String.Format("{0}Credential:", indent));
            CustomDebugger.Log(String.Format("{0}  Is Valid?: {1}", indent, credential.IsValid()));
            CustomDebugger.Log(String.Format("{0}  Class Type: {1}", indent, credential.GetType()));
            if (credential.IsValid()) {
                CustomDebugger.Log(String.Format("{0}  Provider: {1}", indent, credential.Provider));
            }
        }
    }


    // Log the result of the specified task, returning true if the task
    // completed successfully, false otherwise.
    protected bool LogTaskCompletion(Task task, string operation) {
        bool complete = false;
        if (task.IsCanceled) {
            CustomDebugger.Log(operation + " canceled.");
        }
        else if (task.IsFaulted) {
            CustomDebugger.Log(operation + " encounted an error.");
            foreach (Exception exception in task.Exception.Flatten().InnerExceptions) {
                string authErrorCode = "";
                FirebaseException firebaseEx = exception as FirebaseException;
                if (firebaseEx != null) {
                    authErrorCode = String.Format("AuthError.{0}: ",
                        ((AuthError)firebaseEx.ErrorCode).ToString());
                }

                CustomDebugger.Log(authErrorCode + exception.ToString());
            }
        }
        else if (task.IsCompleted) {
            CustomDebugger.Log(operation + " completed");
            complete = true;
        }

        return complete;
    }
}