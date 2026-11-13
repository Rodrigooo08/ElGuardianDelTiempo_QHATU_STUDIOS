using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Logica del minijuego de memoria de pares (Tramo 2, Nivel 1).
// Crea las cartas a partir de los datos, las mezcla y decide si dos cartas forman un par.
// Avisa con eventos para que otros sistemas (interfaz, sonido, nivel) reaccionen sin que este script los conozca.
public class ControladorMemoria : MonoBehaviour
{
    [Header("Identificacion")]
    [Tooltip("Identificador unico de este minijuego (para el registro de progreso).")]
    [SerializeField] private string idMinijuego = "N1_T2_Memoria";

    [Header("Datos")]
    [SerializeField] private DatosMemoria datos;

    [Header("Tablero")]
    [Tooltip("Prefab de la carta (el que tiene el script CartaMemoria).")]
    [SerializeField] private CartaMemoria prefabCarta;
    [Tooltip("Objeto con el Grid Layout Group donde se crean las cartas.")]
    [SerializeField] private RectTransform contenedorCartas;

    [Header("Tiempos")]
    [Tooltip("Segundos que se ven las dos cartas cuando NO son par, antes de taparse.")]
    [SerializeField] private float segundosAntesDeOcultar = 0.8f;

    // Avisan a otros sistemas.
    public event System.Action<CartaMemoriaDatos, int, int> ParEncontrado; // datos del par, encontrados, total
    public event System.Action Completado;
    public event System.Action Abandonado;
    public event System.Action Cerrado;

    public string IdMinijuego => idMinijuego;
    public DatosMemoria Datos => datos;

    private readonly List<CartaMemoria> cartas = new List<CartaMemoria>();
    private CartaMemoria primera;
    private CartaMemoria segunda;
    private bool resolviendo;
    private bool completado;
    private int paresEncontrados;
    private int totalPares;
    private Coroutine rutinaResolver;

    // Si el minijuego se vuelve a activar despues de abandonarlo o cerrarlo, empieza de cero.
    private void OnEnable()
    {
        if (cartas.Count > 0)
        {
            Reiniciar();
        }
    }

    private void Start()
    {
        if (datos == null || datos.Cartas == null || datos.Cartas.Length == 0)
        {
            Debug.LogError("Faltan los Datos de la memoria (o no tienen cartas) en el Inspector.", this);
            return;
        }
        if (prefabCarta == null)
        {
            Debug.LogError("Falta asignar el Prefab Carta en el Inspector.", this);
            return;
        }
        if (contenedorCartas == null)
        {
            Debug.LogError("Falta asignar el Contenedor Cartas (el objeto Tablero) en el Inspector.", this);
            return;
        }

        CrearCartas();
        Mezclar();
    }

    private void OnDestroy()
    {
        foreach (CartaMemoria carta in cartas)
        {
            if (carta != null)
            {
                carta.Pulsada -= AlPulsarCarta;
            }
        }
    }

    // Por cada tipo de carta de los datos se crean 2 cartas iguales.
    private void CrearCartas()
    {
        HashSet<string> idsUsados = new HashSet<string>();

        foreach (CartaMemoriaDatos datosCarta in datos.Cartas)
        {
            if (datosCarta == null)
            {
                continue;
            }
            if (string.IsNullOrEmpty(datosCarta.id))
            {
                Debug.LogError("Hay una carta sin id en los datos. Se ignora.", this);
                continue;
            }
            if (!idsUsados.Add(datosCarta.id))
            {
                Debug.LogError("Id de carta repetido en los datos: " + datosCarta.id + ". Se ignora.", this);
                continue;
            }

            for (int copia = 1; copia <= 2; copia++)
            {
                CartaMemoria carta = Instantiate(prefabCarta, contenedorCartas);
                carta.name = "Carta_" + datosCarta.id + "_" + copia;
                carta.Configurar(datosCarta, datos.Reverso);
                carta.Pulsada += AlPulsarCarta;
                cartas.Add(carta);
            }
        }

        totalPares = idsUsados.Count;
    }

    // Mezcla el orden de la lista y acomoda las cartas en el tablero segun ese orden.
    private void Mezclar()
    {
        for (int i = cartas.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (cartas[i], cartas[j]) = (cartas[j], cartas[i]);
        }

        // El Grid Layout Group ubica a los hijos segun su orden en la jerarquia.
        for (int i = 0; i < cartas.Count; i++)
        {
            cartas[i].transform.SetSiblingIndex(i);
        }
    }

    [ContextMenu("Reiniciar memoria")]
    public void Reiniciar()
    {
        if (cartas.Count == 0)
        {
            return;
        }

        if (rutinaResolver != null)
        {
            StopCoroutine(rutinaResolver);
            rutinaResolver = null;
        }

        primera = null;
        segunda = null;
        resolviendo = false;
        completado = false;
        paresEncontrados = 0;

        foreach (CartaMemoria carta in cartas)
        {
            carta.Reiniciar();
        }
        Mezclar();
    }

    private void AlPulsarCarta(CartaMemoria carta)
    {
        // Mientras se resuelve un par o si ya termino, no se aceptan mas cartas.
        if (completado || resolviendo || carta == primera)
        {
            return;
        }

        carta.Voltear(true);

        if (primera == null)
        {
            primera = carta;
            return;
        }

        segunda = carta;
        rutinaResolver = StartCoroutine(Resolver());
    }

    private IEnumerator Resolver()
    {
        resolviendo = true;

        CartaMemoria a = primera;
        CartaMemoria b = segunda;
        primera = null;
        segunda = null;

        // Esperamos a que las dos terminen de girar para que el jugador las vea bien.
        while (a.EnAnimacion || b.EnAnimacion)
        {
            yield return null;
        }

        if (a.Id == b.Id)
        {
            a.MarcarEmparejada();
            b.MarcarEmparejada();
            paresEncontrados++;
            ParEncontrado?.Invoke(BuscarDatos(a.Id), paresEncontrados, totalPares);

            if (paresEncontrados >= totalPares)
            {
                completado = true;
                Debug.Log("Memoria completada: " + idMinijuego);
                Completado?.Invoke();
            }
        }
        else
        {
            yield return new WaitForSeconds(segundosAntesDeOcultar);
            a.Voltear(false);
            b.Voltear(false);
        }

        resolviendo = false;
        rutinaResolver = null;
    }

    private CartaMemoriaDatos BuscarDatos(string id)
    {
        foreach (CartaMemoriaDatos datosCarta in datos.Cartas)
        {
            if (datosCarta != null && datosCarta.id == id)
            {
                return datosCarta;
            }
        }
        return null;
    }

    // Pensado para el boton "Atras": se conecta en el OnClick del boton.
    public void Abandonar()
    {
        if (!completado)
        {
            Debug.Log("Minijuego abandonado: " + idMinijuego);
            Abandonado?.Invoke();
        }

        gameObject.SetActive(false);
    }

    // Pensado para el boton "Continuar" de la tarjeta, luego de completar el minijuego.
    public void Cerrar()
    {
        Cerrado?.Invoke();
        gameObject.SetActive(false);
    }
}