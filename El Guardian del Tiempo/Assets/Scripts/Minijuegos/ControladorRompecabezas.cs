using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Paso 3b: tamaño de tablero fijo, cámara ajustada y marco visible.
public class ControladorRompecabezas : MonoBehaviour
{
    [Header("Datos")]
    [SerializeField] private DatosRompecabezas datos;

    [Header("Apariencia")]
    [Tooltip("Material con shader de sprites de URP 2D. Si queda vacío, se busca uno automáticamente.")]
    [SerializeField] private Material materialBase;

    [Header("Marco del tablero")]
    [SerializeField] private bool mostrarMarco = true;
    [SerializeField] private Color colorMarco = new Color(0.85f, 0.7f, 0.3f, 1f);
    [SerializeField] private Color colorFondoMarco = new Color(0.12f, 0.08f, 0.04f, 0.5f);
    [SerializeField] private float grosorMarco = 0.1f;

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

    // Avisan a otros sistemas (sonido, interfaz, ficha) sin que este script los conozca.
    public event System.Action<int, int> PiezaEncajada;   // encajadas, total
    public event System.Action Completado;

    private readonly List<PiezaRompecabezas> piezas = new List<PiezaRompecabezas>();

    private Vector2 tamanoTablero;
    private PiezaRompecabezas piezaAgarrada;
    private Vector3 desplazamientoAgarre;
    private int ordenMaximo;
    private int encajadas;
    private bool completado;

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

        if (ajustarCamara && camaraJuego.orthographic)
        {
            AjustarCamara();
        }
        if (mostrarMarco)
        {
            CrearMarco();
        }

        CrearPiezas();
        DispersarPiezas();
    }

    private void Update()
    {
        if (completado || camaraJuego == null || piezas.Count == 0)
        {
            return;
        }

        Pointer puntero = Pointer.current;
        if (puntero == null)
        {
            return;
        }

        if (piezaAgarrada == null)
        {
            if (puntero.press.wasPressedThisFrame)
            {
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

    // ---------- Marco ----------

    private void CrearMarco()
    {
        Shader shader = BuscarShaderSprites();
        if (shader == null)
        {
            Debug.LogWarning("No se pudo crear el marco: no se encontró un shader de sprites.", this);
            return;
        }

        Material materialMarco = new Material(shader);
        float mitadX = tamanoTablero.x * 0.5f;
        float mitadY = tamanoTablero.y * 0.5f;

        GameObject objetoMarco = new GameObject("Marco");
        objetoMarco.transform.SetParent(transform, false);

        // Fondo translúcido (un rectángulo con color por vértice).
        Mesh fondo = new Mesh();
        fondo.name = "FondoMarco";
        fondo.vertices = new Vector3[]
        {
            new Vector3(-mitadX, -mitadY, 0f),
            new Vector3(-mitadX,  mitadY, 0f),
            new Vector3( mitadX,  mitadY, 0f),
            new Vector3( mitadX, -mitadY, 0f)
        };
        fondo.uv = new Vector2[] { Vector2.zero, Vector2.up, Vector2.one, Vector2.right };
        fondo.colors = new Color[] { colorFondoMarco, colorFondoMarco, colorFondoMarco, colorFondoMarco };
        fondo.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
        fondo.RecalculateBounds();

        MeshFilter filtroFondo = objetoMarco.AddComponent<MeshFilter>();
        filtroFondo.sharedMesh = fondo;
        MeshRenderer renderizadorFondo = objetoMarco.AddComponent<MeshRenderer>();
        renderizadorFondo.sharedMaterial = materialMarco;
        renderizadorFondo.sortingOrder = -2;

        // Borde dorado alrededor del tablero.
        GameObject objetoBorde = new GameObject("Borde");
        objetoBorde.transform.SetParent(objetoMarco.transform, false);

        float bordeX = mitadX + grosorMarco * 0.5f;
        float bordeY = mitadY + grosorMarco * 0.5f;

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
        linea.sharedMaterial = materialMarco;
        linea.sortingOrder = -1;
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

        material.mainTexture = datos.Imagen != null ? datos.Imagen : CrearTexturaDePrueba();
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
            Debug.Log("¡Rompecabezas completado!");
            Completado?.Invoke();
        }
    }
}