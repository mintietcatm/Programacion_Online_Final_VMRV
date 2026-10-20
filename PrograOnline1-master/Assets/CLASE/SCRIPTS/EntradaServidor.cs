using Fusion;
using NUnit.Framework;
using TMPro;
using UnityEngine;

public class EntradaServidor : MonoBehaviour
{
    public TMP_Text textoNombre;
    public TMP_Text textoJugadores;
    public TMP_Text textoCapacidad;

    public string nombreSesion;

    public void SetInfo(SessionInfo session)
    {
        nombreSesion = session.Name;

        textoNombre.text = session.Name;
        textoJugadores.text =""+ session.PlayerCount;
        textoCapacidad.text =""+ session.MaxPlayers;



    }

    }
