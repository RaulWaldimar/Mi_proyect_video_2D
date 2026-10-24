using System.Collections;
using UnityEngine;

/// <summary>
/// Administra la musica de fondo del juego.
/// Es un singleton que persiste entre escenas (DontDestroyOnLoad), por lo que
/// la musica no se corta al cambiar de nivel y tampoco se reinicia cuando el
/// jugador muere y se recarga la misma escena.
///
/// Uso: colocar este componente en un GameObject "MusicManager" en cada escena
/// y asignarle la musica correspondiente en el campo "Musica".
/// Si la musica pedida es distinta a la que ya suena, se hace un fundido cruzado.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instancia { get; private set; }

    [Header("Musica de esta escena")]
    [Tooltip("Cancion que debe sonar en esta escena. Vacio = mantener la que ya esta sonando.")]
    [SerializeField] private AudioClip musica;

    [Header("Ajustes")]
    [Range(0f, 1f)]
    [Tooltip("Volumen de la musica (0 a 1). Recomendado 0.3 - 0.5 para no tapar los efectos.")]
    [SerializeField] private float volumen = 0.4f;

    [Tooltip("Segundos que dura el fundido cruzado al cambiar de cancion.")]
    [SerializeField] private float tiempoFundido = 1.5f;

    [Tooltip("Si esta activo, la musica sigue sonando al cambiar de escena.")]
    [SerializeField] private bool persistirEntreEscenas = true;

    private AudioSource fuenteA;
    private AudioSource fuenteB;
    private AudioSource fuenteActiva;
    private AudioClip clipActual;
    private Coroutine rutinaFundido;

    /// <summary>Volumen configurado para la musica.</summary>
    public float Volumen => volumen;

    /// <summary>Clip que esta sonando actualmente (puede ser null).</summary>
    public AudioClip ClipActual => clipActual;

    private void Awake()
    {
        // Si ya existe un MusicManager (viene de otra escena), este se descarta.
        if (Instancia != null && Instancia != this)
        {
            // Pero si esta escena pide otra cancion, se la pasamos al manager vivo.
            if (musica != null && musica != Instancia.clipActual)
            {
                Instancia.Reproducir(musica, Instancia.tiempoFundido);
            }

            Destroy(gameObject);
            return;
        }

        Instancia = this;

        if (persistirEntreEscenas)
        {
            DontDestroyOnLoad(gameObject);
        }

        fuenteA = GetComponent<AudioSource>();
        ConfigurarFuente(fuenteA, 0f);

        // Segunda fuente, necesaria para el fundido cruzado.
        fuenteB = gameObject.AddComponent<AudioSource>();
        ConfigurarFuente(fuenteB, 0f);

        fuenteActiva = fuenteA;

        if (musica != null)
        {
            fuenteA.clip = musica;
            fuenteA.volume = volumen;
            fuenteA.Play();
            clipActual = musica;
        }
    }

    private void OnDestroy()
    {
        if (Instancia == this)
        {
            Instancia = null;
        }
    }

    private static void ConfigurarFuente(AudioSource fuente, float volumenInicial)
    {
        fuente.playOnAwake = false;
        fuente.loop = true;
        fuente.spatialBlend = 0f;   // 2D: se escucha igual en todo el nivel
        fuente.volume = volumenInicial;
        fuente.priority = 0;        // maxima prioridad, no la corta Unity
    }

    /// <summary>
    /// Cambia la cancion con un fundido cruzado. Si ya esta sonando esa misma
    /// cancion, no hace nada (evita reiniciarla al recargar la escena).
    /// </summary>
    public void Reproducir(AudioClip nueva, float duracion = -1f)
    {
        if (nueva == null)
        {
            return;
        }

        if (nueva == clipActual && fuenteActiva != null && fuenteActiva.isPlaying)
        {
            return;
        }

        if (duracion < 0f)
        {
            duracion = tiempoFundido;
        }

        if (rutinaFundido != null)
        {
            StopCoroutine(rutinaFundido);
            rutinaFundido = null;
        }

        if (duracion <= 0f)
        {
            CambiarSinFundido(nueva);
            return;
        }

        rutinaFundido = StartCoroutine(FundidoCruzado(nueva, duracion));
    }

    private void CambiarSinFundido(AudioClip nueva)
    {
        fuenteActiva.Stop();
        fuenteActiva.clip = nueva;
        fuenteActiva.volume = volumen;
        fuenteActiva.Play();
        clipActual = nueva;
    }

    private IEnumerator FundidoCruzado(AudioClip nueva, float duracion)
    {
        AudioSource saliente = fuenteActiva;
        AudioSource entrante = (saliente == fuenteA) ? fuenteB : fuenteA;

        entrante.clip = nueva;
        entrante.volume = 0f;
        entrante.Play();
        clipActual = nueva;

        float tiempo = 0f;
        while (tiempo < duracion)
        {
            tiempo += Time.unscaledDeltaTime;
            float avance = Mathf.Clamp01(tiempo / duracion);
            saliente.volume = Mathf.Lerp(volumen, 0f, avance);
            entrante.volume = Mathf.Lerp(0f, volumen, avance);
            yield return null;
        }

        saliente.Stop();
        saliente.clip = null;
        saliente.volume = 0f;

        entrante.volume = volumen;
        fuenteActiva = entrante;
        rutinaFundido = null;
    }

    /// <summary>Detiene la musica con un fundido de salida.</summary>
    public void Parar(float duracion = -1f)
    {
        if (fuenteActiva == null)
        {
            return;
        }

        if (duracion < 0f)
        {
            duracion = tiempoFundido;
        }

        if (rutinaFundido != null)
        {
            StopCoroutine(rutinaFundido);
            rutinaFundido = null;
        }

        if (duracion <= 0f)
        {
            fuenteA.Stop();
            fuenteB.Stop();
            fuenteA.volume = 0f;
            fuenteB.volume = 0f;
            clipActual = null;
            return;
        }

        StartCoroutine(FundidoSalida(duracion));
    }

    private IEnumerator FundidoSalida(float duracion)
    {
        float volumenInicial = fuenteActiva.volume;
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.unscaledDeltaTime;
            fuenteActiva.volume = Mathf.Lerp(volumenInicial, 0f, Mathf.Clamp01(tiempo / duracion));
            yield return null;
        }

        fuenteA.Stop();
        fuenteB.Stop();
        fuenteA.volume = 0f;
        fuenteB.volume = 0f;
        clipActual = null;
    }

    /// <summary>Cambia el volumen de la musica en tiempo real (util para un slider de opciones).</summary>
    public void EstablecerVolumen(float nuevoVolumen)
    {
        volumen = Mathf.Clamp01(nuevoVolumen);

        if (rutinaFundido != null || fuenteActiva == null)
        {
            return;
        }

        fuenteActiva.volume = volumen;
    }
}
