using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransicionEscena : MonoBehaviour
{
    public static TransicionEscena Instancia { get; private set; }

    private Animator animator;

    [SerializeField] private AnimationClip animacionFinal;

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
    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    private void Update()
    {
        /*if (Input.GetKeyUp(KeyCode.Escape))
        {
            IniciarTransicion(6);
        }
        if (Input.GetKeyUp(KeyCode.V))
        {
            IniciarTransicion(7);
        }*/
    }
    public void IniciarTransicion(int indice)
    {
        StartCoroutine(CambiarEscena(indice));
    }
    private IEnumerator CambiarEscena(int indice)
    {
        animator.SetTrigger("Finalizar");
        yield return new WaitForSeconds(animacionFinal.length);

        SceneManager.LoadScene(indice);
        animator.SetTrigger("Iniciar");
    }
    public void IniciarTransicion(string escena)
    {
        StartCoroutine(CambiarEscena(escena));
    }
    private IEnumerator CambiarEscena(string escena)
    {
        animator.SetTrigger("Finalizar");
        yield return new WaitForSeconds(animacionFinal.length);

        SceneManager.LoadScene(escena);
        animator.SetTrigger("Iniciar");
    }
}
