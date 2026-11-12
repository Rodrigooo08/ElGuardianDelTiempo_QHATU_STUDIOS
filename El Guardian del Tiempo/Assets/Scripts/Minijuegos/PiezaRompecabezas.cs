using UnityEngine;

// Datos y estado de una pieza del rompecabezas. No tiene lógica de entrada:
// el ControladorRompecabezas decide qué pieza se mueve.
public class PiezaRompecabezas : MonoBehaviour
{
    public Vector2 CentroLocal { get; private set; }
    public bool Encajada { get; private set; }
    public MeshRenderer Renderizador { get; private set; }
    public PolygonCollider2D Colision { get; private set; }

    public void Inicializar(Vector2 centroLocal, MeshRenderer renderizador, PolygonCollider2D colision)
    {
        CentroLocal = centroLocal;
        Renderizador = renderizador;
        Colision = colision;
        Encajada = false;
    }

    public void MarcarEncajada()
    {
        Encajada = true;
        Colision.enabled = false;
    }

    public void Liberar()
    {
        Encajada = false;
        Colision.enabled = true;
    }
}