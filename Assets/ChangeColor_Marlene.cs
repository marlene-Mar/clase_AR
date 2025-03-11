using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeColor_Marlene : MonoBehaviour
{
    public GameObject model; //Modelo al que se le aplicará el cambio de color
    public Color [] colors; //Colores que se le aplicarán al modelo
    public Material[] materials; //Materiales que se le aplicarán al modelo

    private int contText_Cambio = 0; //Contador para cambiar de textura

    /*---------------------------PRACTICA 2------------------------------------
    //public Material colorMaterial;

    //Para hacer que cambie de color cada que se aprieta botón 
    //private Color[] colors = { Color.red, Color.green, Color.yellow, Color.black, Color.magenta };

    ---------------------------------------------------------------------------*/


    // Start is called before the first frame update
    void Start()
    {
        /*------------------------PRACTICA 2-------------------------------------
        Cambio de color al modelo en cuanto se inicia la aplicación
        model.GetComponent<Renderer>().material.color = Color.red;
        -----------------------------------------------------------------------*/
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeColor_Bot()
    {
        contText_Cambio++; //Incrementa el contador para cambiar de textura
        //Si el contador es mayor o igual a la cantidad de texturas, se reinicia
        if (contText_Cambio >= materials.Length)
        {
            contText_Cambio = 0;
        }
        //Se le asigna un color aleatorio al material
        materials[contText_Cambio].color = colors[Random.Range(0, colors.Length)];
        //Se le asigna un material aleatorio al modelo
        model.GetComponent<Renderer>().material = materials[Random.Range(0, materials.Length)];

        /*--------------------------PRACTICA 2------------------------------------
        color = colors[Random.Range(0, colors.Length)];
        model.GetComponent<Renderer>().material.color = color;
        colorMaterial.color = color;
        ------------------------------------------------------------------------*/
    }
}
