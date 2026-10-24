using UnityEditor;
using UnityEngine;

/// <summary>
/// Herramientas de desarrollo para la intro de historia.
/// </summary>
public static class HerramientasHistoria
{
    [MenuItem("Tools/Historia/Borrar progreso (volver a ver la historia)")]
    public static void BorrarProgreso()
    {
        PlayerPrefs.DeleteKey(HistoriaController.ClaveVista);
        PlayerPrefs.Save();
        Debug.Log("[Historia] Progreso borrado. La proxima vez que pulses 'Jugar' se vera la historia.");
    }

    [MenuItem("Tools/Historia/Marcar historia como vista")]
    public static void MarcarVista()
    {
        PlayerPrefs.SetInt(HistoriaController.ClaveVista, 1);
        PlayerPrefs.Save();
        Debug.Log("[Historia] Marcada como vista. 'Jugar' ira directo al Nivel_1.");
    }
}
