using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class BlobAI : MonoBehaviour
{
    public enum BlobState { Idle, Wander }
    public BlobState currentState;

    private NavMeshAgent agent;
    private float idleTimer;
    public float idleTime = 20f;
    public float idleRange = 5f;
    private float currentIdleTarget;

    public float wanderRadius = 6f;

    // Crowd params
    public float neighborRadius = 2.5f;
    public float separationRadius = 1.2f;
    public float flowStrength = 0.6f;
    public float smoothness = 6f;
    public int maxDensity = 4;
    public bool toggleCrowdSteering = false;

    public Vector3 flowDirection = Vector3.zero; // optional global bias

    private Vector3 smoothedVelocity;
    private Vector3 lastPos;
    private float stuckTimer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Non-blocking crowd config
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
        agent.avoidancePriority = Random.Range(20, 60);
        agent.autoRepath = true;

        currentState = BlobState.Idle;
        RollIdleTime();
    }

    void Update()
    {
        if (toggleCrowdSteering)
        {
            CrowdSteering();   // 🔥 crowd brain
        }

        switch (currentState)
        {
            case BlobState.Idle:
                HandleIdle();
                break;

            case BlobState.Wander:
                HandleWander();
                break;
        }

        DeadlockRecovery();
    }

    // ================= FSM =================

    void HandleIdle()
    {
        idleTimer += Time.deltaTime;

        if (idleTimer >= currentIdleTarget)
        {
            idleTimer = 0f;
            SetWanderDestination();
            currentState = BlobState.Wander;
        }
    }

    void HandleWander()
    {
        if (agent.hasPath && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            RollIdleTime();
            currentState = BlobState.Idle;
        }
    }

    // ================= WANDER =================

    void SetWanderDestination()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 randomDir = Random.insideUnitSphere * wanderRadius;
            randomDir += transform.position;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDir, out hit, wanderRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                return;
            }
        }

        currentState = BlobState.Idle;
    }

    void RollIdleTime()
    {
        currentIdleTarget = Random.Range(idleTime - idleRange, idleTime + idleRange);
        if (currentIdleTarget < 0.1f) currentIdleTarget = 0.1f;
    }

    // ================= CROWD AI =================

    void CrowdSteering()
    {
        List<BlobAI> neighbors = GetNeighbors();

        if (neighbors.Count == 0) return;

        Vector3 separation = Vector3.zero;
        Vector3 alignment = Vector3.zero;
        Vector3 cohesion = Vector3.zero;

        foreach (var n in neighbors)
        {
            Vector3 dir = transform.position - n.transform.position;
            float d = dir.magnitude;

            // soft separation
            if (d < separationRadius)
                separation += dir.normalized * (1f - d / separationRadius);

            alignment += n.agent.velocity;
            cohesion += n.transform.position;
        }

        cohesion = (cohesion / neighbors.Count - transform.position).normalized;
        alignment = alignment.normalized;

        // density bias
        Vector3 densityForce = -cohesion * (neighbors.Count / (float)maxDensity);

        // flow field
        Vector3 flowForce = flowDirection.normalized * flowStrength;

        // final force blend
        Vector3 force =
            separation * 2.5f +
            alignment  * 1.2f +
            cohesion   * 0.6f +
            densityForce * 1.4f +
            flowForce;

        // smoothing
        smoothedVelocity = Vector3.Lerp(smoothedVelocity, force, Time.deltaTime * smoothness);

        agent.Move(smoothedVelocity * Time.deltaTime);
    }

    List<BlobAI> GetNeighbors()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, neighborRadius);
        List<BlobAI> list = new List<BlobAI>();

        foreach (var h in hits)
        {
            BlobAI b = h.GetComponent<BlobAI>();
            if (b != null && b != this)
                list.Add(b);
        }
        return list;
    }

    // ================= DEADLOCK RECOVERY =================

    void DeadlockRecovery()
    {
        if (Vector3.Distance(transform.position, lastPos) < 0.01f)
            stuckTimer += Time.deltaTime;
        else
            stuckTimer = 0f;

        lastPos = transform.position;

        if (stuckTimer > 1.5f)
        {
            SetWanderDestination();
            stuckTimer = 0f;
        }
    }
}
