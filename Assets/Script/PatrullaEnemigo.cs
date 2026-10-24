using System.Collections;
using UnityEngine;

public class PatrullaEnemigo : MonoBehaviour
{
    [Header("Patrulla")]
    public float velocidad = 2f;
    public float distancia = 3f;               // Cuanto se aleja del punto inicial a cada lado

    [Header("Ataque")]
    public float rangoAtaque = 1.2f;           // Distancia horizontal a la que ataca al gato
    public float alturaAtaque = 1f;            // Diferencia de altura maxima para atacar
    public float esperaEntreAtaques = 1f;      // Cada cuantos segundos hace dano
    public int danio = 10;                     // Vida que le quita al gato en cada ataque

    [Header("Muerte")]
    public float fuerzaRebote = 8f;            // Impulso que recibe el gato al pisar al enemigo
    public float esperaDespuesDeMorir = 0.3f;  // Tiempo que el cuerpo queda en el suelo al terminar la animacion

    private float limiteIzquierdo;
    private float limiteDerecho;
    private int direccion = 1;                 // 1 = derecha, -1 = izquierda
    private SpriteRenderer sr;
    private Animator anim;
    private Collider2D miCollider;
    private Rigidbody2D rb;
    private MovimientoGato gato;
    private Collider2D gatoCollider;
    private VidaCat_Player vidaGato;
    private bool muerto;
    private float proximoAtaque;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        miCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        gato = FindAnyObjectByType<MovimientoGato>();
        if (gato != null)
        {
            gatoCollider = gato.GetComponent<Collider2D>();
            vidaGato = gato.GetComponent<VidaCat_Player>();
        }

        limiteIzquierdo = transform.position.x - distancia;
        limiteDerecho = transform.position.x + distancia;
    }

    void Update()
    {
        if (muerto) return;

        bool atacando = GatoAlLado();
        anim.SetBool("Atacando", atacando);
        anim.SetBool("Caminando", !atacando);

        if (atacando)
        {
            // Mira hacia el gato y le hace dano cada cierto tiempo
            direccion = gato.transform.position.x > transform.position.x ? 1 : -1;
            if (sr != null) sr.flipX = direccion > 0;

            if (Time.time >= proximoAtaque)
            {
                proximoAtaque = Time.time + esperaEntreAtaques;
                if (vidaGato != null) vidaGato.RecibirDanio(danio);
            }
            return;   // Mientras ataca no patrulla
        }

        // Se mueve solo en X
        transform.position += new Vector3(direccion * velocidad * Time.deltaTime, 0f, 0f);

        // Cambia de direccion al llegar a un limite
        if (transform.position.x >= limiteDerecho) direccion = -1;
        else if (transform.position.x <= limiteIzquierdo) direccion = 1;

        // Voltea el sprite (si mira al reves, invierte true y false)
        if (sr != null) sr.flipX = direccion > 0;
    }

    // El gato esta a su lado (no encima, no lejos)
    bool GatoAlLado()
    {
        if (gato == null || gatoCollider == null) return false;

        float dx = Mathf.Abs(gato.transform.position.x - transform.position.x);
        float dy = Mathf.Abs(gato.transform.position.y - transform.position.y);
        bool encima = gatoCollider.bounds.min.y > miCollider.bounds.center.y;

        return dx <= rangoAtaque && dy <= alturaAtaque && !encima;
    }

    void OnCollisionEnter2D(Collision2D c) { RevisarPisada(c); }
    void OnCollisionStay2D(Collision2D c) { RevisarPisada(c); }

    void RevisarPisada(Collision2D c)
    {
        if (muerto) return;

        MovimientoGato g = c.gameObject.GetComponent<MovimientoGato>();
        if (g == null) return;

        Rigidbody2D gatoRb = c.rigidbody;
        float piesGato = c.collider.bounds.min.y;
        float centroEnemigo = miCollider.bounds.center.y;

        // Pisada: los pies del gato estan arriba del centro del enemigo y el gato no va subiendo
        if (piesGato > centroEnemigo && gatoRb.linearVelocity.y <= 0.1f)
        {
            gatoRb.linearVelocity = new Vector2(gatoRb.linearVelocity.x, fuerzaRebote);
            Morir();
        }
    }

    void Morir()
    {
        muerto = true;
        anim.SetBool("Atacando", false);
        anim.SetBool("Caminando", false);
        anim.SetTrigger("Morir");

        // Congela la fisica para que el cuerpo se quede en el suelo
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }
        miCollider.enabled = false;

        StartCoroutine(EsperarYDesaparecer());
    }

    IEnumerator EsperarYDesaparecer()
    {
        // Espera a que el Animator entre al estado de muerte (maximo 1 segundo)
        float espera = 0f;
        yield return null;
        while (!anim.GetCurrentAnimatorStateInfo(0).IsName("Enemigo_Die") && espera < 1f)
        {
            espera += Time.deltaTime;
            yield return null;
        }

        // Espera lo que dura la animacion de morir
        float duracion = anim.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(duracion + esperaDespuesDeMorir);

        Destroy(gameObject);
    }

    // Dibuja el recorrido (rojo) y el rango de ataque (amarillo) en la Scene
    void OnDrawGizmosSelected()
    {
        float x = Application.isPlaying ? (limiteIzquierdo + limiteDerecho) / 2f : transform.position.x;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(x - distancia, transform.position.y, 0), new Vector3(x + distancia, transform.position.y, 0));

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(rangoAtaque * 2f, alturaAtaque * 2f, 0f));
    }
}