using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;

public class Consumer : MonoBehaviour
{
    private Firebase.FirebaseApp app;

    void Start()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                app = Firebase.FirebaseApp.DefaultInstance;

                // Set a flag here to indicate whether Firebase is ready to use by your app.
                Login();
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });

    }

    private void AddData(Vector3 position)
    {
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;

        DocumentReference docRef = db.Collection("users").Document(auth.CurrentUser.UserId);

        Dictionary<string, object> user = new Dictionary<string, object>
        {
            { "First", "Ada" },
            { "Last", "Lovelace" },
            { "Born", 1815 },
            { "Position", new Dictionary<string, object>
                {
                    { "x", position.x },
                    { "y", position.y },
                    { "z", position.z }
                }
            }
        };
        docRef.SetAsync(user).ContinueWithOnMainThread(task => {
            Debug.Log("Added data to the alovelace document in the users collection.");
            GetData();
        });
    }

    private void GetData()
    {
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        CollectionReference usersRef = db.Collection("users");
        usersRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            QuerySnapshot snapshot = task.Result;
            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                Debug.Log(message: $"User: { document.Id}");
                Dictionary<string, object> documentDictionary = document.ToDictionary();
                Debug.Log(message: $"First: {documentDictionary["First"]}");
                if (documentDictionary.ContainsKey("Middle"))
                {
                    Debug.Log(message: $"Middle: {documentDictionary["Middle"]}");
                }

                Debug.Log(message: $"Last: { documentDictionary["Last"]}");
                Debug.Log(message: $"Born: { documentDictionary["Born"]}");

                if (documentDictionary.ContainsKey("Position"))
                {
                    Dictionary<string, object> positionDictionary = documentDictionary["Position"] as Dictionary<string, object>;
                    float x = System.Convert.ToSingle(positionDictionary["x"]);
                    float y = System.Convert.ToSingle(positionDictionary["y"]);
                    float z = System.Convert.ToSingle(positionDictionary["z"]);
                    Debug.Log(message: $"Position: x={x}, y={y}, z={z}");
                }
            }

            Debug.Log(message: "Read all data from the users collection.");
        });
    }

    private void Login()
    {
        Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;

        if (auth.CurrentUser != null)
        {
            Debug.Log("Usuario existente");
            AddData(transform.position);
            return;
        }

        auth.SignInAnonymouslyAsync().ContinueWith(task => {
        if (task.IsCanceled)
        {
            Debug.LogError("SignInAnonymouslyAsync was canceled.");
            return;
        }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
            }
        });
    }
}
