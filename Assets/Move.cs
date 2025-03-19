//PRACTICA 4: EL OBJETO 3D SE MOVERÉ ENTRE MARCADORES
//Nommbre: De la Cruz Padilla Marlene Mariana

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia; //Libreria de Vuforia para sacar la información de los marcadores

public class Move : MonoBehaviour
{
    public GameObject model; //Variable para el modelo 3D
    public ObserverBehaviour[] ImageTargets; //Variable para los marcadores
    public int currentTarget; //Variable para el marcador actual
    public float speed = 1.0f; //Variable para la velocidad de movimiento
    private bool isMoving = false; //Variable para saber si el objeto se esta moviendo

    // Start is called before the first frame update
    /*void Start()
    {
        
    }*/

    public void moveToNextMarket()
    {
        if (!isMoving)
        {
            StartCoroutine(MoveModel());
        }
    }

    //Generando un corrutina
    private IEnumerator MoveModel()
    {
        //El modelo 3D ya termino de moverse
        isMoving = true;
        ObserverBehaviour target = GetNextDetectedTarget();
        if (target == null){
            isMoving = false;
            yield break;
        }

        Vector3 startPosition = model.transform.position;
        Vector3 endPosition = target.transform.position;

        float journey = 0;

        //Transladando el modelo 3D
        while (journey <= 1f){
            journey += Time.deltaTime * speed;
            model.transform.position = Vector3.Lerp(startPosition, endPosition, journey);
            yield return null;
        }

        currentTarget=(currentTarget + 1) % ImageTargets.Length;
        isMoving = false;

    }

    private ObserverBehaviour GetNextDetectedTarget(){
        foreach (ObserverBehaviour target in ImageTargets){
            if (target != null && (target.TargetStatus.Status==Status.TRACKED || target.TargetStatus.Status==Status.EXTENDED_TRACKED))
            {
                return target;
            }
        }
        return null;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

