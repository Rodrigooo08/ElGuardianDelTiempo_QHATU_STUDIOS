using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Una carta del tablero de memoria. Se da vuelta con una animacion corta
// (se achica en horizontal, cambia de lado y se agranda).
// No decide nada del juego: solo avisa cuando el jugador la toca.
[RequireComponent(typeof(Button))]
public class CartaMemoria : MonoBehaviour
{
    [Header("Dorso (boca abajo)")]
    [SerializeField] private Image imagenDorso;
    [Tooltip("Color del dorso si no hay imagen de reverso en los datos.")]
    [SerializeField] private Color colorDorsoSinImagen = new Color(0.35f, 0.22f, 0.12f, 1f);

    [Header("Cara (boca arriba)")]
    [Tooltip("Imagen de fondo de la cara. Es el objeto 'Cara', que contiene al retrato y a los nombres.")]
    [SerializeField] private Image fondoCara;
    [SerializeField] private Image imagenRetrato;
    [SerializeField] private TMP_Text textoNombres;
    [Tooltip("Color del fondo de la cara cuando la carta ya fue emparejada.")]
    [SerializeField] private Color colorEmparejada = new Color(0.78f, 0.9f, 0.7f, 1f);

    [Header("Animacion")]
    [Tooltip("Segundos que tarda en achicarse (y lo mismo en agrandarse). El giro completo dura el doble.")]
    [SerializeField] private float duracionMitad = 0.12f;
    [Tooltip("Escala horizontal minima durante el giro (no llega a 0 para evitar problemas de interfaz).")]
    [SerializeField] private float escalaMinima = 0.02f;

    // Avisa que el jugador tocó esta carta (solo si se puede dar vuelta).
    public event Action<CartaMemoria> Pulsada;

    public string Id => id;
    public bool BocaArriba => bocaArriba;
    public bool Emparejada => emparejada;
    public bool EnAnimacion => enAnimacion;

    private Button boton;
    private Color colorCaraNormal;
    private string id;
    private bool bocaArriba;
    private bool emparejada;
    private bool enAnimacion;
    private Coroutine rutina;
    private RectTransform rect;

    private void Awake()
    {
        boton = GetComponent<Button>();
        rect = (RectTransform)transform;
        if (fondoCara != null)
        {
            colorCaraNormal = fondoCara.color;
        }
        boton.onClick.AddListener(AlHacerClic);
    }

    private void OnDestroy()
    {
        if (boton != null)
        {
            boton.onClick.RemoveListener(AlHacerClic);
        }
    }

    // Si el objeto se desactiva en medio del giro (ej: botón Atrás), la carta
    // no puede quedar a medias: se deja en su estado final.
    private void OnDisable()
    {
        if (rutina != null)
        {
            StopCoroutine(rutina);
            rutina = null;
        }
        enAnimacion = false;
        rect.localScale = Vector3.one;
        MostrarLado(bocaArriba);
    }

    // Prepara la carta con sus datos. Siempre queda boca abajo y sin emparejar.
    public void Configurar(CartaMemoriaDatos datos, Sprite reverso)
    {
        if (datos == null)
        {
            Debug.LogError("CartaMemoria: se intentó configurar con datos vacíos.", this);
            return;
        }

        id = datos.id;

        imagenDorso.sprite = reverso;
        imagenDorso.color = reverso != null ? Color.white : colorDorsoSinImagen;

        bool hayImagen = datos.imagen != null;
        imagenRetrato.gameObject.SetActive(hayImagen);
        if (hayImagen)
        {
            imagenRetrato.sprite = datos.imagen;
        }

        textoNombres.text = ConstruirNombres(datos);

        Reiniciar();
    }

    // Da vuelta la carta. Si ya está en ese estado o está girando, no hace nada.
    public void Voltear(bool alFrente)
    {
        if (enAnimacion)
        {
            Debug.LogWarning("CartaMemoria: se pidió voltear una carta que todavía está girando.", this);
            return;
        }
        if (alFrente == bocaArriba)
        {
            return;
        }

        // El estado lógico cambia ya; la animación es solo visual.
        bocaArriba = alFrente;
        rutina = StartCoroutine(Girar(alFrente));
    }

    // La carta queda a la vista y ya no se puede tocar.
    public void MarcarEmparejada()
    {
        emparejada = true;
        boton.interactable = false;
        if (fondoCara != null)
        {
            fondoCara.color = colorEmparejada;
        }
    }

    // Vuelve al estado inicial: boca abajo, sin emparejar y tocable.
    public void Reiniciar()
    {
        if (rutina != null)
        {
            StopCoroutine(rutina);
            rutina = null;
        }

        enAnimacion = false;
        bocaArriba = false;
        emparejada = false;
        boton.interactable = true;
        rect.localScale = Vector3.one;
        if (fondoCara != null)
        {
            fondoCara.color = colorCaraNormal;
        }
        MostrarLado(false);
    }

    private void AlHacerClic()
    {
        if (emparejada || bocaArriba || enAnimacion)
        {
            return;
        }
        Pulsada?.Invoke(this);
    }

    private IEnumerator Girar(bool alFrente)
    {
        enAnimacion = true;

        yield return Escalar(1f, escalaMinima);
        MostrarLado(alFrente);
        yield return Escalar(escalaMinima, 1f);

        enAnimacion = false;
        rutina = null;
    }

    private IEnumerator Escalar(float desde, float hasta)
    {
        float transcurrido = 0f;
        while (transcurrido < duracionMitad)
        {
            transcurrido += Time.deltaTime;
            float progreso = Mathf.Clamp01(transcurrido / duracionMitad);
            rect.localScale = new Vector3(Mathf.Lerp(desde, hasta, progreso), 1f, 1f);
            yield return null;
        }
        rect.localScale = new Vector3(hasta, 1f, 1f);
    }

    private void MostrarLado(bool alFrente)
    {
        imagenDorso.gameObject.SetActive(!alFrente);
        fondoCara.gameObject.SetActive(alFrente);
    }

    private static string ConstruirNombres(CartaMemoriaDatos datos)
    {
        if (datos.integrantes == null || datos.integrantes.Length == 0)
        {
            return string.Empty;
        }

        StringBuilder texto = new StringBuilder();
        for (int i = 0; i < datos.integrantes.Length; i++)
        {
            if (i > 0)
            {
                texto.Append('\n');
            }
            texto.Append(datos.integrantes[i].nombre);
        }
        return texto.ToString();
    }
}