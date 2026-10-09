using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class EventSystem : MonoBehaviour
{

    public static event Action eventoDePrueba;

    public static event Action eventoSaludo;
    public static event Action eventoDinamico;
    public static event Action eventoGeneral;

    private Action Metodo;
    public PhotonManager photonManager;

    private void Awake()
    {
        photonManager = FindObjectOfType<PhotonManager>();
    }

    //Esto se manda a llamar cuando se prende el script
    private void OnEnable()
    {
        eventoDePrueba += Metodo1; //Al hecho de agregar un metodo a un evento se le conoce como suscripcion
        eventoDePrueba += photonManager.Metodo2;


        //tarea

        eventoSaludo += Saludar;
        eventoDinamico += SuscriptorA;
        eventoDinamico += SuscriptorB;
       
    }

    // Este se manda a llamar cuando se apaga el script
    private void OnDisable()
    {
        eventoDePrueba -= Metodo1;

        //tarea
        eventoSaludo -= Saludar;
        eventoDinamico -= SuscriptorA;
        eventoDinamico -= SuscriptorB;
            }

    private void Start()
    {
        Metodo = Metodo1; //Lo que esta pasando no es que mi accion triggerea el Metodo1, sino que copia su comportamiento
        Metodo1();
    }

    private void Update()
    {
        //if (InputManager.Instance.IsShootPressed())
        //{
        //    eventoDePrueba?.Invoke();
        //}

        //tarea
        if (Keyboard.current.digit1Key.wasPressedThisFrame) // con el 1
        {
            eventoSaludo?.Invoke();
        }

        if (Keyboard.current.digit2Key.wasPressedThisFrame) // con el 2
        {
            eventoDinamico?.Invoke();

            eventoDinamico -= SuscriptorB;
        }

        if (Keyboard.current.digit3Key.wasPressedThisFrame) // con el 3
        {
            eventoGeneral?.Invoke();

        }
    }
    private void Metodo1()
    {
        Debug.Log("HolaMundo!");
        //eventoDePrueba -= photon.Metodo2;
    }

    private void Saludar()
    {
        Debug.Log("El evento fue activado");
    }

    private void SuscriptorA()
    {
        Debug.Log("Suscriptor A");

    }

    private void SuscriptorB()
    {
        Debug.Log("Suscriptor B");
    }

  }
