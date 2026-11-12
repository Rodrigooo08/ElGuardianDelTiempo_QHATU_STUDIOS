using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Parte visual del botón de ayuda: muestra los usos restantes y brilla
// cuando el controlador sugiere usar la ayuda. La lógica está en ControladorRompecabezas.
[RequireComponent(typeof(Button))]
public class BotonAyudaRompecabezas : MonoBehaviour
{
    [SerializeField] private ControladorRompecabezas controlador;
    [Tooltip("Texto del botón (el TextMeshPro hijo).")]
    [SerializeField] private TMP_Text textoBoton;
    [Tooltip("Imagen que se ilumina. Si queda vacía se usa la del propio botón.")]
    [SerializeField] private Image imagenFondo;

    [Header("Brillo")]
    [SerializeField] private Color colorBrillo = new Color(1f, 0.9f, 0.4f, 1f);
    [SerializeField] private float velocidadPulso = 3f;
    [SerializeField] private float aumentoEscala = 0.08f;

    private Button boton;
    private Color colorOriginal;
    private Vector3 escalaOriginal;
    private bool brillando;

    private void Awake()
    {
        boton = GetComponent<Button>();
        if (imagenFondo == null)
        {
            imagenFondo = GetComponent<Image>();
        }

        escalaOriginal = transform.localScale;
        if (imagenFondo != null)
        {
            colorOriginal = imagenFondo.color;
        }
    }

    private void OnEnable()
    {
        if (controlador == null)
        {
            Debug.LogWarning("Falta asignar el Controlador en el botón de ayuda.", this);
            return;
        }

        controlador.UsosAyudaCambiados += AlCambiarUsos;
        controlador.AyudaSugerida += AlSugerirAyuda;
        AlCambiarUsos(controlador.UsosAyudaRestantes);
    }

    private void Start()
    {
        // En Start todos los Awake ya corrieron, así el valor inicial es el correcto.
        if (controlador != null)
        {
            AlCambiarUsos(controlador.UsosAyudaRestantes);
        }
    }

    private void OnDisable()
    {
        if (controlador != null)
        {
            controlador.UsosAyudaCambiados -= AlCambiarUsos;
            controlador.AyudaSugerida -= AlSugerirAyuda;
        }
        AlSugerirAyuda(false);
    }

    private void Update()
    {
        if (!brillando || imagenFondo == null)
        {
            return;
        }

        // Valor que sube y baja entre 0 y 1.
        float pulso = (Mathf.Sin(Time.unscaledTime * velocidadPulso) + 1f) * 0.5f;
        imagenFondo.color = Color.Lerp(colorOriginal, colorBrillo, pulso);
        transform.localScale = escalaOriginal * (1f + aumentoEscala * pulso);
    }

    private void AlCambiarUsos(int usosRestantes)
    {
        if (textoBoton != null)
        {
            textoBoton.text = "Ayuda (" + usosRestantes + ")";
        }
        boton.interactable = usosRestantes > 0;
    }

    private void AlSugerirAyuda(bool activar)
    {
        brillando = activar;

        if (!activar)
        {
            // Se vuelve al aspecto normal del botón.
            transform.localScale = escalaOriginal;
            if (imagenFondo != null)
            {
                imagenFondo.color = colorOriginal;
            }
        }
    }
}