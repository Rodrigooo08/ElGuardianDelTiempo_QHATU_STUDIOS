using System;
using UnityEngine;

// Un integrante de la Primera Junta: nombre y cargo que se muestran al encontrar su par.
[Serializable]
public class IntegranteJunta
{
    public string nombre;
    public string cargo;
}

// Un tipo de carta del tablero. En el juego se crean 2 cartas iguales por cada una de estas.
// Una carta puede mostrar a 1, 2 o 3 integrantes de la Primera Junta.
[Serializable]
public class CartaMemoriaDatos
{
    [Tooltip("Identificador unico de la carta (ej: saavedra, moreno_paso).")]
    public string id;
    [Tooltip("Integrantes que muestra esta carta (1 a 3).")]
    public IntegranteJunta[] integrantes;
    [Tooltip("Imagen de la cara de la carta. Si queda vacia, se muestran solo los nombres.")]
    public Sprite imagen;
}

// Datos del minijuego de memoria de pares (Tramo 2 del Nivel 1).
// Se crea desde el menu: Assets > Create > Rolly > Datos Memoria.
[CreateAssetMenu(menuName = "Rolly/Datos Memoria", fileName = "Datos_Memoria_N1")]
public class DatosMemoria : ScriptableObject
{
    [Header("Reverso")]
    [Tooltip("Imagen del dorso de las cartas (boca abajo). Si queda vacia, se usa un color liso.")]
    [SerializeField] private Sprite reverso;

    [Header("Cartas")]
    [Tooltip("Tipos de carta. Cada uno genera 2 cartas iguales. Para 12 cartas se necesitan 6 tipos.")]
    [SerializeField] private CartaMemoriaDatos[] cartas;

    [Header("Dato curioso final")]
    [SerializeField] private string tituloDato = "La Primera Junta";
    [TextArea(3, 8)]
    [SerializeField] private string textoDato =
        "El 25 de mayo de 1810 se formo en Buenos Aires la Primera Junta, el primer gobierno patrio. " +
        "La integraron nueve personas: un presidente, seis vocales y dos secretarios.";

    public Sprite Reverso => reverso;
    public CartaMemoriaDatos[] Cartas => cartas;
    public string TituloDato => tituloDato;
    public string TextoDato => textoDato;

    // Unity llama a Reset() al crear el asset y cuando se usa "Reset" en el menu de los tres puntitos.
    // Solo ocurre en el editor, nunca durante el juego.
    private void Reset()
    {
        CargarCartasDeEjemplo();
    }

    // Carga las 6 cartas propuestas. Los cargos son un borrador: hay que validarlos con la catedra.
    [ContextMenu("Cargar cartas de ejemplo")]
    private void CargarCartasDeEjemplo()
    {
        cartas = new CartaMemoriaDatos[]
        {
            Crear("saavedra", Integrante("Cornelio Saavedra", "Presidente de la Junta")),
            Crear("belgrano", Integrante("Manuel Belgrano", "Vocal")),
            Crear("castelli", Integrante("Juan Jose Castelli", "Vocal")),
            Crear("moreno_paso",
                Integrante("Mariano Moreno", "Secretario"),
                Integrante("Juan Jose Paso", "Secretario")),
            Crear("azcuenaga_alberti",
                Integrante("Miguel de Azcuenaga", "Vocal"),
                Integrante("Manuel Alberti", "Vocal")),
            Crear("matheu_larrea",
                Integrante("Domingo Matheu", "Vocal"),
                Integrante("Juan Larrea", "Vocal"))
        };

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    private static IntegranteJunta Integrante(string nombre, string cargo)
    {
        return new IntegranteJunta { nombre = nombre, cargo = cargo };
    }

    private static CartaMemoriaDatos Crear(string id, params IntegranteJunta[] integrantes)
    {
        return new CartaMemoriaDatos { id = id, integrantes = integrantes };
    }
}
