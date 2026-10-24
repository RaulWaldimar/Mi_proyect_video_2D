using UnityEngine;
using UnityEngine.SceneManagement;

public class FinNivel2 : MonoBehaviour
{
    // Cambia "menuinicial" por "menuinicio"
    private const string EscenaMenuInicial = "menuinicio";
    private const string TagJugador = "Player";

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(TagJugador))
        {
            Debug.Log("¡FINAL DEL JUEGO!");
            SceneManager.LoadScene(EscenaMenuInicial);
        }
    }
}