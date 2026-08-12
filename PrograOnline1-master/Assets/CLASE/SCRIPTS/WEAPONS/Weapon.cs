using Fusion;
using UnityEngine;


/// <summary>
/// Esta clase contendra todo lo basico de un arma
/// Variables
/// Comportamiento
/// 
/// Pero todo de manera generica. El comportamiento de cada arma
/// Se dara en la herencia
/// 
/// Una clase abstracta nos permite crear metodos que de principio no contienen logica para
/// despues escribir su funcionamiento en la herencia.
/// Y que la implementacion de esos metodos sea obligatoria.
/// El abstracto lo usas cuando quieres un metodo que si o si obligatoriamente existe en toda la herencia
/// 
/// Cuando hago un metodo virtual.
/// Yo a ese metodo le puedo escribir una logica.
/// Y posteriormente en herencia, sobreescribir esa logica, pero pudiendo mantener la logica base.
/// El virtual lo usas cuando puede o non pasar en herencia.
/// 
/// </summary>
public abstract class Weapon : NetworkBehaviour
{
    public ShootType shootType;

    public int damage; //danio
    public int actualAmmo; //Municion del mi cargador Actual
    public int magazineSize; //Capacidad del Cargador
    public int maxAmmoCapacity; //Capacidad maxima del almacenamiento

    public GameObject projectile;
    public abstract void RaycastShoot(); //Disparo con raycast
    public abstract void PhysicShoot();  //Disparo con proyectil fisico
    public abstract void Reload(); //Recarga

}
