using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public enum EstadoPuesto
{
    Libre,
    Asomando,
    Visible,
    Entregado,
    Perdido
}

[RequireComponent(typeof(BoxCollider2D))]
public class PuestoReparto : MonoBehaviour, IPointerClickHandler
{
    [Header("Referencias")]
    [SerializeField] private Transform vecino;
    [SerializeField] private SpriteRenderer spriteVecino;
    [SerializeField] private SpriteRenderer spriteGlobito;

    [Header("Colores provisorios del globito")]
    [SerializeField] private Color colorCinta = new Color(0.45f, 0.75f, 1f);
    [SerializeField] private Color colorParaguas = new Color(0.6f, 0.6f, 0.6f);

    [Header("Animación (segundos)")]
    [SerializeField] private float duracionAsomar = 0.25f;
    [SerializeField] private float duracionEntrega = 0.3f;
    [SerializeField] private float duracionSalida = 0.25f;
    [SerializeField] private float desplazamientoAsomar = 0.4f;

    [Header("Depuración")]
    [SerializeField] private bool mostrarMensajes = true;

    // Avisan al controlador. El puesto no sabe nada de puntajes ni de niveles.
    public event Action<PuestoReparto, ObjetoReparto> AlEntregar;
    public event Action<PuestoReparto> AlPerder;

    public EstadoPuesto Estado { get; private set; } = EstadoPuesto.Libre;
    public ObjetoReparto ObjetoPedido { get; private set; }
    public bool EstaLibre => Estado == EstadoPuesto.Libre;

    private Vector3 escalaBase;
    private Vector3 posicionBase;
    private Color colorOriginalVecino;
    private Coroutine rutinaActual;

    private void Awake()
    {
        if (vecino == null || spriteVecino == null || spriteGlobito == null)
        {
            Debug.LogError("PuestoReparto: faltan referencias en el Inspector.", this);
            enabled = false;
            return;
        }

        escalaBase = vecino.localScale;
        posicionBase = vecino.localPosition;
        colorOriginalVecino = spriteVecino.color;
        OcultarTodo();
    }

    /// <summary>
    /// Hace asomar un vecino. Devuelve false si el puesto está ocupado.
    /// </summary>
    public bool Mostrar(TipoVecino tipo, float segundosVisible)
    {
        if (!enabled || !EstaLibre) return false;

        ObjetoPedido = tipo.objetoQuePide;
        if (tipo.imagen != null) spriteVecino.sprite = tipo.imagen;
        spriteGlobito.color = ObjetoPedido == ObjetoReparto.Cinta ? colorCinta : colorParaguas;

        rutinaActual = StartCoroutine(CicloDeVida(segundosVisible));
        return true;
    }

    public void OnPointerClick(PointerEventData datos)
    {
        // Solo cuenta si hay un vecino asomado o visible. Evita dobles entregas.
        if (Estado != EstadoPuesto.Asomando && Estado != EstadoPuesto.Visible) return;

        if (rutinaActual != null) StopCoroutine(rutinaActual);
        rutinaActual = StartCoroutine(Entrega());
    }

    private IEnumerator CicloDeVida(float segundosVisible)
    {
        // Asomar: sube un poco y crece desde cero
        Estado = EstadoPuesto.Asomando;
        vecino.gameObject.SetActive(true);
        spriteGlobito.gameObject.SetActive(false);
        PonerAlfa(1f);

        float t = 0f;
        while (t < 1f)
        {
            t = Mathf.Min(1f, t + Time.deltaTime / duracionAsomar);
            float suave = Mathf.SmoothStep(0f, 1f, t);
            vecino.localScale = escalaBase * suave;
            vecino.localPosition = posicionBase + Vector3.down * desplazamientoAsomar * (1f - suave);
            yield return null;
        }

        // Visible: muestra el globito y espera
        Estado = EstadoPuesto.Visible;
        spriteGlobito.gameObject.SetActive(true);
        yield return new WaitForSeconds(segundosVisible);

        // Perdido: se va sin castigo
        Estado = EstadoPuesto.Perdido;
        spriteGlobito.gameObject.SetActive(false);
        if (mostrarMensajes) Debug.Log($"[{name}] Perdido");
        AlPerder?.Invoke(this);

        t = 0f;
        while (t < 1f)
        {
            t = Mathf.Min(1f, t + Time.deltaTime / duracionSalida);
            PonerAlfa(1f - t);
            yield return null;
        }

        OcultarTodo();
    }

    private IEnumerator Entrega()
    {
        Estado = EstadoPuesto.Entregado;
        spriteGlobito.gameObject.SetActive(false);
        if (mostrarMensajes) Debug.Log($"[{name}] Entregado: {ObjetoPedido}");

        // Se avisa de inmediato, antes de la animación, para que el puntaje no espere.
        AlEntregar?.Invoke(this, ObjetoPedido);

        // Rebote: crece un poco y se achica hasta desaparecer
        vecino.localPosition = posicionBase;
        float t = 0f;
        while (t < 1f)
        {
            t = Mathf.Min(1f, t + Time.deltaTime / duracionEntrega);
            float factor = t < 0.4f
                ? Mathf.Lerp(1f, 1.15f, t / 0.4f)
                : Mathf.Lerp(1.15f, 0f, (t - 0.4f) / 0.6f);
            vecino.localScale = escalaBase * factor;
            yield return null;
        }

        OcultarTodo();
    }

    private void OcultarTodo()
    {
        vecino.gameObject.SetActive(false);
        spriteGlobito.gameObject.SetActive(false);
        vecino.localScale = escalaBase;
        vecino.localPosition = posicionBase;
        PonerAlfa(1f);
        rutinaActual = null;
        Estado = EstadoPuesto.Libre;
    }

    private void PonerAlfa(float alfa)
    {
        Color c = colorOriginalVecino;
        c.a = alfa;
        spriteVecino.color = c;
    }

    // Aparecen en el menú de los tres puntitos del componente, solo con el juego en Play.
    [ContextMenu("Probar aparición (cinta)")]
    private void ProbarCinta()
    {
        if (!Application.isPlaying) return;
        Mostrar(new TipoVecino { nombre = "Prueba", objetoQuePide = ObjetoReparto.Cinta }, 3f);
    }

    [ContextMenu("Probar aparición (paraguas)")]
    private void ProbarParaguas()
    {
        if (!Application.isPlaying) return;
        Mostrar(new TipoVecino { nombre = "Prueba", objetoQuePide = ObjetoReparto.Paraguas }, 3f);
    }
}