using UnityEngine;

public class InterfazUI : MonoBehaviour
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
        Debug.Log("UI reacciono al evento");
    }
}
