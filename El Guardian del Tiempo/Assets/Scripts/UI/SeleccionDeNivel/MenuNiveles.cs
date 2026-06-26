using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuNiveles : MonoBehaviour
{
public void CambiarNivel(string nombreNivel)
    {
        if (TransicionEscena.Instancia != null)
        {
            TransicionEscena.Instancia.IniciarTransicion(nombreNivel);
        }
        else
        {
            Debug.LogError("TransicionEscena no está inicializado. Asegúrate de que el objeto está presente en la escena.");
        }
    }
    public void CambiarNivel(int numeroNivel)
    {
        if (TransicionEscena.Instancia != null)
        {
            TransicionEscena.Instancia.IniciarTransicion(numeroNivel);
        }
        else
        {
            Debug.LogError("TransicionEscena no está inicializado. Asegúrate de que el objeto está presente en la escena.");
        }
    }
    public void VolverMenu()
    {
        if (TransicionEscena.Instancia != null)
        {
            TransicionEscena.Instancia.IniciarTransicion("MainMenu");
        }
        else
        {
            Debug.LogError("TransicionEscena no está inicializado. Asegúrate de que el objeto está presente en la escena.");
        }
    }
}
