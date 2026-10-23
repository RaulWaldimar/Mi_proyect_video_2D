using UnityEngine;

public class SeguirCamara : MonoBehaviour
{
    public Transform objetivo;
    public float suavidad = 5f;
    public Vector3 desfase = new Vector3(0f, 1f, -10f);

    void LateUpdate()
    {
        if (objetivo == null) return;
        Vector3 destino = objetivo.position + desfase;
        transform.position = Vector3.Lerp(transform.position, destino, suavidad * Time.deltaTime);
    }
}
