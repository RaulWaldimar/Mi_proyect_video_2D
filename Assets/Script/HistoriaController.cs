using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Controla la intro de historia: muestra una secuencia de imagenes y avanza
/// con el boton (o con Espacio / Enter).
///
/// Al terminar la ultima imagen, guarda en PlayerPrefs que la historia ya fue
/// vista (para no repetirla) y carga el primer nivel.
/// </summary>
public class HistoriaController : MonoBehaviour
{
    /// <summary>Clave de PlayerPrefs que indica si la historia ya se vio.</summary>
    public const string ClaveVista = "HistoriaVista";

    [Header("Contenido")]
    [Tooltip("Imagenes de la historia, en orden.")]
    [SerializeField] private Sprite[] imagenes;

    [Tooltip("Componente Image donde se muestran las imagenes.")]
    [SerializeField] private Image imagen;

    [Tooltip("Texto de progreso, ej: '1 / 4'.")]
    [SerializeField] private TMP_Text textoProgreso;

    [Header("Navegacion")]
    [SerializeField] private Button botonSiguiente;
    [SerializeField] private TMP_Text textoBoton;
    [SerializeField] private Button botonesaltar;
    [SerializeField] private TMP_Text textoBotonesaltar;

    [Tooltip("Escena que se carga al terminar la historia.")]
    [SerializeField] private string escenaSiguiente = "Nivel_1";

    [Header("Transiciones")]
    [Tooltip("Duracion del fundido entre imagenes.")]
    [SerializeField] private float duracionFundido = 0.35f;

    [Tooltip("CanvasGroup del objeto de la imagen, para hacer los fundidos.")]
    [SerializeField] private CanvasGroup grupoImagen;

    private int indice;
    private bool enTransicion;

    private void Start()
    {
        if (imagenes == null || imagenes.Length == 0 || imagen == null)
        {
            Debug.LogWarning("[HistoriaController] Faltan imagenes o la Image destino.", this);
            return;
        }

        indice = 0;
        imagen.sprite = imagenes[0];
        ActualizarInterfaz();

        if (botonSiguiente != null)
        {
            botonSiguiente.onClick.AddListener(Siguiente);
        }

        if (botonesaltar != null)
        {
            botonesaltar.onClick.AddListener(Saltar);
        }

        if (grupoImagen != null)
        {
            // La imagen de entrada se muestra al instante: un fundido de entrada
            // solo anadiria riesgo de quedarse a medias si el juego arranca lento.
            grupoImagen.alpha = 1f;
        }
    }

    private void Update()
    {
        // Avanzar con teclado (funciona porque el proyecto admite el Input Manager clasico).
        if (Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            Siguiente();
        }
    }

    /// <summary>Avanza a la siguiente imagen o termina la historia.</summary>
    public void Siguiente()
    {
        if (enTransicion)
        {
            return;
        }

        if (indice >= imagenes.Length - 1)
        {
            Terminar();
            return;
        }

        StartCoroutine(CambiarA(indice + 1));
    }

    private IEnumerator CambiarA(int nuevoIndice)
    {
        enTransicion = true;

        if (botonSiguiente != null)
        {
            botonSiguiente.interactable = false;
        }

        if (grupoImagen != null)
        {
            yield return Fundir(grupoImagen, 1f, 0f, duracionFundido);
        }

        indice = nuevoIndice;
        imagen.sprite = imagenes[indice];
        ActualizarInterfaz();

        if (grupoImagen != null)
        {
            yield return Fundir(grupoImagen, 0f, 1f, duracionFundido);
        }

        if (botonSiguiente != null)
        {
            botonSiguiente.interactable = true;
        }

        enTransicion = false;
    }

    private IEnumerator Fundir(CanvasGroup grupo, float desde, float hasta, float duracion)
    {
        float tiempo = 0f;
        grupo.alpha = desde;

        while (tiempo < duracion)
        {
            // Si el editor esta en segundo plano el delta puede ser practicamente 0
            // y el fundido se quedaria a medias; forzamos un paso minimo.
            float delta = Time.unscaledDeltaTime;
            if (delta < 0.001f)
            {
                delta = 1f / 60f;
            }

            tiempo += delta;
            grupo.alpha = Mathf.Lerp(desde, hasta, Mathf.Clamp01(tiempo / duracion));
            yield return null;
        }

        grupo.alpha = hasta;
    }

    /// <summary>Actualiza el texto de progreso y la etiqueta del boton.</summary>
    private void ActualizarInterfaz()
    {
        if (textoProgreso != null)
        {
            textoProgreso.text = $"{indice + 1} / {imagenes.Length}";
        }

        if (textoBoton != null)
        {
            textoBoton.text = (indice == imagenes.Length - 1) ? "Comenzar" : "Siguiente";
        }
    }

    /// <summary>Guarda el progreso, detiene la musica y entra al primer nivel.</summary>
    private void Terminar()
    {
        FinalizarHistoria();

        SceneManager.LoadScene(escenaSiguiente);
    }

    /// <summary>
    /// Salta la historia: termina directamente sin avanzar por las imagenes.
    /// </summary>
    public void Saltar()
    {
        FinalizarHistoria();

        SceneManager.LoadScene(escenaSiguiente);
    }

    private void FinalizarHistoria()
    {
        PlayerPrefs.SetInt(ClaveVista, 1);
        PlayerPrefs.Save();

        if (MusicManager.Instancia != null)
        {
            MusicManager.Instancia.Parar(0f);
        }
    }
}
