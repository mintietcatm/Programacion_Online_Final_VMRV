using Fusion;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MenuPrincipal : MonoBehaviour
{
    public PhotonManager photonManager;

    public GameObject uiPrincipal;
    public GameObject pantallaInicio;
    public GameObject panelMenu;
    public GameObject panelServidores;
    public GameObject panelCrear;
    public GameObject panelUnirse;

    public TMP_Text mensaje;
 

    public TMP_InputField nombreCrear;
    public TMP_InputField cantidadJugadores;
    public Toggle partidaPrivada;
    public TMP_InputField nombreUnirse;


    private bool ocupado;
    private bool conectado;


    private void Start()
    {
        OcultarPaneles();
        uiPrincipal.SetActive(true);
        pantallaInicio.SetActive(true);

        mensaje.text = "";

    }

    private void Update()
    {
        if (pantallaInicio.activeSelf && !ocupado)
        {
            if (Keyboard.current != null)
            {
                if (Keyboard.current.anyKey.wasPressedThisFrame)
                {
                    EntrarAlMenu();
                }
            }
        }
    }

    private async void EntrarAlMenu()
    {
        ocupado = true;

        mensaje.text = "Conectando...";

        conectado = await photonManager.ConnectToServerLobby();

        if (conectado)
        {
            OcultarPaneles();
            panelMenu.SetActive(true);
            mensaje.text = "";
        }

        else
        {
            mensaje.text = "Error, no se pudo conectar. Intentalo otra vez";
        }
        ocupado = false;
    }

    public void AbrirMenu()
    {
        if (ocupado)
        {
            return;
        }

        OcultarPaneles();
        panelMenu.SetActive(true);

    }

    public void AbrirServidores()

    {
        if (ocupado)
        {
            return;
        }

        OcultarPaneles();
        panelServidores.SetActive(true);
    }

    public void AbrirCrear()
    {
        if (ocupado)
        {
            return;
        }

        OcultarPaneles();
        panelCrear.SetActive(true);
    }

    public void AbrirUnirse()
    {
        if (ocupado)
        {
            return;
        }

        OcultarPaneles();
        panelUnirse.SetActive(true);
    }

 

    public async void CrearPartida()
    {
        string nombre = nombreCrear.text.Trim(); //trim quita los espacios al inicio y final del texto pa que no los cuente como parte del string
        int jugadores;

        if (nombre == "") //osea, si el nombre esta vacio
        {
            mensaje.text = "Inserta nombre de la partida";
            return;
        }

        if (!int.TryParse(cantidadJugadores.text, out jugadores)) //el try parse es como el to string. lo convierte en un entero. try pq devuelve un booleano. 
        {
            mensaje.text = "De cuantos jugadores es la partida";
            return;
        }

        await IniciarPartida(GameMode.Host, nombre, jugadores, partidaPrivada.isOn);

    }

    private async void UnirsePartida()
    {
        string nombre = nombreUnirse.text.Trim();

        if (nombre == "")
        {
            mensaje.text = "Inserta nombre de la partida";
            return;
        }

        await IniciarPartida(GameMode.Client, nombre, 2, false);
    }

    private async Task IniciarPartida(GameMode mode, string nombre, int jugadores, bool privada)
    {
        if (ocupado)
        {
            return;
        }

        ocupado = true;
        mensaje.text = "Conectando...";

        if (!conectado) //si no esta conectado se intenta conectar
        {
            conectado = await photonManager.ConnectToServerLobby();
        }

        if (!conectado) //si aun no se puede conectar es pq hay error
        {
            Debug.Log("No se pudo conectar");
            mensaje.text = "No se pudo conectar";
            return;
        }

        bool resultado = await photonManager.StartGame(mode, nombre, jugadores, privada);

        if (resultado)
        {
            OcultarPaneles();
        }
        ocupado = false;
    }

    public async void PartidaRapida()
    {
        await IniciarPartida(GameMode.Host, null, 2, false); //aqui rellena lo que necesita el metodo iniciar partida
    }

    public async void PartidaRandom()
    {
        await IniciarPartida(GameMode.Client, null, 2, false); //mismo que arriba
    }

    private void OcultarPaneles() //ponemos todo en false pa que no se queden puestos
    {
        pantallaInicio.SetActive(false);
        panelMenu.SetActive(false);
        panelServidores.SetActive(false);
        panelCrear.SetActive(false);
        panelUnirse.SetActive(false);
    }
}
