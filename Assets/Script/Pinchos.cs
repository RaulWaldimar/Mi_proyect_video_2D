using UnityEngine;

public class Pinchos : MonoBehaviour
{
    public int dano = 20;
    public float tiempoEntreDano = 0.5f;
    float proximoDano;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (Time.time < proximoDano) return;

        VidaCat_Player vida = other.GetComponentInParent<VidaCat_Player>();
        if (vida != null)
        {
            vida.RecibirDanio(dano);
            proximoDano = Time.time + tiempoEntreDano;
        }
    }
}