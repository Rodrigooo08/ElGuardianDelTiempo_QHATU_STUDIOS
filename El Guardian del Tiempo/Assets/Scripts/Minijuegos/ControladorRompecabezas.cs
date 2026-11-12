using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// Paso 4: ayuda con silueta temporal y sugerencia por inactividad.
public class ControladorRompecabezas : MonoBehaviour
{
    [Header("Identificación")]
    [Tooltip("Identificador único de este minijuego (para el registro de progreso).")]
    [SerializeField] private string idMinijuego = "N1_T1_Rompecabezas";

    [Header("Datos")]
    [SerializeField] private DatosRompecabezas datos;

    [Header("Apariencia de las piezas")]
    [Tooltip("Material con shader de sprites de URP 2D. Si queda vacío, se busca uno automáticamente.")]
    [SerializeField] private Material materialBase;

    [Header("Fondo de pantalla")]
    [SerializeField] private bool mostrarFondoPantalla = true;
    [Tooltip("Imagen opcional (debe estar importada como Sprite). Si queda vacía se usa un color liso.")]
    [SerializeField] private Sprite spriteFondoPantalla;
    [SerializeField] private Color colorFondoPantalla = new Color(0.25f, 0.18f, 0.1f, 1f);

    [Header("Marco del tablero")]
    [SerializeField] private bool mostrarMarco = true;
    [Tooltip("Fondo oscuro translúcido detrás del tablero.")]
    [SerializeField] private Color colorFondoMarco = new Color(0.12f, 0.08f, 0.04f, 0.5f);
    [Tooltip("Borde generado por código. Se usa si no hay imagen de marco.")]
    [SerializeField] private Color colorMarco = new Color(0.85f, 0.7f, 0.3f, 1f);
    [SerializeField] private float grosorMarco = 0.1f;
    [Tooltip("Imagen opcional de marco (debe estar importada como Sprite). Si se asigna, reemplaza el borde generado.")]
    [SerializeField] private Sprite spriteMarco;
    [Tooltip("Cuánto se agranda la imagen de marco respecto al tablero, de cada lado.")]
    [SerializeField] private float extraImagenMarco = 0.4f;

    [Header("Ayuda")]
    [Tooltip("Cuántas veces puede usar la ayuda el jugador.")]
    [SerializeField] private int usosAyuda = 2;
    [Tooltip("Segundos que la silueta se queda visible.")]
    [SerializeField] private float duracionSilueta = 3.5f;
    [Range(0f, 1f)]
    [Tooltip("Qué tan visible se ve la silueta (1 = opaca).")]
    [SerializeField] private float transparenciaSilueta = 0.5f;
    [Tooltip("Segundos sin tocar la pantalla para que el botón de ayuda empiece a brillar.")]
    [SerializeField] private float tiempoInactividad = 15f;

    [Header("Cámara")]
    [Tooltip("Si queda vacía se usa la Main Camera.")]
    [SerializeField] private Camera camaraJuego;
    [Tooltip("Ajusta el tamaño de la cámara para que entre todo. Apagar al integrar el minijuego en el nivel.")]
    [SerializeField] private bool ajustarCamara = true;
    [Tooltip("Espacio libre alrededor del tablero y de la zona de dispersión.")]
    [SerializeField] private float margenCamara = 1f;

    [Header("Dispersión inicial")]
    [Tooltip("Mitad del ancho y del alto del área donde se reparten las piezas.")]
    [SerializeField] private Vector2 areaDispersion = new Vector2(5.5f, 2.2f);

    [Header("Encaje")]
    [Tooltip("Distancia máxima al lugar correcto para que la pieza encaje.")]
    [SerializeField] private float distanciaEncaje = 0.6f;
    [Tooltip("Margen que las piezas deben respetar respecto al borde de la pantalla.")]
    [SerializeField] private float margenPantalla = 0.5f;

    // Avisan a otros sistemas (sonido, interfaz, ficha, nivel) sin que este script los conozca.
    public event System.Action<int, int> PiezaEncajada;   // encajadas, total
    public event System.Action Completado;
    public event System.Action Abandonado;
    public event System.Action<int> UsosAyudaCambiados;   // usos que quedan
    public event System.Action<bool> AyudaSugerida;       // true = que el botón brille

    public string IdMinijuego => idMinijuego;
    public int UsosAyudaRestantes => usosAyudaRestantes;

    private readonly List<PiezaRompecabezas> piezas = new List<PiezaRompecabezas>();

    private Vector2 tamanoTablero;
    private Texture2D texturaImagen;
    private PiezaRompecabezas piezaAgarrada;
    private Vector3 desplazamientoAgarre;
    private int ordenMaximo;
    private int encajadas;
    private bool completado;

    // Ayuda
    private int usosAyudaRestantes;
    private float tiempoSinActividad;
    private bool sugerenciaActiva;
    private Coroutine rutinaSilueta;
    private Mesh mallaSilueta;
    private Color[] coloresSilueta;

    private void Awake()
    {
        usosAyudaRestantes = usosAyuda;
        coloresSilueta = new Color[4];
    }

    // Si el minijuego se vuelve a activar después de abandonarlo, empieza de cero.
    private void OnEnable()
    {
        if (piezas.Count > 0)
        {
            Reiniciar();
        }
    }

    private void OnDisable()
    {
        DetenerSilueta();
    }

    private void Start()
    {
        if (datos == null)
        {
            Debug.LogError("Falta asignar los Datos del rompecabezas en el Inspector.", this);
            return;
        }

        if (camaraJuego == null)
        {
            camaraJuego = Camera.main;
        }
        if (camaraJuego == null)
        {
            Debug.LogError("No hay cámara. Asigná una en el Inspector o usá la Main Camera.", this);
            return;
        }
        if (!camaraJuego.orthographic)
        {
            Debug.LogWarning("La cámara no es ortográfica; los límites de pantalla no van a ser correctos.", this);
        }

        tamanoTablero = CalcularTamanoTablero();
        texturaImagen = datos.Imagen != null ? datos.Imagen : CrearTexturaDePrueba();

        if (ajustarCamara && camaraJuego.orthographic)
        {
            AjustarCamara();
        }
        if (mostrarFondoPantalla)
        {
            CrearFondoPantalla();
        }
        if (mostrarMarco)
        {
            CrearMarco();
        }

        CrearSilueta();
        CrearPiezas();
        DispersarPiezas();
    }

    private void Update()
    {
        if (completado || camaraJuego == null || piezas.Count == 0)
        {
            return;
        }

        ActualizarInactividad();

        Pointer puntero = Pointer.current;
        if (puntero == null)
        {
            return;
        }

        if (piezaAgarrada == null)
        {
            if (puntero.press.wasPressedThisFrame)
            {
                // Si el toque es sobre un botón de la interfaz, no se agarra ninguna pieza.
                if (PunteroSobreInterfaz())
                {
                    return;
                }
                IntentarAgarrar(PuntoEnMundo(puntero.position.ReadValue()));
            }
        }
        else if (puntero.press.isPressed)
        {
            Arrastrar(PuntoEnMundo(puntero.position.ReadValue()));
        }
        else
        {
            Soltar();
        }
    }

    private bool PunteroSobreInterfaz()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    // ---------- Ayuda ----------

    // Pensado para el botón "Ayuda": se conecta en el OnClick del botón.
    public void UsarAyuda()
    {
        if (completado || mallaSilueta == null)
        {
            return;
        }
        // Sin usos, o con la silueta ya visible, no se hace nada ni se gasta un uso.
        if (usosAyudaRestantes <= 0 || rutinaSilueta != null)
        {
            return;
        }

        usosAyudaRestantes--;
        UsosAyudaCambiados?.Invoke(usosAyudaRestantes);

        tiempoSinActividad = 0f;
        EstablecerSugerencia(false);
        rutinaSilueta = StartCoroutine(MostrarSilueta());
    }

    // Si el jugador no toca la pantalla durante un rato, el botón de ayuda empieza a brillar.
    private void ActualizarInactividad()
    {
        Pointer puntero = Pointer.current;
        bool hayActividad = piezaAgarrada != null || (puntero != null && puntero.press.isPressed);

        if (hayActividad)
        {
            tiempoSinActividad = 0f;
            EstablecerSugerencia(false);
            return;
        }

        if (usosAyudaRestantes <= 0 || rutinaSilueta != null)
        {
            EstablecerSugerencia(false);
            return;
        }

        tiempoSinActividad += Time.deltaTime;
        if (tiempoSinActividad >= tiempoInactividad)
        {
            EstablecerSugerencia(true);
        }
    }

    private void EstablecerSugerencia(bool activa)
    {
        if (sugerenciaActiva == activa)
        {
            return;
        }

        sugerenciaActiva = activa;
        AyudaSugerida?.Invoke(activa);
    }

    // Una corrutina es una función que puede "pausarse" (yield) y seguir después.
    // Acá: aparece la silueta, espera, y se desvanece.
    private IEnumerator MostrarSilueta()
    {
        yield return Desvanecer(0f, transparenciaSilueta, 0.3f);
        yield return new WaitForSeconds(duracionSilueta);
        yield return Desvanecer(transparenciaSilueta, 0f, 0.5f);
        rutinaSilueta = null;
    }

    private IEnumerator Desvanecer(float desde, float hasta, float duracion)
    {
        float transcurrido = 0f;
        while (transcurrido < duracion)
        {
            transcurrido += Time.deltaTime;
            EstablecerAlfaSilueta(Mathf.Lerp(desde, hasta, transcurrido / duracion));
            yield return null;
        }
        EstablecerAlfaSilueta(hasta);
    }

    private void EstablecerAlfaSilueta(float alfa)
    {
        if (mallaSilueta == null)
        {
            return;
        }

        Color color = new Color(1f, 1f, 1f, alfa);
        for (int i = 0; i < coloresSilueta.Length; i++)
        {
            coloresSilueta[i] = color;
        }
        mallaSilueta.colors = coloresSilueta;
    }

    private void DetenerSilueta()
    {
        if (rutinaSilueta != null)
        {
            StopCoroutine(rutinaSilueta);
            rutinaSilueta = null;
        }
        EstablecerAlfaSilueta(0f);
    }

    // ---------- Abandonar ----------

    // Pensado para el botón "Atrás": se conecta en el OnClick del botón.
    public void Abandonar()
    {
        piezaAgarrada = null;

        if (!completado)
        {
            Debug.Log("Minijuego abandonado: " + idMinijuego);
            Abandonado?.Invoke();
        }

        gameObject.SetActive(false);
    }

    // ---------- Tamaño del tablero y cámara ----------

    // Acomoda la imagen dentro de la caja máxima (ancho x alto de los datos)
    // sin estirarla: se respeta la proporción de la imagen.
    private Vector2 CalcularTamanoTablero()
    {
        Vector2 caja = new Vector2(datos.AnchoTablero, datos.AltoTablero);

        if (datos.Imagen == null || datos.Imagen.height <= 0)
        {
            return caja;
        }

        float proporcionImagen = (float)datos.Imagen.width / datos.Imagen.height;
        float proporcionCaja = caja.x / caja.y;

        if (proporcionImagen > proporcionCaja)
        {
            return new Vector2(caja.x, caja.x / proporcionImagen);
        }
        return new Vector2(caja.y * proporcionImagen, caja.y);
    }

    // Elige el tamaño de la cámara para que siempre entren el tablero y
    // la zona de dispersión, con cualquier forma de pantalla.
    private void AjustarCamara()
    {
        float mitadAnchoNecesaria = Mathf.Max(tamanoTablero.x * 0.5f, areaDispersion.x) + margenCamara;
        float mitadAltoNecesaria = Mathf.Max(tamanoTablero.y * 0.5f, areaDispersion.y) + margenCamara;

        float tamanoPorAlto = mitadAltoNecesaria;
        float tamanoPorAncho = mitadAnchoNecesaria / camaraJuego.aspect;
        camaraJuego.orthographicSize = Mathf.Max(tamanoPorAlto, tamanoPorAncho);
    }

    // ---------- Fondo de pantalla ----------

    // Capa que cubre toda la vista de la cámara. Se queda detrás de todo.
    private void CrearFondoPantalla()
    {
        if (!camaraJuego.orthographic)
        {
            return;
        }

        GameObject objetoFondo = new GameObject("FondoPantalla");
        objetoFondo.transform.SetParent(transform, false);

        SpriteRenderer renderizador = objetoFondo.AddComponent<SpriteRenderer>();
        renderizador.sortingOrder = -10;

        if (spriteFondoPantalla != null)
        {
            renderizador.sprite = spriteFondoPantalla;
        }
        else
        {
            Texture2D blanca = Texture2D.whiteTexture;
            renderizador.sprite = Sprite.Create(
                blanca,
                new Rect(0f, 0f, blanca.width, blanca.height),
                new Vector2(0.5f, 0.5f),
                blanca.width);
            renderizador.color = colorFondoPantalla;
        }

        // La imagen se agranda hasta cubrir toda la pantalla (sin dejar bordes vacíos).
        float alto = camaraJuego.orthographicSize * 2f;
        float ancho = alto * camaraJuego.aspect;
        Vector2 tamanoSprite = renderizador.sprite.bounds.size;
        float escala = Mathf.Max(ancho / tamanoSprite.x, alto / tamanoSprite.y) * 1.02f;

        Vector3 centroCamara = camaraJuego.transform.position;
        objetoFondo.transform.position = new Vector3(centroCamara.x, centroCamara.y, 0f);
        objetoFondo.transform.localScale = new Vector3(escala, escala, 1f);
    }

    // ---------- Marco, silueta y fondo del tablero ----------

    private void CrearMarco()
    {
        Shader shader = BuscarShaderSprites();
        if (shader == null)
        {
            Debug.LogWarning("No se pudo crear el marco: no se encontró un shader de sprites.", this);
            return;
        }

        GameObject objetoMarco = new GameObject("Marco");
        objetoMarco.transform.SetParent(transform, false);

        Material materialLiso = new Material(shader);
        CrearRectangulo(objetoMarco.transform, "FondoTablero", materialLiso, colorFondoMarco, -3);

        if (spriteMarco != null)
        {
            CrearMarcoConImagen(objetoMarco.transform);
        }
        else
        {
            CrearBordeGenerado(objetoMarco.transform, materialLiso);
        }
    }

    // La silueta empieza invisible; solo se muestra al usar la ayuda.
    private void CrearSilueta()
    {
        Shader shader = BuscarShaderSprites();
        if (shader == null)
        {
            Debug.LogWarning("No se pudo crear la silueta: no se encontró un shader de sprites.", this);
            return;
        }

        Material materialSilueta = new Material(shader);
        materialSilueta.mainTexture = texturaImagen;
        mallaSilueta = CrearRectangulo(transform, "Silueta", materialSilueta, new Color(1f, 1f, 1f, 0f), -2);
    }

    // Rectángulo del tamaño del tablero, con un color por vértice (y textura si el material la tiene).
    private Mesh CrearRectangulo(Transform padre, string nombre, Material material, Color color, int orden)
    {
        float mitadX = tamanoTablero.x * 0.5f;
        float mitadY = tamanoTablero.y * 0.5f;

        Mesh malla = new Mesh();
        malla.name = nombre;
        malla.vertices = new Vector3[]
        {
            new Vector3(-mitadX, -mitadY, 0f),
            new Vector3(-mitadX,  mitadY, 0f),
            new Vector3( mitadX,  mitadY, 0f),
            new Vector3( mitadX, -mitadY, 0f)
        };
        malla.uv = new Vector2[] { Vector2.zero, Vector2.up, Vector2.one, Vector2.right };
        malla.colors = new Color[] { color, color, color, color };
        malla.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
        malla.RecalculateBounds();

        GameObject objeto = new GameObject(nombre);
        objeto.transform.SetParent(padre, false);

        MeshFilter filtro = objeto.AddComponent<MeshFilter>();
        filtro.sharedMesh = malla;

        MeshRenderer renderizador = objeto.AddComponent<MeshRenderer>();
        renderizador.sharedMaterial = material;
        renderizador.sortingOrder = orden;

        return malla;
    }

    private void CrearBordeGenerado(Transform padre, Material material)
    {
        GameObject objetoBorde = new GameObject("Borde");
        objetoBorde.transform.SetParent(padre, false);

        float bordeX = tamanoTablero.x * 0.5f + grosorMarco * 0.5f;
        float bordeY = tamanoTablero.y * 0.5f + grosorMarco * 0.5f;

        LineRenderer linea = objetoBorde.AddComponent<LineRenderer>();
        linea.useWorldSpace = false;
        linea.loop = true;
        linea.positionCount = 4;
        linea.SetPositions(new Vector3[]
        {
            new Vector3(-bordeX, -bordeY, 0f),
            new Vector3(-bordeX,  bordeY, 0f),
            new Vector3( bordeX,  bordeY, 0f),
            new Vector3( bordeX, -bordeY, 0f)
        });
        linea.startWidth = grosorMarco;
        linea.endWidth = grosorMarco;
        linea.startColor = colorMarco;
        linea.endColor = colorMarco;
        linea.numCornerVertices = 2;
        linea.sharedMaterial = material;
        linea.sortingOrder = -1;
    }

    // Marco dibujado por el diseñador: se estira para rodear el tablero.
    private void CrearMarcoConImagen(Transform padre)
    {
        GameObject objetoImagen = new GameObject("MarcoImagen");
        objetoImagen.transform.SetParent(padre, false);

        SpriteRenderer renderizador = objetoImagen.AddComponent<SpriteRenderer>();
        renderizador.sprite = spriteMarco;
        renderizador.sortingOrder = -1;

        Vector2 tamanoSprite = spriteMarco.bounds.size;
        float anchoFinal = tamanoTablero.x + extraImagenMarco * 2f;
        float altoFinal = tamanoTablero.y + extraImagenMarco * 2f;
        objetoImagen.transform.localScale = new Vector3(anchoFinal / tamanoSprite.x, altoFinal / tamanoSprite.y, 1f);
    }

    private Shader BuscarShaderSprites()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }
        return shader;
    }

    // ---------- Creación de piezas ----------

    private void CrearPiezas()
    {
        Material material = ObtenerMaterial();
        if (material == null)
        {
            return;
        }

        for (int i = 0; i < datos.Piezas.Count; i++)
        {
            Vector2[] puntos = datos.Piezas[i].Puntos;
            if (puntos == null || puntos.Length < 3)
            {
                Debug.LogWarning("La pieza " + (i + 1) + " tiene menos de 3 puntos y se omite.", this);
                continue;
            }

            Mesh malla = CrearMalla(puntos, out Vector2[] puntosLocales, out Vector2 centro);

            GameObject objetoPieza = new GameObject("Pieza_" + (i + 1));
            objetoPieza.transform.SetParent(transform, false);

            MeshFilter filtro = objetoPieza.AddComponent<MeshFilter>();
            filtro.sharedMesh = malla;

            MeshRenderer renderizador = objetoPieza.AddComponent<MeshRenderer>();
            renderizador.sharedMaterial = material;

            PolygonCollider2D colision = objetoPieza.AddComponent<PolygonCollider2D>();
            colision.pathCount = 1;
            colision.SetPath(0, puntosLocales);

            PiezaRompecabezas pieza = objetoPieza.AddComponent<PiezaRompecabezas>();
            pieza.Inicializar(centro, renderizador, colision);
            piezas.Add(pieza);
        }
    }

    // Convierte una forma (0..1) en una malla del tamaño del tablero.
    private Mesh CrearMalla(Vector2[] forma, out Vector2[] puntosLocales, out Vector2 centro)
    {
        int cantidad = forma.Length;
        Vector3[] vertices = new Vector3[cantidad];
        Vector2[] uv = new Vector2[cantidad];
        puntosLocales = new Vector2[cantidad];
        centro = Vector2.zero;

        for (int i = 0; i < cantidad; i++)
        {
            float x = (forma[i].x - 0.5f) * tamanoTablero.x;
            float y = (forma[i].y - 0.5f) * tamanoTablero.y;
            vertices[i] = new Vector3(x, y, 0f);
            puntosLocales[i] = new Vector2(x, y);
            uv[i] = forma[i];
            centro += puntosLocales[i];
        }
        centro /= cantidad;

        // Triángulos en abanico desde el punto 0. Unity espera el sentido horario.
        int[] triangulos = new int[(cantidad - 2) * 3];
        for (int i = 0; i < cantidad - 2; i++)
        {
            triangulos[i * 3] = 0;
            triangulos[i * 3 + 1] = i + 2;
            triangulos[i * 3 + 2] = i + 1;
        }

        Mesh malla = new Mesh();
        malla.name = "MallaPieza";
        malla.vertices = vertices;
        malla.uv = uv;
        malla.triangles = triangulos;
        malla.RecalculateBounds();
        return malla;
    }

    private Material ObtenerMaterial()
    {
        Material material;

        if (materialBase != null)
        {
            material = new Material(materialBase);
        }
        else
        {
            Shader shader = BuscarShaderSprites();
            if (shader == null)
            {
                Debug.LogError("No se encontró un shader de sprites. Asigná un material en el Inspector.", this);
                return null;
            }
            material = new Material(shader);
        }

        material.mainTexture = texturaImagen;
        return material;
    }

    // Imagen de prueba si los datos no tienen una asignada.
    private Texture2D CrearTexturaDePrueba()
    {
        const int ancho = 512;
        const int alto = 384;
        Texture2D textura = new Texture2D(ancho, alto, TextureFormat.RGBA32, false);

        for (int y = 0; y < alto; y++)
        {
            for (int x = 0; x < ancho; x++)
            {
                float u = (float)x / (ancho - 1);
                float v = (float)y / (alto - 1);
                Color color = new Color(u, v, 1f - u, 1f);

                if (x % 64 < 3 || y % 64 < 3)
                {
                    color = Color.white;
                }
                textura.SetPixel(x, y, color);
            }
        }

        textura.wrapMode = TextureWrapMode.Clamp;
        textura.filterMode = FilterMode.Bilinear;
        textura.Apply();
        return textura;
    }

    // ---------- Dispersión y reinicio ----------

    private void DispersarPiezas()
    {
        for (int i = 0; i < piezas.Count; i++)
        {
            Vector2 desplazamiento = Vector2.zero;

            // Se intenta que la pieza no quede casi en su lugar correcto.
            for (int intento = 0; intento < 10; intento++)
            {
                Vector2 nuevoCentro = new Vector2(
                    Random.Range(-areaDispersion.x, areaDispersion.x),
                    Random.Range(-areaDispersion.y, areaDispersion.y));
                desplazamiento = nuevoCentro - piezas[i].CentroLocal;

                if (desplazamiento.magnitude >= distanciaEncaje * 2f)
                {
                    break;
                }
            }

            piezas[i].transform.localPosition = new Vector3(desplazamiento.x, desplazamiento.y, 0f);
            piezas[i].Renderizador.sortingOrder = i + 1;
        }

        ordenMaximo = piezas.Count;
    }

    [ContextMenu("Reiniciar rompecabezas")]
    public void Reiniciar()
    {
        if (piezas.Count == 0)
        {
            return;
        }

        piezaAgarrada = null;
        encajadas = 0;
        completado = false;

        DetenerSilueta();
        usosAyudaRestantes = usosAyuda;
        UsosAyudaCambiados?.Invoke(usosAyudaRestantes);
        tiempoSinActividad = 0f;
        EstablecerSugerencia(false);

        foreach (PiezaRompecabezas pieza in piezas)
        {
            pieza.Liberar();
        }
        DispersarPiezas();
    }

    // ---------- Arrastre ----------

    private Vector3 PuntoEnMundo(Vector2 posicionPantalla)
    {
        Vector3 punto = new Vector3(posicionPantalla.x, posicionPantalla.y, -camaraJuego.transform.position.z);
        Vector3 mundo = camaraJuego.ScreenToWorldPoint(punto);
        mundo.z = 0f;
        return mundo;
    }

    private void IntentarAgarrar(Vector3 puntoMundo)
    {
        // Los colliders se mueven con el transform; aseguramos que estén actualizados.
        Physics2D.SyncTransforms();
        Collider2D[] colisiones = Physics2D.OverlapPointAll(puntoMundo);

        PiezaRompecabezas elegida = null;
        int mejorOrden = int.MinValue;

        foreach (Collider2D colision in colisiones)
        {
            PiezaRompecabezas pieza = colision.GetComponent<PiezaRompecabezas>();
            if (pieza == null || pieza.Encajada)
            {
                continue;
            }

            if (pieza.Renderizador.sortingOrder > mejorOrden)
            {
                mejorOrden = pieza.Renderizador.sortingOrder;
                elegida = pieza;
            }
        }

        if (elegida == null)
        {
            return;
        }

        piezaAgarrada = elegida;
        desplazamientoAgarre = elegida.transform.position - puntoMundo;
        ordenMaximo++;
        elegida.Renderizador.sortingOrder = ordenMaximo;
    }

    private void Arrastrar(Vector3 puntoMundo)
    {
        Transform t = piezaAgarrada.transform;
        Vector3 posicionDeseada = puntoMundo + desplazamientoAgarre;
        posicionDeseada.z = t.position.z;

        // Dónde quedaría el centro de la pieza, y se limita al área visible.
        Vector3 centroActual = t.TransformPoint(new Vector3(piezaAgarrada.CentroLocal.x, piezaAgarrada.CentroLocal.y, 0f));
        Vector3 centroDeseado = centroActual + (posicionDeseada - t.position);
        Vector3 centroLimitado = LimitarAPantalla(centroDeseado);

        t.position = posicionDeseada + (centroLimitado - centroDeseado);
    }

    private Vector3 LimitarAPantalla(Vector3 punto)
    {
        float mitadAlto = camaraJuego.orthographicSize;
        float mitadAncho = mitadAlto * camaraJuego.aspect;
        float limiteX = Mathf.Max(0f, mitadAncho - margenPantalla);
        float limiteY = Mathf.Max(0f, mitadAlto - margenPantalla);

        Vector3 centroCamara = camaraJuego.transform.position;
        punto.x = Mathf.Clamp(punto.x, centroCamara.x - limiteX, centroCamara.x + limiteX);
        punto.y = Mathf.Clamp(punto.y, centroCamara.y - limiteY, centroCamara.y + limiteY);
        return punto;
    }

    private void Soltar()
    {
        PiezaRompecabezas pieza = piezaAgarrada;
        piezaAgarrada = null;

        Vector2 posicionLocal = pieza.transform.localPosition;
        if (posicionLocal.magnitude <= distanciaEncaje)
        {
            Encajar(pieza);
        }
    }

    private void Encajar(PiezaRompecabezas pieza)
    {
        pieza.transform.localPosition = Vector3.zero;
        pieza.MarcarEncajada();
        pieza.Renderizador.sortingOrder = 0;
        encajadas++;

        PiezaEncajada?.Invoke(encajadas, piezas.Count);

        if (encajadas >= piezas.Count)
        {
            completado = true;
            DetenerSilueta();
            EstablecerSugerencia(false);
            Debug.Log("¡Rompecabezas completado!");
            Completado?.Invoke();
        }
    }
}