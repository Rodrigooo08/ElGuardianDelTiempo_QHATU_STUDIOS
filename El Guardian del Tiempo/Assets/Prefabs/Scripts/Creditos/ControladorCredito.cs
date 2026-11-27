using UnityEngine;
using UnityEngine.SceneManagement;

public class ControladorCredito : MonoBehaviour
{
    [Header("Navegacion")]
    [SerializeField] private string nombreEscena = "MainMenu";

    public void VolverAlMenu()
    {
        SceneManager.LoadScene(nombreEscena);
    }
}
