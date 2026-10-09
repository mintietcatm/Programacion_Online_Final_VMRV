using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;


/// <summary>
/// Callback: Un método que se pasa a otra como argumento. Existe una función receptora que se ejecuta mas tarde.
/// Su función principal es indicar que hacer justo después de que concluya un proceso.
/// </summary>
public class PhotonManager : MonoBehaviour, INetworkRunnerCallbacks

    {
    private NetworkRunner networkRunner; //el network runner es quien controla todas las funciones de red en UNITY
    [SerializeField] private UnityEvent onPlayerJoined;
    [SerializeField] private NetworkPrefabRef playerPrefab;
    [SerializeField] private Dictionary<PlayerRef, NetworkObject> players = new Dictionary<PlayerRef, NetworkObject>();
    [SerializeField] private GameObject menuPanel;
    private StormZone stormZone;
    private AmmoPickup ammoPickup;
    private RiflePickup riflePickup;

    private void Awake()
    {
        Debug.Log("Awake");
        networkRunner = GetComponent<NetworkRunner>();
    }

    // En este lobby el server (photon) sabe que existes de ahi tu puedes ir a cualquier lado, ya sea una partida una party o cualquier lugar del menu que el juego te permita

    // Ejercicio 

    //Mientras te esta conectando, deben de hacer aparecer en algun lado un mensaje de conectando. 
    //Si ya te logro conectar, ahora si debe de aparecer la pantalla de menu principal

    //pueden crear otro metodo si lo necesitan, pero no pueden borrar o no usar la tarea asincrona

    public async Task ConnectToServerLobby()
    {
        // JoinSessionLobby sirve pasra conectarte a la partida, mas no iniciarla
        // Nos puede conectar a una sala global o puede conectar a varios jugadores a una sala sin iniciar una partida

        await networkRunner.JoinSessionLobby(SessionLobby.ClientServer);
    }

    private void Start() // Cuando inicia el script
    {
        Debug.Log("Start");
        networkRunner.AddCallbacks(this); // Este runner va a tener vinculadas las callbacks de este script

        //SceneManager.LoadScene(0);//LoadSceneMode.Single por defecto
        //SceneManager.LoadScene(0,LoadSceneMode.Additive);//Carga una escena sobre otra
    }

    // Tarea asincrona que nos inicia la partida

private async Task StartGame(GameMode mode)
    {
        SceneRef gameScene = SceneRef.FromIndex(0); // Referencia de donde esta la escena, ya sea por índice o nombre

        NetworkSceneInfo sceneInfo = new NetworkSceneInfo(); // En esta variable voy a guardar las escenas a usar en mi juego, solo mientras estoy jugando
                                                             // 0 menu / 1 primer mapa / 2 segundo mapa / 3 menu de zombies

        if (gameScene.IsValid) // revisa que el índice sea válido
        {
            sceneInfo.AddSceneRef(gameScene,LoadSceneMode.Additive); // añadimos la escena a la lista de escenas que podemos usar
        }
        else
        {
            Debug.LogError("Escena no valida");
        }

        // argumentos son como los parametros de un método, sin embargo, argumentos se refiere a los datos que pide una llamada a la api
        await networkRunner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "00001", // esto me sirve para que solo haya una partida en todo el juego, no puede haber más de una sesion con el mismo nombre
            Scene = sceneInfo,
            CustomLobbyName = "Loby de pruebas",
            PlayerCount = 2, 
        });
    }


public void StartGameAsHost()
    {
        if (menuPanel != null) menuPanel.SetActive(false);

        Task startGame = StartGame(GameMode.Host);
    }


public void StartGameAsClient()
    {
        if (menuPanel != null) menuPanel.SetActive(false);

        Task startGame = StartGame(GameMode.Client);
    }

    //Se manda lamar cuando te conectas, ya sea como clliente o host
    public void OnConnectedToServer(NetworkRunner runner)
    {
        //throw new NotImplementedException();
    }
    //Cuando empieza a intentar conectarte al server pero falla
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        //throw new NotImplementedException();
    }
    //Cuando empieza a intentar conectarte al server
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        //throw new NotImplementedException();
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        //throw new NotImplementedException();
    }
    //Cuando te desconectas, se va el internet o cierras el juego
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        //throw new NotImplementedException();
    }
    //Alguien sale
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        //throw new NotImplementedException();
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        Camera localCamera = Camera.main;

        NetworkInfoData data = new NetworkInfoData()
        {
            movementAxis = InputManager.Instance.GetMoveInput(),
            mouseMovement = InputManager.Instance.GetMouseDelta(),
            aimDirection = localCamera != null ? localCamera.transform.forward : Vector3.forward,
            runInputPressed = InputManager.Instance.WasRunInputPressed(),
            movingBackwards = InputManager.Instance.IsMovingBackwards(),
            movingOnXAxis = InputManager.Instance.IsMovingOnXAxis(),
            moveInputPressed = InputManager.Instance.IsMoveInputPressed(),
            shootPressed = InputManager.Instance.IsAttackPressed(),
            reloadPressed = InputManager.Instance.IsReloadPressed()
        };

        input.Set(data);
    }


    //Cuando se intenta mandar un imput pero se piede el paquere
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        //throw new NotImplementedException();
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        //throw new NotImplementedException();
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        //throw new NotImplementedException();
    }
    //Cuando un jugador entra a partida
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        //NetworkObject playerObj = runner.Spawn(playerPrefab, Vector3.zero, Quaternion.identity, player);
        //players.Add(player, playerObj);

        if (runner.IsServer)
        {
            Vector3 spawnPosition = players.Count == 0
                ? new Vector3(-4f, 1f, 0f)
                : new Vector3(4f, 1f, 0f);

            players.Add(player, runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, player));
        }

        CreateStormIfNeeded(runner);
        CreateAmmoPickupIfNeeded(runner);
        CreateRiflePickupIfNeeded(runner);

        onPlayerJoined?.Invoke();
    }

    private void CreateStormIfNeeded(NetworkRunner runner)
    {
        if (stormZone != null)
            return;

        GameObject stormObject = new GameObject("Storm Zone");
        stormObject.transform.position = new Vector3(4f, 0f, 11f);

        stormZone = stormObject.AddComponent<StormZone>();
        stormZone.Initialize(runner);
    }

    private void CreateAmmoPickupIfNeeded(NetworkRunner runner)
    {
        if (ammoPickup != null)
            return;

        GameObject pickupObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pickupObject.name = "Ammo Pickup";
        pickupObject.transform.position = new Vector3(0f, 0.5f, 4f);
        pickupObject.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);

        Renderer pickupRenderer = pickupObject.GetComponent<Renderer>();
        pickupRenderer.material.color = Color.green;

        BoxCollider pickupCollider = pickupObject.GetComponent<BoxCollider>();
        pickupCollider.isTrigger = true;

        ammoPickup = pickupObject.AddComponent<AmmoPickup>();
        ammoPickup.Initialize(runner);
    }

    private void CreateRiflePickupIfNeeded(NetworkRunner runner)
    {
        if (riflePickup != null)
            return;

        GameObject pickupObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pickupObject.name = "Rifle Pickup";
        pickupObject.transform.position = new Vector3(8f, 0.5f, 14f);
        pickupObject.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);

        Renderer pickupRenderer = pickupObject.GetComponent<Renderer>();
        pickupRenderer.material.color = Color.blue;

        BoxCollider pickupCollider = pickupObject.GetComponent<BoxCollider>();
        pickupCollider.isTrigger = true;

        riflePickup = pickupObject.AddComponent<RiflePickup>();
        riflePickup.Initialize(runner);
    }

    public void Metodo2()
    {
        Debug.Log("Metodo2");
    }
    //Cuando un jugador sle de a partida
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        //throw new NotImplementedException();
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        //throw new NotImplementedException();
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        //throw new NotImplementedException();
    }
    //Cuando cargauna escena
    public void OnSceneLoadDone(NetworkRunner runner)
    {
        //throw new NotImplementedException();
    }
    //Cuandop empieza a cargar una escena
    public void OnSceneLoadStart(NetworkRunner runner)
    {
        //throw new NotImplementedException();
    }
    //Cuando se crea una partida y temina/elimina una partida, tanto pub como privs
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        //throw new NotImplementedException();
    }
    //Cuando el host o server
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        //throw new NotImplementedException();
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        //throw new NotImplementedException();
    }

}
