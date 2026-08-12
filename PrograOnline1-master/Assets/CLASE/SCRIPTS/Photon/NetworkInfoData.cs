using Fusion;
using UnityEngine;


/// <summary>
/// Este struct va a contener toda la informacion que queremos mandar 
/// Debe heredar de INetworkInput para que el servidor la pueda leer
/// 
/// Photon no sabe si lo que se manda aqui es un input o no
/// </summary>

public struct NetworkInfoData : INetworkInput
{
    public Vector2 movementAxis;

    //parcial
    public Vector2 mouseMovement;
    public Vector3 aimDirection;
    public bool runInputPressed;
    public bool movingBackwards;
    public bool movingOnXAxis;
    public bool moveInputPressed;

    public bool shootPressed;
    public bool reloadPressed;
}
