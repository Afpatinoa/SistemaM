using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Firebase;
using Firebase.Database;

public class OnlineUsersList : MonoBehaviour
{
    public TMP_Text usersText;
    private DatabaseReference databaseReference;
    private Dictionary<string, bool> onlineUsers;

    private void Start()
    {
        onlineUsers = new Dictionary<string, bool>();
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        databaseReference.Child("users").ChildAdded += HandleUserAdded;
        databaseReference.Child("users").ChildRemoved += HandleUserRemoved;
    }

    private void HandleUserAdded(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        string userId = args.Snapshot.Key;
        bool isOnline = (bool)args.Snapshot.Value;
        onlineUsers[userId] = isOnline;
        UpdateUsersText();
    }

    private void HandleUserRemoved(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        string userId = args.Snapshot.Key;
        onlineUsers.Remove(userId);
        UpdateUsersText();
    }

    private void UpdateUsersText()
    {
        List<string> onlineUsersList = new List<string>();
        foreach (KeyValuePair<string, bool> pair in onlineUsers)
        {
            if (pair.Value)
            {
                onlineUsersList.Add(pair.Key);
            }
        }

        if (onlineUsersList.Count == 0)
        {
            usersText.text = "No hay usuarios en línea";
        }
        else if (onlineUsersList.Count == 1)
        {
            usersText.text = onlineUsersList[0] + " está en línea";
        }
        else
        {
            string usersString = string.Join(" vs ", onlineUsersList.ToArray());
            usersText.text = usersString + " están en línea";
        }
    }
}
