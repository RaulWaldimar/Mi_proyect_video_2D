using UnityEngine;

public class MovimientoGato : MonoBehaviour
{
    public Transform puntoPies;
    public float velocidad = 5f;
    public float fuerzaSalto = 10f;
    public int maxSaltos = 2;          // 2 = doble salto
    public LayerMask capaSuelo;        // Aquí eliges la capa del suelo

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator animator;
    private float horizontal;
    private bool quiereSaltar;
    private bool enSuelo;
    private int saltosHechos;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
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

        // Salto: permite hasta maxSaltos antes de tocar el suelo
        if (Input.GetKeyDown(KeyCode.Space) && saltosHechos < maxSaltos)
        {
            quiereSaltar = true;
            saltosHechos++;
        }

        // Parámetros del Animator (los nombres deben coincidir con los del Animator)
        animator.SetFloat("Speed", Mathf.Abs(horizontal));
        animator.SetFloat("VerticalVelocity", rb.linearVelocity.y);
        animator.SetBool("IsGrounded", enSuelo);
    }

    void FixedUpdate()
    {
        // Revisa el suelo en los pies
        enSuelo = Physics2D.OverlapBox(
            puntoPies.position, new Vector2(0.3f, 0.1f), 0f, capaSuelo);

        if (enSuelo && rb.linearVelocity.y <= 0.1f)
            saltosHechos = 0;                 // Al tocar el suelo se recuperan los saltos
        else if (!enSuelo && saltosHechos == 0)
            saltosHechos = 1;                 // Si se cae sin saltar, queda 1 salto en el aire

        // En Unity 2022 o anterior cambia linearVelocity por velocity
        rb.linearVelocity = new Vector2(horizontal * velocidad, rb.linearVelocity.y);

        if (quiereSaltar)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSalto);
            quiereSaltar = false;
        }
    }
}