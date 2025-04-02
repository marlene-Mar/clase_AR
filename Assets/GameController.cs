using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [Header("Personajes")]
    public GameObject personajePrincipal; // Pompompurin
    public GameObject[] modelosOponentes; //Oponentes
    public GameObject[] chococat; //Chococat

    [Header("Marcadores")]
    public ObserverBehaviour marcadorPersonaje;
    public ObserverBehaviour marcadorChococat;
    public List<ObserverBehaviour> marcadoresOponentes = new List<ObserverBehaviour>();

    [Header("UI")]
    //Textos en pantalla
    public TMPro.TextMeshProUGUI textoEstadoJuego;
    public TMPro.TextMeshProUGUI textoPuntajePersonaje; //Puntaje de Pompompurin
    public TMPro.TextMeshProUGUI textoPuntajeOponente; //Puntaje del oponente

    //Botones de interacción
    public Button botonReiniciar; //Reinicia el juego
    public Button botonPersonaje; //Jugar Pompompurin
    public Button botonOponente; //Jugar oponente

    [Header("Configuración")]
    public float velocidadMovimiento = 2.0f;
    public int oponentesNecesariosParaGanar = 2;
    public float distanciaMinimaCombate = 0.1f;
    public float distanciaDeteccionChococat = 0.5f;
    public float minSwipeDistance = 30f; // Distancia mínima para considerar como swipe

    // Variables privadas
    private bool personajeEnMovimiento = false;
    private List<int> indicesMarcadoresVisibles = new List<int>();
    private bool esperandoSeleccion = false;
    private int indiceOponenteActual = -1;
    private GameObject oponenteActual;
    private bool mostradaMensajeBienvenida = false;


    //Datos del juego
    private int puntajePersonaje = 0;
    private int puntajeOponente = 0;
    private int victoriasPersonaje = 0;
    private int victoriasOponente = 0;
    private bool enCombate = false;
    private bool esperandoNuevosMarcadores = false;
    private HashSet<int> oponentesDerrotados = new HashSet<int>(); //Identificar oponentes ya derrotados

    string fullText = "Debes derrotar a 2 oponentes \ny rescatarlos del control de los malos\nSalva a Chococat";
    public float delay = 0.05f; // Tiempo entre cada letra

    // Variables para el control de gestos touch
    private Vector2 touchStartPosition;
    private bool isTouchTracking = false;

    void Start()
    {
        // Configurar botones
        ConfigurarBotones();

        // Desactivar todos los modelos al inicio
        DesactivarTodosLosModelos();

        // Mensaje inicial
        ActualizarEstadoJuego("¿Pompompurin?");
    }


    void Update()
    {
        // Verificar marcador del personaje
        VerificarMarcadorPersonaje();

        // Manejo de selección de oponentes si hay 2 marcadores visibles
        if (esperandoSeleccion && indicesMarcadoresVisibles.Count == 2)
        {
            ActualizarEstadoJuego("Desliza izquierda o derecha para elegir oponente");
            DetectarGestoSwipe();
        }

        // Verificar si hemos llegado a Chococat y debemos mostrar mensaje de victoria
        if (oponentesDerrotados.Count >= oponentesNecesariosParaGanar &&
            EsMarcadorVisible(marcadorChococat) &&
            personajePrincipal.activeInHierarchy)
        {
            float distanciaAChococat = Vector3.Distance(personajePrincipal.transform.position, marcadorChococat.transform.position);

            // Si estamos muy cerca de Chococat, mostramos el mensaje de victoria
            if (distanciaAChococat < distanciaDeteccionChococat)
            {
                textoEstadoJuego.text = "¡Rescataste a Chococat!";
                GanaJuego();
                return; // Salir para evitar sobrescribir el mensaje
            }
        }

        // Verificar otras condiciones de Chococat si no hemos ganado aún
        VerificarCondicionesChococat();
    }

    private void ConfigurarBotones()
    {
        // Configurar listeners de botones
        botonReiniciar.onClick.RemoveAllListeners();
        botonPersonaje.onClick.RemoveAllListeners();
        botonOponente.onClick.RemoveAllListeners();

        botonReiniciar.onClick.AddListener(ReiniciarJuego);
        botonPersonaje.onClick.AddListener(TiraPersonaje);
        botonOponente.onClick.AddListener(TiraOponente);

        // Desactivar botones de juego
        botonPersonaje.interactable = false;
        botonOponente.interactable = false;
    }

    private void DesactivarTodosLosModelos()
    {
        // Desactivar todos los modelos de oponentes
        foreach (GameObject oponente in modelosOponentes)
        {
            oponente.SetActive(false);
        }

        // Desactivar modelos de Chococat
        foreach (GameObject cat in chococat)
        {
            cat.SetActive(false);
        }

        personajePrincipal.SetActive(false);
    }

    private bool EsMarcadorVisible(ObserverBehaviour marcador)
    {
        return marcador.TargetStatus.Status == Status.TRACKED ||
               marcador.TargetStatus.Status == Status.EXTENDED_TRACKED;
    }

    private void VerificarMarcadorPersonaje()
    {
        bool isPersonajeVisible = EsMarcadorVisible(marcadorPersonaje);
        personajePrincipal.SetActive(isPersonajeVisible);

        if (isPersonajeVisible)
        {
            // En tu método VerificarMarcadorPersonaje()
            if (isPersonajeVisible && !mostradaMensajeBienvenida)
            {
                StartCoroutine(ShowSequentialTexts(
                    "Bienvenido Pompompurin. Debes derrotar a 2 oponentes y rescatarlos del control de los malos.¡Salva a Chococat!",
                    "Para iniciar debes contar con 2 oponentes",
                    2.0f  // 2 segundos de espera entre mensajes
                ));
                mostradaMensajeBienvenida = true;
            }

            if (!personajeEnMovimiento && !esperandoSeleccion && indiceOponenteActual == -1)
            {
                // Si el personaje está visible y no estamos en selección, buscar oponentes
                BuscarMarcadoresOponentes();
            }
        }
        else
        {
            ActualizarEstadoJuego("¿Pompompurin?");
            mostradaMensajeBienvenida = false;
        }
    }

    //Corrutinas de indicaciones del juego

    IEnumerator ShowText()
    {
        textoEstadoJuego.text = ""; // Inicia con un texto vacío
        foreach (char letter in fullText)
        {
            textoEstadoJuego.text += letter;
            yield return new WaitForSeconds(delay); // Espera antes de mostrar la siguiente letra
        }
    }

    IEnumerator ShowSequentialTexts(string primerMensaje, string segundoMensaje, float tiempoEspera = 1.0f)
    {
        // Mostrar primer mensaje
        fullText = primerMensaje;
        yield return StartCoroutine(ShowText());

        // Esperar el tiempo especificado entre mensajes
        yield return new WaitForSeconds(tiempoEspera);

        // Mostrar segundo mensaje
        fullText = segundoMensaje;
        yield return StartCoroutine(ShowText());
    }

    //Función para buscar marcadores de oponentes, solo si hay al menos 2 visibles

    private void BuscarMarcadoresOponentes()
    {
        // Verificar si hay al menos dos marcadores de oponentes visibles
        indicesMarcadoresVisibles.Clear();

        for (int i = 0; i < marcadoresOponentes.Count; i++)
        {
            // Solo considerar marcadores que no han sido derrotados
            if (!oponentesDerrotados.Contains(i) && EsMarcadorVisible(marcadoresOponentes[i]))
            {
                indicesMarcadoresVisibles.Add(i);
            }
        }

        // Si hay al menos dos marcadores visibles, iniciar selección
        if (indicesMarcadoresVisibles.Count >= 2)
        {
            // Limitamos a solo 2 marcadores si hay más
            if (indicesMarcadoresVisibles.Count > 2)
            {
                indicesMarcadoresVisibles = indicesMarcadoresVisibles.GetRange(0, 2);
            }

            esperandoSeleccion = true;
            ActualizarEstadoJuego("Elegir un oponente");
        }
        else
        {
            MostrarEstadoSegunProgreso();
        }
    }


    //Funciones para mostrar mensajes y verificar condiciones de victoria
    private void MostrarEstadoSegunProgreso()
    {
        if (oponentesDerrotados.Count >= oponentesNecesariosParaGanar)
        {
            ActualizarEstadoJuego($"¡Has derrotado a {oponentesNecesariosParaGanar} oponentes!\nVe hacia Chococat.");

            // Comprobar si el marcador final está visible
            if (EsMarcadorVisible(marcadorChococat))
            {
                // Activar modelo de Chococat
                foreach (GameObject cat in chococat)
                {
                    cat.SetActive(true);
                }
            }
        }
    }

    private void VerificarCondicionesChococat()
    {
        if (oponentesDerrotados.Count >= oponentesNecesariosParaGanar && EsMarcadorVisible(marcadorChococat))
        {
            // Activar modelo de Chococat
            foreach (GameObject cat in chococat)
            {
                cat.SetActive(true);
            }

            // Verificar si el personaje está en el marcador de Chococat
            float distanciaAChococat = Vector3.Distance(personajePrincipal.transform.position, marcadorChococat.transform.position);
            if (distanciaAChococat < distanciaDeteccionChococat && personajePrincipal.activeInHierarchy)
            {
                ActualizarEstadoJuego("¡Rescataste a Chococat!");
                GanaJuego();
                return; // Salir de Update para evitar sobrescribir el mensaje
            }
            else
            {
                ActualizarEstadoJuego("¡Has derrotado a " + oponentesNecesariosParaGanar + " oponentes! Ve hacia Chococat para ganar.");
            }

            // Si no estamos en combate ni esperando selección, permitir ir a Chococat
            if (!enCombate && !esperandoSeleccion && !personajeEnMovimiento &&
                distanciaAChococat > distanciaMinimaCombate)
            {
                // Mover automáticamente hacia Chococat
                StartCoroutine(MoverPersonaje(marcadorChococat.transform.position));
            }
        }
    }

    private void DetectarGestoSwipe()
    {
        // Comprobar si hay toques en la pantalla
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    // Guardar la posición inicial del toque
                    touchStartPosition = touch.position;
                    isTouchTracking = true;
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (isTouchTracking)
                    {
                        // Calcular la distancia horizontal del deslizamiento
                        float swipeDistanceX = touch.position.x - touchStartPosition.x;

                        // Verificar si la distancia es suficiente para considerarlo un swipe
                        if (Mathf.Abs(swipeDistanceX) > minSwipeDistance)
                        {
                            // Seleccionar marcador según dirección del swipe (0 para izquierda, 1 para derecha)
                            SeleccionarMarcadorPorGesto(swipeDistanceX > 0 ? 1 : 0);
                        }

                        isTouchTracking = false;
                    }
                    break;
            }
        }
    }

    private void SeleccionarMarcadorPorGesto(int indiceSeleccion)
    {
        if (!esperandoSeleccion || indicesMarcadoresVisibles.Count != 2)
            return;

        // Obtener el índice real del marcador seleccionado
        indiceOponenteActual = indicesMarcadoresVisibles[indiceSeleccion];
        ObserverBehaviour marcadorDestino = marcadoresOponentes[indiceOponenteActual];

        // Verificar que el marcador siga visible
        if (EsMarcadorVisible(marcadorDestino))
        {
            // Desactivar selección
            esperandoSeleccion = false;

            // Indicar la selección al usuario
            ActualizarEstadoJuego("Oponente " + (indiceSeleccion == 0 ? "izquierdo" : "derecho") + " seleccionado");

            // Mover el personaje al marcador seleccionado
            StartCoroutine(MoverPersonaje(marcadorDestino.transform.position));
        }
        else
        {
            // Si el marcador ya no es visible, cancelar la selección
            esperandoSeleccion = false;
            indicesMarcadoresVisibles.Clear();
            ActualizarEstadoJuego("¿Pompompurin?");
        }
    }

    private IEnumerator MoverPersonaje(Vector3 posicionDestino)
    {
        personajeEnMovimiento = true;

        //Mensaje específico según destino
        if (posicionDestino == marcadorChococat.transform.position)
        {
            ActualizarEstadoJuego("Avanzando hacia Chococat...");
        }
        else
        {
            ActualizarEstadoJuego("Avanzando a oponente");
        }

        Vector3 posicionInicial = personajePrincipal.transform.position;
        float tiempoTranscurrido = 0;

        while (tiempoTranscurrido < 1f)
        {
            tiempoTranscurrido += Time.deltaTime * velocidadMovimiento;
            personajePrincipal.transform.position = Vector3.Lerp(posicionInicial, posicionDestino, tiempoTranscurrido);
            yield return null;
        }

        personajeEnMovimiento = false;

        // Determinar acción según destino
        if (posicionDestino == marcadorChococat.transform.position && oponentesDerrotados.Count >= oponentesNecesariosParaGanar)
        {
            ActualizarEstadoJuego("¡Rescataste a Chococat!");
            GanaJuego();
            yield break;
        }

        // Si estamos regresando al marcador inicial tras perder, no iniciar combate
        if (Vector3.Distance(posicionDestino, marcadorPersonaje.transform.position) < distanciaMinimaCombate)
        {
            ReiniciarVariablesCombate();
            yield break;
        }

        // Iniciar el combate
        IniciarCombate(posicionDestino);
    }

    private void IniciarCombate(Vector3 posicionDestino)
    {
        enCombate = true;

        // Activar el modelo del oponente seleccionado
        oponenteActual = modelosOponentes[indiceOponenteActual % modelosOponentes.Length];
        oponenteActual.SetActive(true);

        // Posicionar el oponente en el marcador
        oponenteActual.transform.position = posicionDestino;
        ActualizarEstadoJuego("¡Llegaste al oponente!\nPresiona tu botón");

        // Activar los botones de juego
        botonPersonaje.interactable = true;
        botonOponente.interactable = false;

        LimpiarTextosPuntaje();
    }

    private void ActualizarEstadoJuego(string mensaje)
    {
        textoEstadoJuego.text = mensaje;
        Debug.Log(mensaje);
    }

    private void LimpiarTextosPuntaje()
    {
        textoPuntajePersonaje.text = "Personaje: -";
        textoPuntajeOponente.text = "Oponente: -";
    }

    public void TiraPersonaje()
    {
        //Tira un dado valores de 1 al 6
        puntajePersonaje = Random.Range(1, 7);
        textoPuntajePersonaje.text = "Personaje: " + puntajePersonaje;

        botonPersonaje.interactable = false;
        botonOponente.interactable = true;

        ActualizarEstadoJuego("Obtuviste: " + puntajePersonaje + "\nTurno del oponente.");
    }

    public void TiraOponente()
    {
        puntajeOponente = Random.Range(1, 7);
        textoPuntajeOponente.text = "Oponente: " + puntajeOponente;

        botonOponente.interactable = false;

        // Determinar el ganador de la ronda
        if (puntajePersonaje > puntajeOponente)
        {
            victoriasPersonaje++;
            ActualizarEstadoJuego("¡Ganaste esta ronda!\n(" + victoriasPersonaje + " de 2)");

            // Si el personaje ha vencido completamente al oponente
            if (victoriasPersonaje >= 2)
            {
                ActualizarEstadoJuego("Oponente vencido. \nPon otro marcador.");
                // Agregar el oponente a la lista de derrotados
                if (!oponentesDerrotados.Contains(indiceOponenteActual))
                {
                    oponentesDerrotados.Add(indiceOponenteActual);
                }
                esperandoNuevosMarcadores = true;
                StartCoroutine(FinalizarCombate(true));
            }
            else
            {
                StartCoroutine(PrepararNuevaRonda());
            }
        }
        else if (puntajePersonaje < puntajeOponente)
        {
            victoriasOponente++;
            ActualizarEstadoJuego("Perdiste esta ronda... \n(" + victoriasOponente + " de 2)");

            // Si el oponente ha vencido completamente al personaje
            if (victoriasOponente >= 2)
            {
                ActualizarEstadoJuego("Has perdido el combate.\nRegresando al inicio...");
                StartCoroutine(FinalizarCombate(false));
            }
            else
            {
                StartCoroutine(PrepararNuevaRonda());
            }
        }
        else
        {
            ActualizarEstadoJuego("¡Empate! Vuelve a tirar.");
            botonPersonaje.interactable = true;
        }
    }

    private IEnumerator PrepararNuevaRonda()
    {
        yield return new WaitForSeconds(2.0f);

        LimpiarTextosPuntaje();

        botonPersonaje.interactable = true;
        botonOponente.interactable = false;

        ActualizarEstadoJuego("Nueva ronda.\nTira para continuar.");
    }

    private IEnumerator FinalizarCombate(bool personajeGano)
    {
        yield return new WaitForSeconds(2.0f);

        if (personajeGano)
        {
            ActualizarEstadoJuego("¡Has ganado el combate!\nAvanza.");
            // Desactivar el modelo del oponente
            if (oponenteActual != null)
            {
                oponenteActual.SetActive(false);
            }

            // Resetear el oponente actual
            oponenteActual = null;

            // Verificar el número de oponentes derrotados
            if (oponentesDerrotados.Count >= oponentesNecesariosParaGanar)
            {
                ActualizarEstadoJuego($"¡Ya has derrotado a {oponentesNecesariosParaGanar} oponentes!\nBusca a Chococat para ganar.");
            }
            else
            {
                ActualizarEstadoJuego("Oponentes derrotados: " + oponentesDerrotados.Count + "\nBusca más oponentes.");
            }

            // Activar espera para ver nuevos marcadores
            StartCoroutine(EsperarNuevosMarcadores());
        }
        else
        {
            ActualizarEstadoJuego("Has perdido el combate.\nRegresando al inicio...");
            // Desactivar el modelo del oponente
            if (oponenteActual != null)
            {
                oponenteActual.SetActive(false);
            }
            // Regresar al personaje a su posición inicial
            StartCoroutine(MoverPersonaje(marcadorPersonaje.transform.position));
        }

        // Resetear variables
        ReiniciarVariablesCombate();
    }

    private void ReiniciarVariablesCombate()
    {
        // Resetear contadores
        victoriasPersonaje = 0;
        victoriasOponente = 0;
        puntajePersonaje = 0;
        puntajeOponente = 0;

        // Resetear estado de combate
        enCombate = false;
        indiceOponenteActual = -1;

        // Desactivar botones de combate
        botonPersonaje.interactable = false;
        botonOponente.interactable = false;

        // Limpiar textos
        LimpiarTextosPuntaje();
    }

    private IEnumerator EsperarNuevosMarcadores()
    {
        esperandoNuevosMarcadores = true;

        // Esperar un tiempo para que el usuario mueva la cámara y busque nuevos marcadores
        yield return new WaitForSeconds(3.0f);

        // Salir del modo combate
        enCombate = false;
        esperandoNuevosMarcadores = false;
        indiceOponenteActual = -1;

        // Verificar estado del juego
        ActualizarEstadoSegunProgreso();
    }

    private void ActualizarEstadoSegunProgreso()
    {
        int oponentesRestantes = marcadoresOponentes.Count - oponentesDerrotados.Count;
        int oponentesDerrotadosCount = oponentesDerrotados.Count;

        if (oponentesDerrotadosCount >= oponentesNecesariosParaGanar)
        {
            ActualizarEstadoJuego($"¡Ya has derrotado a {oponentesNecesariosParaGanar} oponentes!\nBusca a Chococat para ganar.");
        }
        else if (oponentesRestantes > 0)
        {
            // Indicar que ya puede seleccionar nuevos marcadores
            ActualizarEstadoJuego("Listo para seguir.\nBusca 2 marcadores para elegir\nOponentes restantes: " + oponentesRestantes);
        }
        else
        {
            ActualizarEstadoJuego("¡Has derrotado a todos los oponentes!\nBusca a Chococat para finalizar.");
        }
    }

    public void GanaJuego()
    {
        // Verificar que se hayan derrotado suficientes oponentes
        if (oponentesDerrotados.Count < oponentesNecesariosParaGanar)
        {
            ActualizarEstadoJuego($"Debes derrotar al menos {oponentesNecesariosParaGanar} oponentes primero.");
            return;
        }

        // Desactivar interacción
        esperandoSeleccion = false;
        personajeEnMovimiento = false;
        enCombate = false;

        // Desactivar botones
        botonPersonaje.interactable = false;
        botonOponente.interactable = false;

        // Mostrar mensaje de victoria con texto específico de rescate
        textoEstadoJuego.text = "¡Rescataste a Chococat!";

        // Desactivar todos los oponentes
        foreach (GameObject oponente in modelosOponentes)
        {
            oponente.SetActive(false);
        }

        // Activar todos los Chococat
        foreach (GameObject cat in chococat)
        {
            cat.SetActive(true);
        }
    }

    public void ReiniciarJuego()
    {
        ActualizarEstadoJuego("Reiniciando...");
        //CARGAR LA ESCENA DE NUEVO
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}