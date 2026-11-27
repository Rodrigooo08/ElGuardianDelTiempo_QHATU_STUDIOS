using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GeneradorCreditos : MonoBehaviour
{
    [System.Serializable]
    public struct Integrante
    {
        public string nombre;
        public string rol;
        public Sprite foto;
    }

    [Header("Referencias")]
    [SerializeField] private GameObject tarjetaPrefab;
    [SerializeField] private Transform contenedorTarjetas;

    [Header("Equipo (6 Integrantes)")]
    [SerializeField] private Integrante[] integrantes = new Integrante[6];

    private void Start()
    {
        foreach (Integrante miembro in integrantes)
        {
            GameObject nuevaTarjeta = Instantiate(tarjetaPrefab, contenedorTarjetas);

            // Busca los componentes hijos por nombre exacto
            TMP_Text textoNombre = nuevaTarjeta.transform.Find("TextoNombre").GetComponent<TMP_Text>();
            TMP_Text textoRol = nuevaTarjeta.transform.Find("TextoRol").GetComponent<TMP_Text>();
            Image imagenFoto = nuevaTarjeta.transform.Find("ImagenFoto").GetComponent<Image>();

            textoNombre.text = miembro.nombre;
            textoRol.text = miembro.rol;
            if (miembro.foto != null) imagenFoto.sprite = miembro.foto;
        }
    }
}