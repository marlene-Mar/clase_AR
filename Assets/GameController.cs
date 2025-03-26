using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [Header("Personajes")]
    public GameObject personajePrincipal;
    public GameObject[] modelosVillanos;
    public Transform[] posicionesVillanos;

    [Header("Marcadores")]
    public ObserverBehaviour marcadorPersonaje;
    public List<ObserverBehaviour> marcadoresVillanos = new List<ObserverBehaviour>();

    [Header("UI")]
    public Button botonAvanzar;
    public Button botonPersonaje;
    public Button botonOponente;
    public TMPro.TextMeshProUGUI textoEstadoJuego;
    public TMPro.TextMeshProUGUI textoPuntajePersonaje;
    public TMPro.TextMeshProUGUI textoPuntajeOponente;

    [Header("Configuración")]
    public float velocidadMovimiento = 2.0f;

    // Variables privadas
    private bool personajeEnMovimiento = false;
    private int indiceVillanoActual = -1;
    private int puntajePersonaje = 0;
    private int puntajeOponente = 0;
    private int victoriasPersonaje = 0;
    private int victoriasOponente = 0;
    private GameObject villanoActual;
    private bool enCombate = false;

    void Start()
    {
        // Inicializar botones
        botonAvanzar.onClick.AddListener(AvanzarAMarcadorAleatorio);
        botonPersonaje.onClick.AddListener(LanzarDadoPersonaje);
        botonOponente.onClick.AddListener(LanzarDadoOponente);

        // Desactivar botones de combate inicialmente
        botonPersonaje.interactable = false;
        botonOponente.interactable = false;

        // Verificar que el personaje principal esté en su marcador inicial
        if (marcadorPersonaje.TargetStatus.Status == Status.TRACKED)
        {
            personajePrincipal.SetActive(true);
            botonAvanzar.interactable = true;
            ActualizarEstadoJuego("Presiona Avanzar para comenzar.");
        }
        else
        {
            ActualizarEstadoJuego("Coloca el marcador del personaje principal en la cámara.");
            botonAvanzar.interactable = false;
        }

        // Desactivar todos los villanos al inicio
        foreach (GameObject villano in modelosVillanos)
        {
            villano.SetActive(false);
        }
    }

    void Update()
    {
        // Verificar estado de los marcadores
        VerificarMarcadores();
    }

    private void VerificarMarcadores()
    {
        // Verificar marcador del personaje
        if (marcadorPersonaje.TargetStatus.Status == Status.TRACKED ||
            marcadorPersonaje.TargetStatus.Status == Status.EXTENDED_TRACKED)
        {
            personajePrincipal.SetActive(true);

            if (!personajeEnMovimiento && !enCombate)
            {
                botonAvanzar.interactable = true;
            }
        }
        else
        {
            personajePrincipal.SetActive(false);
            botonAvanzar.interactable = false;
            if (!enCombate)
            {
                ActualizarEstadoJuego("Marcador del personaje perdido. Colócalo en la cámara.");
            }
        }

        // Si estamos en combate, verificar el marcador del villano
        if (enCombate && indiceVillanoActual >= 0 && indiceVillanoActual < marcadoresVillanos.Count)
        {
            if (marcadoresVillanos[indiceVillanoActual].TargetStatus.Status != Status.TRACKED &&
                marcadoresVillanos[indiceVillanoActual].TargetStatus.Status != Status.EXTENDED_TRACKED)
            {
                ActualizarEstadoJuego("Marcador del oponente perdido. Colócalo en la cámara.");
                botonPersonaje.interactable = false;
                botonOponente.interactable = false;
            }
            else
            {
                if (!personajeEnMovimiento)
                {
                    botonPersonaje.interactable = true;
                    botonOponente.interactable = true;
                }
            }
        }
    }

    public void AvanzarAMarcadorAleatorio()
    {
        if (personajeEnMovimiento || marcadoresVillanos.Count == 0)
            return;

        // Seleccionar un marcador aleatorio que no sea el actual
        int nuevoIndice;
        if (marcadoresVillanos.Count > 1 && indiceVillanoActual >= 0)
        {
            do
            {
                nuevoIndice = Random.Range(0, marcadoresVillanos.Count);
            } while (nuevoIndice == indiceVillanoActual);
        }
        else
        {
            nuevoIndice = Random.Range(0, marcadoresVillanos.Count);
        }

        indiceVillanoActual = nuevoIndice;
        ObserverBehaviour marcadorDestino = marcadoresVillanos[indiceVillanoActual];

        // Verificar que el marcador destino esté visible
        if (marcadorDestino.TargetStatus.Status == Status.TRACKED ||
            marcadorDestino.TargetStatus.Status == Status.EXTENDED_TRACKED)
        {
            // Desactivar villanos anteriores
            foreach (GameObject villano in modelosVillanos)
            {
                villano.SetActive(false);
            }

            // Activar el villano actual
            villanoActual = modelosVillanos[indiceVillanoActual % modelosVillanos.Length];
            villanoActual.SetActive(true);

            // Mover el personaje
            StartCoroutine(MoverPersonaje(marcadorDestino.transform.position));
            ActualizarEstadoJuego("Avanzando hacia el oponente...");
        }
        else
        {
            ActualizarEstadoJuego("El marcador destino no está visible. Inténtalo de nuevo.");
        }
    }

    private IEnumerator MoverPersonaje(Vector3 posicionDestino)
    {
        personajeEnMovimiento = true;
        botonAvanzar.interactable = false;

        Vector3 posicionInicial = personajePrincipal.transform.position;
        float tiempoTranscurrido = 0;

        while (tiempoTranscurrido < 1f)
        {
            tiempoTranscurrido += Time.deltaTime * velocidadMovimiento;
            personajePrincipal.transform.position = Vector3.Lerp(posicionInicial, posicionDestino, tiempoTranscurrido);
            yield return null;
        }

        personajeEnMovimiento = false;
        enCombate = true;

        ActualizarEstadoJuego("Presiona tu botón");
        botonPersonaje.interactable = true;
        botonOponente.interactable = false;

        // Resetear los textos de puntaje
        textoPuntajePersonaje.text = "Personaje: -";
        textoPuntajeOponente.text = "Oponente: -";
    }

    public void LanzarDadoPersonaje()
    {
        puntajePersonaje = Random.Range(1, 11);
        textoPuntajePersonaje.text = "Personaje: " + puntajePersonaje;

        botonPersonaje.interactable = false;
        botonOponente.interactable = true;

        ActualizarEstadoJuego("Has sacado un " + puntajePersonaje + ". Ahora es turno del oponente.");
    }

    public void LanzarDadoOponente()
    {
        puntajeOponente = Random.Range(1, 11);
        textoPuntajeOponente.text = "Oponente: " + puntajeOponente;

        botonOponente.interactable = false;

        // Determinar el ganador de la ronda
        if (puntajePersonaje > puntajeOponente)
        {
            victoriasPersonaje++;
            ActualizarEstadoJuego("¡Ganaste esta ronda! (" + victoriasPersonaje + " de 2)");
        }
        else if (puntajePersonaje < puntajeOponente)
        {
            victoriasOponente++;
            ActualizarEstadoJuego("Perdiste esta ronda. (" + victoriasOponente + " de 2)");
        }
        else
        {
            ActualizarEstadoJuego("¡Empate! Nadie gana esta ronda.");
        }

        // Verificar si alguien ganó el combate (2 de 3)
        if (victoriasPersonaje >= 2)
        {
            StartCoroutine(FinalizarCombate(true));
        }
        else if (victoriasOponente >= 2)
        {
            StartCoroutine(FinalizarCombate(false));
        }
        else
        {
            // Continuar con otra ronda
            StartCoroutine(PrepararNuevaRonda());
        }
    }

    private IEnumerator PrepararNuevaRonda()
    {
        yield return new WaitForSeconds(2.0f);

        textoPuntajePersonaje.text = "Personaje: -";
        textoPuntajeOponente.text = "Oponente: -";

        botonPersonaje.interactable = true;
        botonOponente.interactable = false;

        ActualizarEstadoJuego("Nueva ronda. Presiona el botón de Personaje para lanzar tu dado.");
    }

    private IEnumerator FinalizarCombate(bool personajeGano)
    {
        yield return new WaitForSeconds(2.0f);

        if (personajeGano)
        {
            ActualizarEstadoJuego("¡Has ganado el combate! Avanza.");
            // Desactivar el villano actual
            villanoActual.SetActive(false);
        }
        else
        {
            ActualizarEstadoJuego("Has perdido el combate. Regresando al inicio...");
            // Regresar al personaje a su posición inicial
            StartCoroutine(MoverPersonaje(marcadorPersonaje.transform.position));
        }

        // Resetear contadores
        victoriasPersonaje = 0;
        victoriasOponente = 0;

        // Salir del modo combate
        enCombate = false;

        // Activar botón de avanzar
        botonAvanzar.interactable = true;
        botonPersonaje.interactable = false;
        botonOponente.interactable = false;
    }

    private void ActualizarEstadoJuego(string mensaje)
    {
        textoEstadoJuego.text = mensaje;
        Debug.Log(mensaje);
    }
}