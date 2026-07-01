using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ManagerLevelCompleto : MonoBehaviour
{
    //[SerializeField] private string nivelSiguiente;

    public void CargarNivelSiguiente(string nivelSiguiente)
    {
        TransicionEscena.Instancia.IniciarTransicion(nivelSiguiente);
        //SceneManager.LoadScene(nivelSiguiente);
    }
}
