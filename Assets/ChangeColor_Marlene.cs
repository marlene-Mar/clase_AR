using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeColor_Marlene : MonoBehaviour
{
    public GameObject model;
    public Color color;
    public Material colorMaterial;

    //Para hacer que cambie de color cada que se aprieta botón
    private Color[] colors = { Color.red, Color.green, Color.yellow, Color.black, Color.magenta };

    // Start is called before the first frame update
    void Start()
    {
        //model.GetComponent<Renderer>().material.color = Color.red;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeColor_Bot()
    {
        color = colors[Random.Range(0, colors.Length)];
        model.GetComponent<Renderer>().material.color = color;
        colorMaterial.color = color; 
    }
}
