using Fusion;
using TMPro;
using UnityEngine;

public class EntradaServidor : MonoBehaviour //session entry
{
    [SerializeField] public TMP_Text textoNombre;
    [SerializeField] public TMP_Text textoJugadores;
    [SerializeField] public TMP_Text textoCapacidad;

    public string nombreSesion;

    public void SetInfo(SessionInfo session)
    {
        nombreSesion = session.Name;

        textoNombre.text = session.Name;
        textoJugadores.text = "Jugadores:" + session.PlayerCount;
        textoCapacidad.text = "Capacidad:" + session.MaxPlayers;



    }

    public void JoinGame()
    {
        //PhotonManager.Instance.StartGame(Fusion.GameMode.Client, textoNombre.text);
    }

}
