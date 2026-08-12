using Fusion;
using UnityEngine;

public class StormZone : MonoBehaviour
{
    [SerializeField] private float startDelay = 15f;
    //[SerializeField] private float initialRadius = 2f;
    //[SerializeField] private float maximumRadius = 20f;
    [SerializeField] private float initialRadius = 20f;
    [SerializeField] private float maximumRadius = 2f;
    //[SerializeField] private float expansionTime = 90f;
    [SerializeField] private float expansionTime = 60f;
    [SerializeField] private int damagePerTick = 5;
    [SerializeField] private float damageInterval = 1f;

    private NetworkRunner runner;
    private SphereCollider stormTrigger;
    private LineRenderer stormLine;
    private float startTime;
    private float nextDamageTime;
    private float currentRadius;

    public void Initialize(NetworkRunner gameRunner)
    {
        runner = gameRunner;
        startTime = Time.time;
        nextDamageTime = Time.time + startDelay;
        currentRadius = initialRadius;

        CreateTrigger();
        CreateStormLine();
        UpdateStormSize();
    }

    private void Update()
    {
        if (runner == null || !runner.IsRunning)
            return;

        float elapsedTime = Time.time - startTime;

        if (elapsedTime > startDelay)
        {
            float expansionProgress = (elapsedTime - startDelay) / expansionTime;
            expansionProgress = Mathf.Clamp01(expansionProgress);
            currentRadius = Mathf.Lerp(initialRadius, maximumRadius, expansionProgress);
        }

        UpdateStormSize();

        if (runner.IsServer && Time.time >= nextDamageTime)
        {
            nextDamageTime = Time.time + damageInterval;
            DamagePlayersInside();
        }
    }

    private void CreateTrigger()
    {
        stormTrigger = gameObject.AddComponent<SphereCollider>();
        stormTrigger.isTrigger = true;
        stormTrigger.radius = initialRadius;
    }

    private void CreateStormLine()
    {
        stormLine = gameObject.AddComponent<LineRenderer>();
        stormLine.loop = true;
        stormLine.useWorldSpace = true;
        stormLine.positionCount = 64;
        stormLine.startWidth = 0.25f;
        stormLine.endWidth = 0.25f;
        stormLine.startColor = new Color(1f, 0.1f, 0.1f, 1f);
        stormLine.endColor = new Color(1f, 0.1f, 0.1f, 1f);
        stormLine.material = new Material(Shader.Find("Sprites/Default"));
    }

    private void UpdateStormSize()
    {
        if (stormTrigger != null)
            stormTrigger.radius = currentRadius;

        if (stormLine == null)
            return;

        for (int pointIndex = 0; pointIndex < stormLine.positionCount; pointIndex++)
        {
            float angle = pointIndex * Mathf.PI * 2f / stormLine.positionCount;
            Vector3 circlePoint = new Vector3(
                Mathf.Cos(angle) * currentRadius,
                0.15f,
                Mathf.Sin(angle) * currentRadius);

            stormLine.SetPosition(pointIndex, transform.position + circlePoint);
        }
    }

    private void DamagePlayersInside()
    {
        Health[] players = UnityEngine.Object.FindObjectsByType<Health>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None);

        foreach (Health playerHealth in players)
        {
            Vector3 distanceToPlayer = playerHealth.transform.position - transform.position;
            distanceToPlayer.y = 0f;

            //if (distanceToPlayer.magnitude <= currentRadius)
            if (distanceToPlayer.magnitude > currentRadius)
                playerHealth.TakeDamage(damagePerTick, PlayerRef.None);
        }
    }
}
