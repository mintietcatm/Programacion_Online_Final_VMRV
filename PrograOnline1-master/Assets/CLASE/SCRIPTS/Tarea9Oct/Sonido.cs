using UnityEngine;

public class Sonido : MonoBehaviour
{
    private void OnEnable()
    {
        EventSystem.eventoGeneral += Mensaje;
    }

    private void OnDisable()
    {
        EventSystem.eventoGeneral -= Mensaje;
    }

    private void Mensaje()
    {
        Debug.Log("Sonido reacciono al evento");
    }
}
