using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using Firebase.Auth;
using UnityEngine.SceneManagement;


public class FirestoreManager : MonoBehaviour
{
    private FirebaseFirestore db;
    private string currentUserId;

    public InputField friendUsernameInputField;
    public TextMeshProUGUI friendRequestText;
    public Button acceptButton;
    public Button denyButton;
    public Button sendRequestButton;

    private void Start()
    {
        // Initialize Firebase app
        FirebaseApp app = FirebaseApp.DefaultInstance;

        // Initialize Firestore instance
        db = FirebaseFirestore.DefaultInstance;

        // Get current user ID
        FirebaseUser currentUser = FirebaseAuth.DefaultInstance.CurrentUser;
        if (currentUser != null)
        {
            currentUserId = currentUser.UserId;

            // Example: Get friend request data
            GetFriendRequest();

            // Add listener to accept button
            acceptButton.onClick.AddListener(() => AcceptFriendRequest("friendRequestId"));

            // Find and assign the send request button object in the scene
            sendRequestButton = GameObject.Find("SendRequestButton").GetComponent<Button>();

            // Add listener to deny button
            denyButton.onClick.AddListener(() => RejectFriendRequest("friendRequestId"));

            // Add listener to send request button
            sendRequestButton.onClick.AddListener(SendFriendRequest);
        }
        else
        {
            Debug.LogError("Current user is null!");
        }
    }



    private void GetFriendRequest()
    {
        DocumentReference docRef = db.Collection("friend_requests").Document("WZi1cVlxDgCk0n3RnQNz");
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            DocumentSnapshot snapshot = task.Result;
            if (snapshot.Exists)
            {
                Dictionary<string, object> data = snapshot.ToDictionary();
                string receiverId = (string)data["receiverId"];
                string senderId = (string)data["senderId"];
                string status = (string)data["status"];

                Debug.Log("Receiver ID: " + receiverId);
                Debug.Log("Sender ID: " + senderId);
                Debug.Log("Status: " + status);

                if (receiverId == currentUserId && status == "pending")
                {
                    string senderUsername = "";
                    db.Collection("users").Document(senderId)
                        .GetSnapshotAsync().ContinueWithOnMainThread(userTask =>
                        {
                            DocumentSnapshot userSnapshot = userTask.Result;
                            if (userSnapshot.Exists)
                            {
                                senderUsername = userSnapshot.GetValue<string>("username");
                                string message = "Tienes una solicitud de amistad de " + senderUsername;
                                friendRequestText.text = message;
                            }
                        });
                }
            }
            else
            {
                Debug.Log("Document does not exist!");
            }
        });
    }


    public void SendFriendRequest()
    {
        string friendUsername = friendUsernameInputField.text;

        db.Collection("users")
            .WhereEqualTo("username", friendUsername)
            .Limit(1)
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                QuerySnapshot snapshot = task.Result;
                if (snapshot.Count == 1)
                {
                    DocumentSnapshot userDoc = snapshot.Documents.FirstOrDefault();
                    if (userDoc != null)
                    {
                        string friendUserId = userDoc.Id;

                        // Use friendUserId to create friend request document
                        Dictionary<string, object> friendRequestData = new Dictionary<string, object>
                        {
                            { "senderId", currentUserId },
                            { "receiverId", friendUserId },
                            { "status", "pending" }
                        };

                        db.Collection("friend_requests").AddAsync(friendRequestData)
                            .ContinueWithOnMainThread(addTask =>
                            {
                                DocumentReference newFriendRequestRef = addTask.Result;
                                Debug.Log("Friend request sent to: " + friendUsername);
                            });
                    }
                    else
                    {
                        Debug.Log("User not found!");
                    }
                }
                else
                {
                    Debug.Log("User not found!");
                }
            });
    }
    public void AcceptFriendRequest(string friendRequestId)
    {
        db.Collection("friend_requests").Document(friendRequestId)
            .UpdateAsync(new Dictionary<string, object>
            {
            { "status", "accepted" }
            })
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("Friend request accepted!");

                // TODO: Add logic to add friend to friend list
            }
                else if (task.IsFaulted)
                {
                    Debug.Log("Error accepting friend request: " + task.Exception);
                }
            });
    }

    public void RejectFriendRequest(string friendRequestId)
    {
        db.Collection("friend_requests").Document(friendRequestId)
            .UpdateAsync(new Dictionary<string, object>
            {
            { "status", "rejected" }
            })
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("Friend request rejected!");
                }
                else if (task.IsFaulted)
                {
                    Debug.Log("Error rejecting friend request: " + task.Exception);
                }
            });
    }

}

