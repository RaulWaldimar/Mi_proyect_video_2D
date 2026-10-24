using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class VidaCat_Player : MonoBehaviour
{
    public int vidaMaxima = 100;
    public Slider barraVida;              // Arrastra aqui el Slider BarraVida
    public float tiempoReinicio = 1f;     // Segundos antes de reiniciar el nivel al morir

    private int vidaActual;
    private bool muerto;

    void Start()
    {
        vidaActual = vidaMaxima;
        if (barraVida != null)
        {
            barraVida.maxValue = vidaMaxima;
            barraVida.value = vidaActual;
        }
    }

    // La llama el enemigo cuando ataca
    public void RecibirDanio(int cantidad)
    {
        if (muerto) return;

        vidaActual = Mathf.Max(vidaActual - cantidad, 0);
        if (barraVida != null) barraVida.value = vidaActual;

        if (vidaActual <= 0) Morir();
    }

    void Morir()
    {
        muerto = true;

        // Desactiva el movimiento
        MovimientoGato mov = GetComponent<MovimientoGato>();
        if (mov != null) mov.enabled = false;

        Invoke(nameof(ReiniciarNivel), tiempoReinicio);
    }

    void ReiniciarNivel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}