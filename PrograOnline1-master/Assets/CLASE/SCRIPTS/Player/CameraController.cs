using Fusion;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;


public class CameraController : NetworkBehaviour
{
    [Header("Camera Settings")] [SerializeField]
    private Transform player;

    [SerializeField] private float mouseSensitivity = 1;

    [FormerlySerializedAs("smooth")] [SerializeField]
    private float smoothnes;

    [SerializeField] private float maxAngleY = 80;
    [SerializeField] private float minAngleY = -80;

    private Vector2 camVelociy;
    private Vector2 smoothVelocity;

    [Header("Blob Movement")] 
    [SerializeField] private float walkingSpeed = 1f;
    
    [SerializeField, Range(0,0.1f)] private float walkingAmplitude = 0.015f; // Que tanto se mueve hacia los lados al caminar
    [SerializeField, Range(0,0.1f)] private float runningAmplitude = 0.015f; // Que tanto se mueve hacia los lados al correr
    [SerializeField, Range(0,15)] private float walkingFrequency = 10.0f; // La frecuencia con la que se mueve al caminar
    [SerializeField, Range(10,20)] private float runningFrequency = 18f; // La frecuencia con la que se mueve al correr
    [SerializeField] private float resetPosSpeed = 3.0f; // Cuando dejas de moverte que regrese al centro
    [SerializeField] private float toggleSpeed = 3.0f; // 
    
    private Vector3 startPos; // Posicion inicial de la cabeza , el centro

    [SerializeField] private bool moveHead;
    
    private Vector2 head;
    
    private InputManager inputManager;
    
    private void Awake()
    {
        startPos = transform.localPosition;
    }
    
private void Start()
    {
        inputManager = InputManager.Instance;
        if (player == null)
        {
            player = FindObjectOfType<MovementController>().transform;
        }

        // La cámara usa el delta relativo del mouse. Si el cursor queda libre,
        // deja de producir delta al llegar al borde de la ventana.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    
    private void Update()
    {
        //RotateCamera();
        //if(!moveHead) return;
        //BlobMove();
        //ResetPosition();
    }
public override void FixedUpdateNetwork()
    {
        if (!HasInputAuthority || !moveHead)
            return;

        BlobMove();
        ResetPosition();
    }

    //Habia entendido mal e intenete hacer lo mismo del movimiento pero en el script que habia de camara.... a la mera hora era mas facil moverlo todo desde MovementController :')

    //private void RotateCamera(NetworkInfoData input)
    //{
    //    //Vector2 rawFrameVelocity = Vector2.Scale(inputManager.GetMouseDelta(), Vector2.one * mouseSensitivity);
    //      Vector2 rawFrameVelocity =
    //   Vector2.Scale(input.mouseMovement,
    //              Vector2.one * mouseSensitivity);
    //    smoothVelocity = Vector2.Lerp(smoothVelocity, rawFrameVelocity, 1 / smoothnes); // Te mueve desde donde tengas el mouse, a la nueva posicion del mouse
    //    camVelociy += smoothVelocity;
    //    camVelociy.y = Mathf.Clamp(camVelociy.y, minAngleY, maxAngleY); // Limita la rotacion de la camara en Y. En base el movimiento del mouse.

    //    transform.localRotation = Quaternion.AngleAxis(-camVelociy.y, Vector3.right); // Rota la camara hacia arriba y abajo. La rotacion esta en X. 
    //    player.localRotation = Quaternion.AngleAxis(camVelociy.x, Vector3.up);

    //    Debug.Log("Rotación Player: " + player.localEulerAngles);
    //}
    
   private void BlobMove()
    {
        if (!inputManager.IsMoveInputPressed()) // Si no presiono ningun input
        {
            return; // termina el metodo
        }
        
        if(inputManager.IsMoveInputPressed()) // Pregunto si me estoy moviendo
        {
            if(inputManager.IsMovingBackwards() || inputManager.IsMovingOnXAxis()) // Me estoy moviendo hacia atras o hacia los lados?
            {
                transform.localPosition += FootStepMotion();
            }
            else //  Entonces me muevo hacia adelante
            {
                if(inputManager.WasRunInputPressed()) // Estoy corriendo?
                {
                    transform.localPosition += RunningFootStepMotion();
                }
                else // Estoy caminando
                {
                    transform.localPosition += FootStepMotion();
                }
            }
        }

        if(inputManager.IsMoveInputPressed())
        {
            transform.localPosition += inputManager.IsMovingBackwards() || inputManager.IsMovingOnXAxis() ? FootStepMotion() : inputManager.WasRunInputPressed() ? RunningFootStepMotion() : FootStepMotion();
        }
        
        
        
    } 

    private void ResetPosition()
    {
        if(transform.localPosition == startPos) return; // Si la camara ya esta en la pos inicial, no hace nada
        transform.localPosition = Vector3.Lerp(transform.localPosition, startPos, resetPosSpeed * Time.deltaTime);
}

    private Vector3 FootStepMotion()
    {
        Vector3 pos = Vector3.zero;
        pos.y = Mathf.Sin(Time.time * walkingFrequency) * walkingAmplitude * walkingSpeed;
        pos.x = Mathf.Cos(Time.time * walkingFrequency / 2) * walkingAmplitude * 2 * walkingSpeed;
        return pos;
    }
    
    
    private Vector3 RunningFootStepMotion()
    {
        Vector3 pos = Vector3.zero;
        pos.y = Mathf.Sin(Time.time * runningFrequency) * runningAmplitude * walkingSpeed;
        pos.x = Mathf.Cos(Time.time * runningFrequency / 2) * runningAmplitude * 2 * walkingSpeed;
        return pos;
    }
    
}