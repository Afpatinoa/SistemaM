using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NombreUsuarioText : MonoBehaviour
{
    public TMP_Text nombreUsuarioText;

    void Start()
    {
        // Obtenemos el nombre de usuario y lo asignamos al texto
        Firebase.Auth.FirebaseUser user = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser;
        if (user != null)
        {
            nombreUsuarioText.text = "Usuario: " + user.DisplayName;
        }
    } // <-- Asegúrate de que aquí haya una llave de cierre
}

