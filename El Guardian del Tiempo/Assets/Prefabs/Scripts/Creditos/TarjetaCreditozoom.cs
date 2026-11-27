using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Canvas))]
public class TarjetaCreditoZoom : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Configuración de Zoom")]
    [SerializeField] private float escalaZoom = 1.15f;
    [SerializeField] private float velocidadTransicion = 10f;

    private Vector3 escalaOriginal;
    private Vector3 escalaObjetivo;
    private Canvas canvasTarjeta;

    private void Awake()
    {
        canvasTarjeta = GetComponent<Canvas>();
        canvasTarjeta.overrideSorting = true;
        canvasTarjeta.sortingOrder = 1;
    }

    private void Start()
    {
        escalaOriginal = transform.localScale;
        escalaObjetivo = escalaOriginal;
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, escalaObjetivo, Time.unscaledDeltaTime * velocidadTransicion);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        escalaObjetivo = escalaOriginal * escalaZoom;
        canvasTarjeta.sortingOrder = 10; 
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        escalaObjetivo = escalaOriginal;
        canvasTarjeta.sortingOrder = 1;
    }
}