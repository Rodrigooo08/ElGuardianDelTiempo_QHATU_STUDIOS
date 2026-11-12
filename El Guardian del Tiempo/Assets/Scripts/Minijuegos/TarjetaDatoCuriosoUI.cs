using System.Collections;
using TMPro;
using UnityEngine;

// Tarjeta con el dato curioso que aparece al completar el rompecabezas.
// Este objeto tiene que estar siempre activo; se oculta con el CanvasGroup (alpha 0).
[RequireComponent(typeof(CanvasGroup))]
public class TarjetaDatoCuriosoUI : MonoBehaviour
{
    [SerializeField] private ControladorRompecabezas controlador;
    [SerializeField] private TMP_Text textoTitulo;
    [SerializeField] private TMP_Text textoCuerpo;
    [Tooltip("Panel de la tarjeta que se agranda un poco al aparecer (opcional).")]
    [SerializeField] private RectTransform contenido;

    [Header("Tiempos")]
    [Tooltip("Segundos de espera tras completar, para que el jugador vea la imagen terminada.")]
    [SerializeField] private float retrasoAparicion = 0.8f;
    [SerializeField] private float duracionFundido = 0.3f;
    [SerializeField] private float escalaInicial = 0.9f;

    private CanvasGroup grupo;
    private Coroutine rutina;

    private void Awake()
    {
        grupo = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        OcultarYa();

        if (controlador == null)
        {
            Debug.LogWarning("Falta asignar el Controlador en la tarjeta del dato curioso.", this);
            return;
        }
        controlador.Completado += AlCompletar;
    }

    private void OnDisable()
    {
        if (controlador != null)
        {
            controlador.Completado -= AlCompletar;
        }

        if (rutina != null)
        {
            StopCoroutine(rutina);
            rutina = null;
        }
        OcultarYa();
    }

    // Pensado para el botón "Continuar": se conecta en su OnClick.
    public void Continuar()
    {
        if (controlador != null)
        {
            controlador.Cerrar();
        }
    }

    private void AlCompletar()
    {
        if (rutina != null)
        {
            StopCoroutine(rutina);
        }
        rutina = StartCoroutine(Mostrar());
    }

    private IEnumerator Mostrar()
    {
        yield return new WaitForSeconds(retrasoAparicion);

        DatosRompecabezas datos = controlador.Datos;
        if (datos != null)
        {
            if (textoTitulo != null)
            {
                textoTitulo.text = datos.TituloDato;
            }
            if (textoCuerpo != null)
            {
                textoCuerpo.text = datos.TextoDato;
            }
        }

        // Desde acá la tarjeta tapa la pantalla y recibe los clics.
        grupo.blocksRaycasts = true;
        grupo.interactable = true;

        float transcurrido = 0f;
        while (transcurrido < duracionFundido)
        {
            transcurrido += Time.deltaTime;
            float progreso = Mathf.Clamp01(transcurrido / duracionFundido);
            grupo.alpha = progreso;
            if (contenido != null)
            {
                contenido.localScale = Vector3.one * Mathf.Lerp(escalaInicial, 1f, progreso);
            }
            yield return null;
        }

        grupo.alpha = 1f;
        if (contenido != null)
        {
            contenido.localScale = Vector3.one;
        }
        rutina = null;
    }

    private void OcultarYa()
    {
        if (grupo == null)
        {
            return;
        }

        grupo.alpha = 0f;
        grupo.interactable = false;
        grupo.blocksRaycasts = false;
    }
}