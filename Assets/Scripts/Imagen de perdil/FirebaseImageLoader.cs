using System;
using System.Threading.Tasks;
using UnityEngine;
using Firebase.Auth;
using Firebase.Storage;
using Firebase.Extensions;
using UnityEngine.UI;

public class FirebaseImageLoader : MonoBehaviour
{
    public RawImage profileImage;
    FirebaseStorage storage;
    StorageReference storageReference;
    string firebaseStoragePath;

    private void Awake()
    {
        storage = FirebaseStorage.DefaultInstance;
        storageReference = storage.GetReferenceFromUrl("gs://machm-d5265.appspot.com");
        firebaseStoragePath = "Imagenes/" + FirebaseAuth.DefaultInstance.CurrentUser.DisplayName + "/image_de_perfil.png";
    }

    private async void Start()
    {
        await LoadProfileImage();
    }

    private async Task LoadProfileImage()
    {
        try
        {
            var downloadTask = storageReference.Child(firebaseStoragePath).GetBytesAsync(
                //token: new System.Threading.CancellationToken(),
                maxDownloadSizeBytes: long.MaxValue
                );

            byte[] result = await downloadTask;

            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(result);

            profileImage.texture = texture;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load profile image: {ex.Message}");
        }
    }
}
