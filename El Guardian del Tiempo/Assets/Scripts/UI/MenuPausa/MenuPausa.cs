using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPausa : MonoBehaviour
{
    public static MenuPausa Instancia { get; private set; }

    private bool pausa = false;
    [SerializeField] private GameObject menuPausa;

    private void Awake()
    {
        Debug.Log("TransicionEscena: Awake ejecutado.");
        if (Instancia != null && Instancia != this)
        {
            Debug.Log("TransicionEscena: Instancia duplicada detectada. Destruyendo esta instancia.");
            Destroy(gameObject);
            return;
        }
        Instancia = this;
        DontDestroyOnLoad(this.gameObject);
        Debug.Log("TransicionEscena: Marcado como DontDestroyOnLoad.");
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded; // Escucha cambios de escena
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // Detén la escucha
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Busca el menú de pausa en la nueva escena
        if (menuPausa == null)
        {
            menuPausa = GameObject.Find("MenuPausa");
            if (menuPausa == null)
            {
                Debug.LogWarning("MenuPausa: No se encontró el objeto MenuPausa en la nueva escena.");
            }
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)){
            if (pausa)
            {
                Continuar();
            }
            else
            {
                Pausa();
            }
        }
    }
    public void Pausa()
    {
        pausa = true;
        Time.timeScale = 0f;
        menuPausa.SetActive(true);
    }
    public void Continuar()
    {
        pausa = false;
        Time.timeScale = 1f;
        menuPausa.SetActive(false);
    }
    public void Reiniciar()
    {
        pausa = false;
        Time.timeScale = 1f;
        menuPausa.SetActive(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void Return()
    {
        pausa = false;
        Time.timeScale = 1f;
        menuPausa.SetActive(false);
        TransicionEscena.Instancia.IniciarTransicion(0);

    }
}
