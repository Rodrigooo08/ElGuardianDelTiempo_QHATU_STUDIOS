using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipal : MonoBehaviour
{ 
    public void Jugar()
    {
        if (TransicionEscena.Instancia != null)
        {
            TransicionEscena.Instancia.IniciarTransicion("Jugar");
        }
        else
        {
            Debug.LogError("TransicionEscena no está inicializado. Asegúrate de que el objeto está presente en la escena.");
        }
    }
    public void SeleccionarNivel()
    {
        TransicionEscena.Instancia.IniciarTransicion("MenuNiveles");//8 es el orden de la escena
    }
    public void Salir()
    {
        Debug.Log("SALIR");
        Application.Quit();
    }
}
