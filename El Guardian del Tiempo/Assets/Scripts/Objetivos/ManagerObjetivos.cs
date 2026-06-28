using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ManagerObjetivos : MonoBehaviour
{
    [SerializeField] private List<PartesObjetivo> partesObjetivos = new List<PartesObjetivo>();

    [SerializeField] private GameObject panelObjetivos;
    [SerializeField] private GameObject botonObjetivos;
    [SerializeField] private GameObject salidaFinal;

    public List<PartesObjetivo> PartesObjetivos => partesObjetivos;

    private void Start()
    {
        ReiniciarMinerales();

        if (panelObjetivos != null)
            panelObjetivos.SetActive(false);

        if (botonObjetivos != null)
            botonObjetivos.SetActive(true); // siempre visible en tu única escena
    }

    public void MarcarCumplido(string descripcion)
    {
        PartesObjetivo mineral = partesObjetivos.Find(m => m.descripcion == descripcion);
        if (mineral != null && !mineral.cumplido)
        {
            mineral.cumplido = true;


            if (mineral.imagenObjetivos != null && mineral.spriteCheck != null)
                mineral.imagenObjetivos.sprite = mineral.spriteCheck;

            ActualizarSalida();
        }
    }

    public void ReiniciarMinerales()
    {
        foreach (PartesObjetivo mineral in partesObjetivos)
        {
            mineral.cumplido = false;
            if (mineral.imagenObjetivos != null && mineral.spriteVacio != null)
                mineral.imagenObjetivos.sprite = mineral.spriteVacio;
        }

        ActualizarSalida();
    }

    private void ActualizarSalida()
    {
        bool todosCumplidos = partesObjetivos.TrueForAll(m => m.cumplido);
        if (salidaFinal != null)
            salidaFinal.SetActive(todosCumplidos);
        if (todosCumplidos)
        {
            // Desenparentar de la cámara
            salidaFinal.transform.SetParent(null);
        }
    }
    //funcion para el boton, muestra y oculta el panel de objetivos
    public void TogglePanelObjetivos()
    {
        if (panelObjetivos != null)
            panelObjetivos.SetActive(!panelObjetivos.activeSelf);
    }
}
