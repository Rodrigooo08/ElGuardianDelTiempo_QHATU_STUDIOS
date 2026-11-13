using System;
using UnityEngine;

/// <summary>
/// Qué objeto necesita un vecino del minijuego de reparto.
/// </summary>
public enum ObjetoReparto
{
    Cinta,
    Paraguas
}

/// <summary>
/// Un tipo de vecino que puede asomarse en un puesto.
/// </summary>
[Serializable]
public class TipoVecino
{
    public string nombre;
    public ObjetoReparto objetoQuePide;
    public Sprite imagen; // provisorio: puede quedar vacío hasta que Axel haga el arte
}

[CreateAssetMenu(fileName = "Datos_Reparto_N1", menuName = "Rolly/Minijuegos/Datos Reparto")]
public class DatosReparto : ScriptableObject
{
    [Header("Identificación")]
    public string idMinijuego = "N1_T3_Reparto";

    [Header("Objetivo")]
    [Min(1)] public int entregasParaCompletar = 12;

    [Header("Dificultad: valor al empezar y valor al final")]
    [Tooltip("Segundos que el vecino queda visible al empezar.")]
    public float segundosVisibleInicial = 3f;
    [Tooltip("Segundos que el vecino queda visible cuando ya casi se completó.")]
    public float segundosVisibleFinal = 1.8f;

    [Tooltip("Segundos de espera entre una aparición y la siguiente, al empezar.")]
    public float segundosEntreAparicionesInicial = 1.5f;
    [Tooltip("Segundos de espera entre apariciones cuando ya casi se completó.")]
    public float segundosEntreAparicionesFinal = 0.7f;

    [Tooltip("Cuántos vecinos pueden estar visibles a la vez al empezar.")]
    [Min(1)] public int maximoSimultaneosInicial = 1;
    [Tooltip("Cuántos vecinos pueden estar visibles a la vez al final.")]
    [Min(1)] public int maximoSimultaneosFinal = 3;

    [Header("Vecinos")]
    public TipoVecino[] vecinos;

    [Header("Dato curioso (borrador: validar con la cátedra)")]
    public string tituloDato;
    [TextArea(3, 8)] public string textoDato;

    // Estas funciones reciben el progreso (0 = recién empieza, 1 = completado)
    // y devuelven el valor de dificultad que corresponde en ese momento.
    public float SegundosVisibles(float progreso)
    {
        return Mathf.Lerp(segundosVisibleInicial, segundosVisibleFinal, Mathf.Clamp01(progreso));
    }

    public float SegundosEntreApariciones(float progreso)
    {
        return Mathf.Lerp(segundosEntreAparicionesInicial, segundosEntreAparicionesFinal, Mathf.Clamp01(progreso));
    }

    public int MaximoSimultaneos(float progreso)
    {
        return Mathf.RoundToInt(Mathf.Lerp(maximoSimultaneosInicial, maximoSimultaneosFinal, Mathf.Clamp01(progreso)));
    }

    // Se ejecuta al tocar valores en el Inspector: evita combinaciones sin sentido.
    private void OnValidate()
    {
        segundosVisibleInicial = Mathf.Max(0.5f, segundosVisibleInicial);
        segundosVisibleFinal = Mathf.Clamp(segundosVisibleFinal, 0.5f, segundosVisibleInicial);
        segundosEntreAparicionesInicial = Mathf.Max(0.2f, segundosEntreAparicionesInicial);
        segundosEntreAparicionesFinal = Mathf.Clamp(segundosEntreAparicionesFinal, 0.2f, segundosEntreAparicionesInicial);
        maximoSimultaneosFinal = Mathf.Max(maximoSimultaneosInicial, maximoSimultaneosFinal);
    }

    // Aparece en el menú de los tres puntitos del Inspector como "Reset".
    // Carga vecinos y un dato de ejemplo para poder probar enseguida.
    private void Reset()
    {
        idMinijuego = "N1_T3_Reparto";
        entregasParaCompletar = 12;

        vecinos = new TipoVecino[]
        {
            new TipoVecino { nombre = "Vecina del balcón",   objetoQuePide = ObjetoReparto.Cinta },
            new TipoVecino { nombre = "Comerciante",         objetoQuePide = ObjetoReparto.Paraguas },
            new TipoVecino { nombre = "Vecino de la esquina", objetoQuePide = ObjetoReparto.Cinta },
            new TipoVecino { nombre = "Aguatero",            objetoQuePide = ObjetoReparto.Paraguas },
            new TipoVecino { nombre = "Criolla con mantilla", objetoQuePide = ObjetoReparto.Cinta },
            new TipoVecino { nombre = "Pulpero",             objetoQuePide = ObjetoReparto.Paraguas }
        };

        tituloDato = "Una semana que cambió todo";
        textoDato = "Entre el 18 y el 25 de mayo de 1810, Buenos Aires vivió días de noticias, " +
                    "reuniones y charlas en la calle. Los vecinos hablaban, opinaban y esperaban " +
                    "novedades, hasta que el 25 se formó la Primera Junta. [BORRADOR: validar con la cátedra]";
    }
}