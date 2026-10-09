using UnityEngine;

public class Luz : MonoBehaviour
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
        Debug.Log("Luz reacciono al evento");
    }
}
