using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UploadButton : MonoBehaviour
{
    public FirebaseUploader firebaseUploader;
    public Button button;

    private void Start()
    {
        button.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        firebaseUploader.UploadImage();
    }
}
