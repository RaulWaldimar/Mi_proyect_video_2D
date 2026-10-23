using UnityEngine;

public class MovimientoGato : MonoBehaviour
{

    public Transform puntoPies;
    public float velocidad = 5f;
    public float fuerzaSalto = 10f;
    public LayerMask capaSuelo;   // Aquí eliges la capa del suelo

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Collider2D col;
    private float horizontal;
    private bool quiereSaltar;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    void Update()
    {
        // D = +1 (derecha), A = -1 (izquierda)
        horizontal = 0f;
        if (Input.GetKey(KeyCode.D)) horizontal = 1f;
        if (Input.GetKey(KeyCode.A)) horizontal = -1f;

        // Voltear el sprite según la dirección
        if (horizontal > 0) sr.flipX = false;
        else if (horizontal < 0) sr.flipX = true;

        // Salto solo si está en el suelo (una vez por salto)
        if (Input.GetKeyDown(KeyCode.Space) && EstaEnSuelo())
            quiereSaltar = true;
    }

    void FixedUpdate()
    {
        // En Unity 2022 o anterior cambia linearVelocity por velocity
        rb.linearVelocity = new Vector2(horizontal * velocidad, rb.linearVelocity.y);

        if (quiereSaltar)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSalto);
            quiereSaltar = false;
        }
    }

    bool EstaEnSuelo()
    {
        return Physics2D.OverlapBox(
            puntoPies.position, new Vector2(0.3f, 0.1f), 0f, capaSuelo);
    }
}