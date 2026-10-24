using UnityEngine;

public class SeguirCamara : MonoBehaviour
{
    public Transform objetivo;
    public float suavidad = 5f;
    public float desfaseX = 0f;
    public float limiteIzquierdo = 0f;
    public float limiteDerecho = 50f;

    private float alturaFija;
    private float profundidadFija;

    void Start()
    {
        // Guarda la altura y profundidad que ya tiene la camara
        alturaFija = transform.position.y;
        profundidadFija = transform.position.z;
    }

    void LateUpdate()
    {
        if (objetivo == null) return;

        float destinoX = objetivo.position.x + desfaseX;
        float nuevaX = Mathf.Lerp(transform.position.x, destinoX, suavidad * Time.deltaTime);

        nuevaX = Mathf.Clamp(nuevaX, limiteIzquierdo, limiteDerecho);

        transform.position = new Vector3(nuevaX, alturaFija, profundidadFija);
    }
}