using Fusion;
using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    [SerializeField] private float pickupDistance = 1.5f;

    private NetworkRunner runner;
    private float nextCheckTime;

    public void Initialize(NetworkRunner gameRunner)
    {
        runner = gameRunner;
    }

    private void Update()
    {
        transform.Rotate(0f, 60f * Time.deltaTime, 0f);

        if (runner == null || !runner.IsRunning || !runner.IsServer)
            return;

        if (Time.time < nextCheckTime)
            return;

        nextCheckTime = Time.time + 0.2f;

        Health[] players = UnityEngine.Object.FindObjectsByType<Health>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None);

        foreach (Health playerHealth in players)
        {
            Vector3 distanceToPlayer = playerHealth.transform.position - transform.position;
            distanceToPlayer.y = 0f;

            if (distanceToPlayer.magnitude > pickupDistance)
                continue;

            Handgun playerWeapon = playerHealth.GetComponent<Handgun>();

            if (playerWeapon != null && playerWeapon.TryCollectAmmo())
                return;
        }
    }
}
