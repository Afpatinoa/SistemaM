using Firebase;
using Firebase.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Firebase.Auth;

public class FirebaseUploader : MonoBehaviour
{
    // La ruta dentro de Firebase Storage donde se guardará la imagen
    private string firebaseStoragePath = "Imagenes/";

    // La imagen que se cargará desde el RawImage
    private Texture2D image;

    // El nombre del archivo que se subirá a Firebase Storage
    private string fileName = "image_de_perfil.png";

    private Firebase.Storage.FirebaseStorage storage;

    // Objeto RawImage para cargar la imagen a Firebase Storage
    public RawImage rawImage;

    void Start()
    {
        // Inicializa la instancia de Firebase Storage
        storage = Firebase.Storage.FirebaseStorage.DefaultInstance;
    }

    public void UploadImage()
    {
        // Convierte la imagen del RawImage a un Texture2D
        image = (Texture2D)rawImage.mainTexture;

        // Convierte el Texture2D a un byte array
        byte[] bytes = image.EncodeToPNG();

        // Busca el objeto que tiene el script AuthManager
        AuthManager authManager = FindObjectOfType<AuthManager>();

        // Obtiene el nombre del usuario actual
        string nombreUsuario = authManager.GetCurrentUser().DisplayName;

        // Actualiza la ruta de Firebase Storage donde se guardará la imagen
        firebaseStoragePath = "Imagenes/" + nombreUsuario + "/";

        // Crea una referencia a la imagen en Firebase Storage
        Firebase.Storage.StorageReference storageRef = storage.GetReference(firebaseStoragePath + fileName);

        // Crea un stream a partir del byte array
        Stream stream = new MemoryStream(bytes);

        // Inicia la carga de la imagen a Firebase Storage
        storageRef.PutStreamAsync(stream).ContinueWith(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.Log("Error al subir la imagen a Firebase Storage");
                return;
            }

            // Obtiene la URL de descarga de la imagen recién subida
            storageRef.GetDownloadUrlAsync().ContinueWith(downloadUrlTask =>
            {
                if (downloadUrlTask.IsFaulted || downloadUrlTask.IsCanceled)
                {
                    Debug.Log("Error al obtener la URL de descarga de la imagen");
                    return;
                }

                string downloadUrl = downloadUrlTask.Result.ToString();
                Debug.Log("URL de descarga de la imagen: " + downloadUrl);
            });
        });
    }


}
