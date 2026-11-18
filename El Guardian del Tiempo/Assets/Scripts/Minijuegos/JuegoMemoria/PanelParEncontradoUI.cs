using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;

// Panel lateral que muestra el nombre y el cargo de los integrantes del par recien encontrado.
// Este objeto tiene que estar siempre activo; se oculta con el CanvasGroup (alpha 0).
[RequireComponent(typeof(CanvasGroup))]
public class PanelParEncontradoUI : MonoBehaviour
{
    [SerializeField] private ControladorMemoria controlador;
    [Tooltip("Texto chico de arriba, ej: 'Par 2 de 6' (opcional).")]
    [SerializeField] private TMP_Text textoContador;
    [Tooltip("Texto con los nombres y cargos.")]
    [SerializeField] private TMP_Text textoContenido;

    [Header("Tiempos")]
    [SerializeField] private float duracionFundido = 0.25f;

    private CanvasGroup grupo;
    private Coroutine rutina;
    // Se reutiliza para no generar basura cada vez que se arma un texto.
    private readonly StringBuilder constructor = new StringBuilder();

    private void Awake()
    {
        grupo = GetComponent<CanvasGroup>();
        // El panel es solo informativo: nunca debe bloquear los clics sobre las cartas.
        grupo.interactable = false;
        grupo.blocksRaycasts = false;
    }

    private void OnEnable()
    {
        OcultarYa();

        if (controlador == null)
        {
            Debug.LogWarning("Falta asignar el Controlador en el panel de par encontrado.", this);
            return;
        }
        controlador.ParEncontrado += AlEncontrarPar;
    }

    private void OnDisable()
    {
        if (controlador != null)
        {
            controlador.ParEncontrado -= AlEncontrarPar;
        }

        if (rutina != null)
        {
            StopCoroutine(rutina);
            rutina = null;
        }
        OcultarYa();
    }

    private void AlEncontrarPar(CartaMemoriaDatos datosCarta, int encontrados, int total)
    {
        if (datosCarta == null || datosCarta.integrantes == null)
        {
            return;
        }

        if (textoContador != null)
        {
            textoContador.text = "Par " + encontrados + " de " + total;
        }
        if (textoContenido != null)
        {
            textoContenido.text = ConstruirTexto(datosCarta);
        }

        if (rutina != null)
        {
            StopCoroutine(rutina);
        }
        rutina = StartCoroutine(Aparecer());
    }

    // Nombre en negrita, cargo debajo y una linea en blanco entre integrantes.
    private string ConstruirTexto(CartaMemoriaDatos datosCarta)
    {
        constructor.Clear();

        for (int i = 0; i < datosCarta.integrantes.Length; i++)
        {
            IntegranteJunta integrante = datosCarta.integrantes[i];
            if (i > 0)
            {
                constructor.Append("\n\n");
            }
            constructor.Append("<b>").Append(integrante.nombre).Append("</b>\n").Append(integrante.cargo);
        }

        return constructor.ToString();
    }

    // Parte de transparente y se hace visible: asi se nota el cambio aunque ya hubiera un par mostrado.
    private IEnumerator Aparecer()
    {
        grupo.alpha = 0f;

        float transcurrido = 0f;
        while (transcurrido < duracionFundido)
        {
            transcurrido += Time.deltaTime;
            grupo.alpha = Mathf.Clamp01(transcurrido / duracionFundido);
            yield return null;
        }

        grupo.alpha = 1f;
        rutina = null;
    }

    private void OcultarYa()
    {
        if (grupo == null)
        {
            return;
        }
        grupo.alpha = 0f;
    }
}