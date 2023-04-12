﻿using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using TMPro;
using UnityEngine.SceneManagement;
using Firebase.Storage;
using Firebase.Extensions;
using System.IO;

public class AuthManager : MonoBehaviour
{

    public static AuthManager instance;

    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser User;


    [Header("Login")]
    public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;
    public TMP_Text warningLoginText;
    public TMP_Text confirmLoginText;


    [Header("Register")]
    public TMP_InputField usernameRegisterField;
    public TMP_InputField emailRegisterField;
    public TMP_InputField passwordRegisterField;
    public TMP_InputField passwordRegisterVerifyField;
    public TMP_Text warningRegisterText;


    void Awake()
    {
        instance = this;

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {

                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }


    private void InitializeFirebase()
    {
        Debug.Log("Setting up Firebase Auth");

        auth = FirebaseAuth.DefaultInstance;
    }


    public void LoginButton()
    {

        StartCoroutine(Login(emailLoginField.text, passwordLoginField.text));

        DontDestroyOnLoad(this.gameObject);

    }

    public void RegisterButton()
    {

        StartCoroutine(Register(emailRegisterField.text, passwordRegisterField.text, usernameRegisterField.text));
    }

    private IEnumerator Login(string _email, string _password)
    {

        var LoginTask = auth.SignInWithEmailAndPasswordAsync(_email, _password);

        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

        if (LoginTask.Exception != null)
        {

            Debug.LogWarning(message: $"Failed to register task with {LoginTask.Exception}");
            FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "Login Failed!";
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    message = "Missing Email";
                    break;
                case AuthError.MissingPassword:
                    message = "Missing Password";
                    break;
                case AuthError.WrongPassword:
                    message = "Wrong Password";
                    break;
                case AuthError.InvalidEmail:
                    message = "Invalid Email";
                    break;
                case AuthError.UserNotFound:
                    message = "Account does not exist";
                    break;
            }
            warningLoginText.text = message;
        }
        else
        {

            User = LoginTask.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})", User.DisplayName, User.Email);
            warningLoginText.text = "";
            confirmLoginText.text = "Logged In";
            SceneManager.LoadScene("UsuariosRegistrados");
        }
    }

    private IEnumerator Register(string _email, string _password, string _username)
    {
        if (_username == "")
        {
            warningRegisterText.text = "Missing Username";
        }
        else if (passwordRegisterField.text != passwordRegisterVerifyField.text)
        {
            warningRegisterText.text = "Password Does Not Match!";
        }
        else
        {
            var RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);

            yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);

            if (RegisterTask.Exception != null)
            {
                // Handle registration errors
            }
            else
            {
                User = RegisterTask.Result;

                if (User != null)
                {
                    // Set user display name
                    UserProfile profile = new UserProfile { DisplayName = _username };
                    var ProfileTask = User.UpdateUserProfileAsync(profile);
                    yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

                    if (ProfileTask.Exception != null)
                    {
                        // Handle profile update errors
                    }
                    else
                    {
                        // Create user folder in Firebase Storage
                        string folderName = _username;
                        CreateFolder(folderName);

                        warningRegisterText.text = "";
                        SceneManager.LoadScene("UsuariosRegistrados");
                    }
                }
            }
        }
    }



    private void CreateFolder(string folderName)
    {
        // Create a reference to the storage folder
        var storageReference = FirebaseStorage.DefaultInstance.GetReference("Imagenes");

        // Create a reference to the user's folder
        var userReference = storageReference.Child(folderName);

        // Create a dummy file inside the user's folder
        var dummyTask = userReference.Child("dummy").PutBytesAsync(new byte[0]);

        // Create a text file inside the user's folder with the user's information
        string userId = auth.CurrentUser.UserId;
        string email = auth.CurrentUser.Email;
        string userData = "Email: " + email + "\nUser ID: " + userId + "\nFriend List:\n";
        var dataTask = userReference.Child("user_info.txt").PutBytesAsync(System.Text.Encoding.UTF8.GetBytes(userData));

        // Wait until both tasks are completed
        Task.WhenAll(dummyTask, dataTask).ContinueWith(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError(task.Exception);
            }
            else
            {
                Debug.Log("User folder created successfully");
            }
        });
    }





    public FirebaseUser GetCurrentUser()
    {
        return auth.CurrentUser;
    }

}
