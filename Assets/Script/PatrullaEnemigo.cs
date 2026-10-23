using UnityEngine;

public class PatrullaEnemigo : MonoBehaviour
{
    public float velocidad = 2f;
    public float distancia = 3f;       // Cuanto se aleja del punto inicial a cada lado

    private float limiteIzquierdo;
    private float limiteDerecho;
    private int direccion = 1;         // 1 = derecha, -1 = izquierda
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        limiteIzquierdo = transform.position.x - distancia;
        limiteDerecho = transform.position.x + distancia;
    }

    void Update()
    {
        // Se mueve solo en X
        transform.position += new Vector3(direccion * velocidad * Time.deltaTime, 0f, 0f);

        // Cambia de direccion al llegar a un limite
        if (transform.position.x >= limiteDerecho) direccion = -1;
        else if (transform.position.x <= limiteIzquierdo) direccion = 1;

        // Voltea el sprite (si mira al reves, invierte true y false)
        if (sr != null) sr.flipX = direccion > 0;
    }

    // Dibuja los limites en la Scene para ajustarlos facil
    void OnDrawGizmosSelected()
    {
        float x = Application.isPlaying ? (limiteIzquierdo + limiteDerecho) / 2f : transform.position.x;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(x - distancia, transform.position.y, 0), new Vector3(x + distancia, transform.position.y, 0));
    }
}
