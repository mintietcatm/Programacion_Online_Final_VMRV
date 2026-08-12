using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Fusion;
using System.Xml.Serialization;
using Fusion.Addons.SimpleKCC;

[RequireComponent(typeof(Rigidbody), typeof(GroundCheck))]

public class MovementController : NetworkBehaviour
{
    private InputManager inputManager;
    private Rigidbody rbPlayer;

    [SerializeField] private Animator _animator;
    [SerializeField] private SimpleKCC kcc;
    private Health playerHealth;

    public override void Spawned() // Se manda a llamar si el objeto es spawneado
    {
        // de network behavior
        base.Spawned();

        rbPlayer = GetComponent<Rigidbody>();
        kcc = GetComponent<SimpleKCC>();
        playerHealth = GetComponent<Health>();
        kcc.SetGravity(-25f);
        Debug.Log(kcc);  //yo a todo le pongo debug.log cuando no jala xd
        cameraTransform = GetComponentInChildren<Camera>().transform;

        // get la camara basicamente
        Camera playerCamera = cameraTransform.GetComponent<Camera>();

        if (playerCamera != null)
        {
            playerCamera.gameObject.SetActive(HasInputAuthority);

            // para que solo oigamos al jugador no al otro
            AudioListener listener = playerCamera.GetComponent<AudioListener>();

            if (listener != null)
                listener.enabled = HasInputAuthority;
        }
    }

    private void Start()
    {
        inputManager = InputManager.Instance;
    }

    // Override es que estas sobreescribiendo algo
    public override void FixedUpdateNetwork()
    {
        //if (!HasInputAuthority)
        //    return; // Si no tenemos autoridad de Input que termine la ejecuci�n

        if (playerHealth != null && playerHealth.IsDead)
        {
            kcc.Move(Vector3.zero);
            _animator.SetBool("IsWalking", false);
            _animator.SetBool("IsRunning", false);
            return;
        }

        if (GetInput(out NetworkInfoData input))
        {
            RotatePlayer(input);
            Movement(input);

            _animator.SetBool("IsWalking", input.moveInputPressed);
            _animator.SetBool("IsRunning", input.runInputPressed);
            _animator.SetFloat("WalkingZ", input.movementAxis.y);
            _animator.SetFloat("WalkingX", input.movementAxis.x);
        }
    }


    #region Movimiento

    [SerializeField] private float walkSpeed = 5.5f;
    [SerializeField] private float runSpeed = 7.7f;
    [SerializeField] private float crouchSpeed = 3.9f;

    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float maxAngleY = 80f;
    [SerializeField] private float minAngleY = -80f;

    [SerializeField] private Transform cameraTransform;
    private float verticalRotation;
    private float horizontalRotation;

    private void Movement(NetworkInfoData input)
    {
        //kcc.Move(transform.localRotation *
        //                    new Vector3(input.movementAxis.x, 0, input.movementAxis.y) *
        //                    (Time.deltaTime * Speed(input)));

        //kcc.Move(kcc.TransformRotation *
        //         new Vector3(input.movementAxis.x, 0f, input.movementAxis.y) *
        //         Speed(input));

        // Proyectamos la direccion de la camara sobre el suelo para que W siempre
        // mueva al jugador hacia donde esta mirando, sin afectar la velocidad.
        //Vector3 cameraForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
        //Vector3 cameraRight = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
        //Vector3 movementDirection = cameraRight * input.movementAxis.x +
        //                            cameraForward * input.movementAxis.y;

        //kcc.Move(movementDirection * Speed(input));

        // Usamos la rotacion simulada del KCC para que host y cliente calculen
        // exactamente la misma direccion durante prediccion y resimulacion.
        kcc.Move(kcc.TransformRotation *
                 new Vector3(input.movementAxis.x, 0f, input.movementAxis.y) *
                 Speed(input));

        //rbPlayer.linearVelocity = transform.localRotation *
        //                    new Vector3(inputManager.GetMoveInput().x, 0, inputManager.GetMoveInput().y) *
        //                    (Time.deltaTime * Speed());
    }

    //Parcial

private void RotatePlayer(NetworkInfoData input)
    {
        //horizontalRotation += input.mouseMovement.x * mouseSensitivity;
        //verticalRotation -= input.mouseMovement.y * mouseSensitivity;
        //verticalRotation = Mathf.Clamp(verticalRotation, minAngleY, maxAngleY);

        // Partimos del estado del KCC, que Fusion puede restaurar cuando
        // resimula ticks. Las variables normales de arriba acumulaban el mouse
        // nuevamente y provocaban giros distintos en el cliente compilado.
        Vector2 currentLookRotation = kcc.GetLookRotation();
        verticalRotation = Mathf.Clamp(
            currentLookRotation.x - input.mouseMovement.y * mouseSensitivity,
            minAngleY,
            maxAngleY);
        horizontalRotation = currentLookRotation.y +
                             input.mouseMovement.x * mouseSensitivity;

        // SimpleKCC conserva yaw en el jugador y pitch en su estado de look.
        // El pitch de la cámara se sincroniza en LateUpdate, después de Render.
        kcc.SetLookRotation(verticalRotation, horizontalRotation, minAngleY, maxAngleY);
    }

private void LateUpdate()
    {
        if (!HasInputAuthority || cameraTransform == null || kcc == null)
            return;

        Vector2 pitchRotation = kcc.GetLookRotation(true, false);
        cameraTransform.localRotation = Quaternion.Euler(pitchRotation.x, 0f, 0f);
    }


    //Fin del parcial

    private float Speed(NetworkInfoData input)
    {
        return input.movingBackwards || input.movingOnXAxis ? walkSpeed : input.runInputPressed ? runSpeed : walkSpeed;
    }


    #endregion
}
