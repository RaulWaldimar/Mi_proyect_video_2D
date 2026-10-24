using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class MovimientoGato : MonoBehaviour
{
    [Header("Referencias")]
    public Transform puntoPies;

    [Header("Movimiento")]
    public float velocidad = 6f;

    [Header("Salto")]
    [Tooltip("Fuerza del primer salto desde el suelo (altura realista de ~1.5 bloques).")]
    public float fuerzaSalto = 7.5f;

    [Tooltip("Fuerza del segundo salto en el aire (doble salto).")]
    public float fuerzaDobleSalto = 6.8f;

    [Tooltip("Máximo número de saltos (2 = salto normal + doble salto).")]
    public int maxSaltos = 2;

    [Tooltip("Escala de gravedad del gato para un salto ágil y no flotante.")]
    public float escalaGravedad = 1.8f;

    [Tooltip("Multiplicador de caída para que el descenso sea ágil y felino.")]
    public float multiplicadorCaida = 1.4f;

    [Header("Detección de Suelo")]
    public LayerMask capaSuelo;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator animator;
    private float horizontal;

    private int saltosRestantes;
    private bool enSuelo;
    private float tiempoDesdeSalto = 1f;
    private float tiempoUltimoSuelo;
    private const float CoyoteTime = 0.12f;

    void Awake()
    {
        InicializarComponentes();
    }

    void Start()
    {
        InicializarComponentes();
        saltosRestantes = maxSaltos;

        // Auto-detectar capa Suelo si no fue asignada en el Inspector
        if (capaSuelo.value == 0)
        {
            int layerSuelo = LayerMask.NameToLayer("Suelo");
            if (layerSuelo != -1)
            {
                capaSuelo = 1 << layerSuelo;
            }
            else
            {
                capaSuelo = LayerMask.GetMask("Default");
            }
        }
    }

    void InicializarComponentes()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();

        if (rb != null)
        {
            rb.gravityScale = escalaGravedad;
        }

        if (puntoPies == null)
        {
            Transform hijo = transform.Find("PuntoPies");
            if (hijo != null) puntoPies = hijo;
        }
    }

    void Update()
    {
        tiempoDesdeSalto += Time.deltaTime;

        // 1. Movimiento horizontal
        horizontal = 0f;

        try
        {
            horizontal = Input.GetAxisRaw("Horizontal");
        }
        catch { }

        if (Mathf.Abs(horizontal) < 0.01f)
        {
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) horizontal += 1f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) horizontal -= 1f;
        }

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            var kb = Keyboard.current;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) horizontal = 1f;
            else if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) horizontal = -1f;
        }

        if (Gamepad.current != null)
        {
            float stickX = Gamepad.current.leftStick.x.ReadValue();
            if (Mathf.Abs(stickX) > 0.2f) horizontal = Mathf.Sign(stickX);
            if (Gamepad.current.dpad.right.isPressed) horizontal = 1f;
            if (Gamepad.current.dpad.left.isPressed) horizontal = -1f;
        }
#endif

        // Orientación del sprite
        if (sr != null)
        {
            if (horizontal > 0.01f) sr.flipX = false;
            else if (horizontal < -0.01f) sr.flipX = true;
        }

        // 2. Detección de pulsación de salto (Espacio, W, Flecha Arriba, Gamepad)
        bool intentoSalto = false;
        try
        {
            intentoSalto = Input.GetButtonDown("Jump");
        }
        catch { }

        if (!intentoSalto)
        {
            intentoSalto = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
        }

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            var kb = Keyboard.current;
            if (kb.spaceKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame)
            {
                intentoSalto = true;
            }
        }

        if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
        {
            intentoSalto = true;
        }
#endif

        // 3. Ejecución del salto / doble salto
        if (intentoSalto)
        {
            ProcesarSalto();
        }

        // Salto variable: si se suelta el botón rápido mientras asciende, corta la subida para toques cortos
        bool soltoSalto = false;
        try { soltoSalto = Input.GetButtonUp("Jump"); } catch { }
        if (!soltoSalto)
        {
            soltoSalto = Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow);
        }
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            var kb = Keyboard.current;
            if (kb.spaceKey.wasReleasedThisFrame || kb.wKey.wasReleasedThisFrame || kb.upArrowKey.wasReleasedThisFrame)
            {
                soltoSalto = true;
            }
        }
        if (Gamepad.current != null && Gamepad.current.buttonSouth.wasReleasedThisFrame)
        {
            soltoSalto = true;
        }
#endif
        if (soltoSalto && rb != null && rb.linearVelocity.y > 1.5f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.55f);
        }

        // 4. Parámetros del Animator
        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(horizontal));
            if (rb != null)
            {
                animator.SetFloat("VerticalVelocity", rb.linearVelocity.y);
            }
            animator.SetBool("IsGrounded", enSuelo);
        }
    }

    private void ProcesarSalto()
    {
        bool enMargenSuelo = enSuelo || (Time.time - tiempoUltimoSuelo <= CoyoteTime);

        if (enMargenSuelo && saltosRestantes == maxSaltos)
        {
            // Primer salto (desde el suelo)
            EjecutarImpulsoSalto(fuerzaSalto);
            saltosRestantes = maxSaltos - 1; // Queda disponible el segundo salto (doble salto)
        }
        else if (saltosRestantes > 0)
        {
            // Segundo salto en el aire (doble salto consecutivo)
            EjecutarImpulsoSalto(fuerzaDobleSalto);
            saltosRestantes--;
        }
    }

    private void EjecutarImpulsoSalto(float fuerza)
    {
        if (rb == null) return;

        // Reinicia la velocidad vertical para que el doble salto responda de inmediato
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerza);
        enSuelo = false;
        tiempoDesdeSalto = 0f;
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        ActualizarDeteccionSuelo();

        // Movimiento horizontal en FixedUpdate
        rb.linearVelocity = new Vector2(horizontal * velocidad, rb.linearVelocity.y);

        // Caída más ágil y realista (evita efecto flotante)
        if (rb.linearVelocity.y < 0 && multiplicadorCaida > 1f)
        {
            rb.linearVelocity += Vector2.up * (Physics2D.gravity.y * rb.gravityScale * (multiplicadorCaida - 1f) * Time.fixedDeltaTime);
        }
    }

    private void ActualizarDeteccionSuelo()
    {
        // Durante los primeros 0.08s después de saltar no verificamos para permitir despegue limpio
        if (tiempoDesdeSalto > 0.08f)
        {
            Vector2 posPies = puntoPies != null 
                ? (Vector2)puntoPies.position 
                : (Vector2)transform.position + new Vector2(0, -0.6f);

            bool tocandoSuelo = Physics2D.OverlapBox(posPies, new Vector2(0.5f, 0.15f), 0f, capaSuelo);

            if (tocandoSuelo && rb.linearVelocity.y <= 0.1f)
            {
                enSuelo = true;
                tiempoUltimoSuelo = Time.time;
                saltosRestantes = maxSaltos; // Recupera el salto y doble salto al tocar suelo
            }
            else
            {
                enSuelo = false;
                // Si cayó de una plataforma sin haber saltado, consume el primer salto y le deja 1 (el salto aéreo)
                if (saltosRestantes == maxSaltos && (Time.time - tiempoUltimoSuelo > CoyoteTime))
                {
                    saltosRestantes = maxSaltos - 1;
                }
            }
        }
        else
        {
            enSuelo = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 pos = puntoPies != null 
            ? (Vector2)puntoPies.position 
            : (Vector2)transform.position + new Vector2(0, -0.6f);

        Gizmos.color = enSuelo ? Color.green : Color.red;
        Gizmos.DrawWireCube(pos, new Vector2(0.5f, 0.15f));
    }
}

// Alias para mantener compatibilidad con el nombre del archivo C# en Unity
public class MovimientoCatPlay : MovimientoGato
{
}
