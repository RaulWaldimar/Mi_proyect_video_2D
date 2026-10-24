using UnityEngine;
using UnityEngine.SceneManagement;

public class menuinicial : MonoBehaviour
{
    /// <summary>Escena de la intro de historia.</summary>
    private const string EscenaHistoria = "Historia_personaje";

    /// <summary>Primer nivel del juego.</summary>
    private const string EscenaNivel1 = "Nivel_1";

    public void jugar()
    {
        // La intro de historia se muestra siempre al pulsar Jugar.
        // El boton "Saltar historia" permite omitirla si el jugador ya la vio.
        SceneManager.LoadScene(EscenaHistoria);
    }

    public void salir()
    {
        Debug.Log("Salir......");
        Application.Quit();
    }
}
