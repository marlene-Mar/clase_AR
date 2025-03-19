//PRACTICA 4: EL OBJETO 3D SE MOVERÉ ENTRE MARCADORES
//Nommbre: De la Cruz Padilla Marlene Mariana

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia; //Libreria de Vuforia para sacar la información de los marcadores

/* PRACTICA 4 - PREVIO
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
        
    }

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
        if (target == null)
        {
            isMoving = false;
            yield break;
        }

        Vector3 startPosition = model.transform.position;
        Vector3 endPosition = target.transform.position;

        float journey = 0;

        //Tranladando el modelo 3D
        while (journey <= 1f)
        {
            journey += Time.deltaTime * speed;
            model.transform.position = Vector3.Lerp(startPosition, endPosition, journey);
            yield return null;
        }

        currentTarget = (currentTarget + 1) % ImageTargets.Length;
        isMoving = false;

    }

    private ObserverBehaviour GetNextDetectedTarget()
    {
        foreach (ObserverBehaviour target in ImageTargets)
        {
            if (target != null && (target.TargetStatus.Status == Status.TRACKED || target.TargetStatus.Status == Status.EXTENDED_TRACKED))
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
*/

//PRACTICA 4 - ACTIVIDAD

public class Move : MonoBehaviour
{
    public GameObject model; //Variable para el modelo 3D
    public ObserverBehaviour[] ImageTargets; //Variable para los marcadores
    public int currentTarget; //Variable para el marcador actual
    public float speed = 1.0f; //Variable para la velocidad de movimiento
    private bool isMoving = false; //Variable para saber si el objeto se esta moviendo

    // Método para mover el modelo al marcador específico basado en el botón presionado
    public void MoveToMarker(int markerIndex)
    {
        if (!isMoving && markerIndex >= 0 && markerIndex < ImageTargets.Length)
        {
            currentTarget = markerIndex;
            StartCoroutine(MoveModel());
        }
    }

    // Métodos para ser llamados desde los botones de la interfaz
    public void MoveToMarker0() { MoveToMarker(0); }
    public void MoveToMarker1() { MoveToMarker(1); }
    public void MoveToMarker2() { MoveToMarker(2); }
    public void MoveToMarker3() { MoveToMarker(3); }
    public void MoveToMarker4() { MoveToMarker(4); }

    //Generando una corrutina
    private IEnumerator MoveModel()
    {
        //El modelo 3D ya terminó de moverse
        isMoving = true;
        ObserverBehaviour target = GetTargetByIndex(currentTarget);
        if (target == null)
        {
            Debug.LogWarning("El marcador " + currentTarget + " no está siendo detectado");
            isMoving = false;
            yield break;
        }

        Vector3 startPosition = model.transform.position;
        Vector3 endPosition = target.transform.position;
        float journey = 0;

        //Trasladando el modelo 3D
        while (journey <= 1f)
        {
            journey += Time.deltaTime * speed;
            model.transform.position = Vector3.Lerp(startPosition, endPosition, journey);
            yield return null;
        }

        isMoving = false;
    }

    // Método para obtener un marcador por su índice y verificar si está siendo detectado
    private ObserverBehaviour GetTargetByIndex(int index)
    {
        if (index >= 0 && index < ImageTargets.Length)
        {
            ObserverBehaviour target = ImageTargets[index];
            if (target != null && (target.TargetStatus.Status == Status.TRACKED || target.TargetStatus.Status == Status.EXTENDED_TRACKED))
            {
                return target;
            }
        }
        return null;
    }

    // Update is called once per frame
    void Update()
    {
        // Opcionalmente, puedes usar teclas numéricas para activar los marcadores
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            MoveToMarker0();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            MoveToMarker1();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            MoveToMarker2();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
        {
            MoveToMarker3();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5))
        {
            MoveToMarker4();
        }
    }
}