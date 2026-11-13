using System.Collections.Generic;
using UnityEngine;

// Una pieza = una lista de puntos (de 0 a 1) que forman un polígono convexo.
[System.Serializable]
public class FormaPieza
{
    [SerializeField] private Vector2[] puntos = new Vector2[0];

    public Vector2[] Puntos => puntos;

    public FormaPieza() { }

    public FormaPieza(Vector2[] puntos)
    {
        this.puntos = puntos;
    }
}

// Datos de un rompecabezas: imagen, tamaño del tablero, piezas y dato curioso.
[CreateAssetMenu(fileName = "DatosRompecabezas", menuName = "Rolly/Minijuegos/Datos del rompecabezas")]
public class DatosRompecabezas : ScriptableObject
{
    [Header("Imagen y tablero")]
    [SerializeField] private Texture2D imagen;
    [SerializeField] private float anchoTablero = 8f;
    [SerializeField] private float altoTablero = 6f;

    [Header("Piezas (puntos de 0 a 1, sentido antihorario, formas convexas)")]
    [SerializeField] private List<FormaPieza> piezas = new List<FormaPieza>();

    [Header("Dato curioso al completar")]
    [SerializeField] private string tituloDato;
    [SerializeField, TextArea(3, 8)] private string textoDato;

    public Texture2D Imagen => imagen;
    public float AnchoTablero => anchoTablero;
    public float AltoTablero => altoTablero;
    public IReadOnlyList<FormaPieza> Piezas => piezas;
    public string TituloDato => tituloDato;
    public string TextoDato => textoDato;

    // Las 8 piezas del Paso 1, para no cargarlas a mano punto por punto.
    private static readonly Vector2[][] formasDeEjemplo = new Vector2[][]
    {
        new[] { new Vector2(0f, 0f),     new Vector2(0.4f, 0f),    new Vector2(0.45f, 0.35f), new Vector2(0f, 0.25f) },
        new[] { new Vector2(0.4f, 0f),   new Vector2(0.75f, 0f),   new Vector2(0.7f, 0.6f),   new Vector2(0.45f, 0.35f) },
        new[] { new Vector2(0.75f, 0f),  new Vector2(1f, 0f),      new Vector2(1f, 0.45f),    new Vector2(0.7f, 0.6f) },
        new[] { new Vector2(0f, 0.25f),  new Vector2(0.45f, 0.35f), new Vector2(0.3f, 0.65f), new Vector2(0f, 0.6f) },
        new[] { new Vector2(0.45f, 0.35f), new Vector2(0.7f, 0.6f), new Vector2(0.3f, 0.65f) },
        new[] { new Vector2(0.7f, 0.6f), new Vector2(1f, 0.45f),   new Vector2(1f, 1f),       new Vector2(0.6f, 1f) },
        new[] { new Vector2(0f, 0.6f),   new Vector2(0.3f, 0.65f), new Vector2(0.25f, 1f),    new Vector2(0f, 1f) },
        new[] { new Vector2(0.3f, 0.65f), new Vector2(0.7f, 0.6f), new Vector2(0.6f, 1f),     new Vector2(0.25f, 1f) }
    };

    [ContextMenu("Cargar piezas de ejemplo")]
    private void CargarPiezasDeEjemplo()
    {
        piezas.Clear();
        foreach (Vector2[] forma in formasDeEjemplo)
        {
            piezas.Add(new FormaPieza(forma));
        }

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}