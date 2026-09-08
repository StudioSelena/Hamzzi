using UnityEngine;
using UnityEngine.AI;

public class HamsterMoving : MonoBehaviour
{
    [SerializeField] private NavMeshAgent NavMeshAgent;

    private float _radius = 10f;
    private float _threshold = 1.0f;
    private bool _isInitialized = false;
    private bool _isMovingEnabled = true;
    private bool _isStopPending = false;

    public bool IsStopPending
    {
        get
        {
            return _isStopPending;
        }
    }

    void Update()
    {
        if (NavMeshAgent == null || !NavMeshAgent.enabled)
        {
            return;
        }

        if (!NavMeshAgent.isOnNavMesh)
        {
            CheckNavMesh();
            return;
        }

        if (_isStopPending)
        {
            if (HasArrived())
            {
                StopMovingNow();
            }

            return;
        }

        if (!_isMovingEnabled)
        {
            return;
        }

        if (!_isInitialized || (!NavMeshAgent.pathPending && (NavMeshAgent.remainingDistance <= _threshold || !NavMeshAgent.hasPath)))
        {
            _isInitialized = true;
            SetRandomDestination();
        }
    }

    public void SetMovingEnabled(bool isEnabled)
    {
        if (isEnabled)
        {
            _isStopPending = false;
            _isMovingEnabled = true;

            if (NavMeshAgent != null && NavMeshAgent.enabled && NavMeshAgent.isOnNavMesh)
            {
                NavMeshAgent.isStopped = false;
            }

            return;
        }

        if (!_isMovingEnabled)
        {
            return;
        }

        _isStopPending = true;
    }

    private bool HasArrived()
    {
        if (NavMeshAgent.pathPending)
        {
            return false;
        }

        if (!NavMeshAgent.hasPath)
        {
            return true;
        }

        return NavMeshAgent.remainingDistance <= _threshold;
    }

    private void StopMovingNow()
    {
        _isStopPending = false;
        _isMovingEnabled = false;

        NavMeshAgent.ResetPath();
        NavMeshAgent.isStopped = true;
    }

    private void CheckNavMesh()
    {
        if (NavMeshAgent == null || !NavMeshAgent.enabled)
        {
            return;
        }

        if (!NavMeshAgent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 50.0f, NavMesh.AllAreas))
            {
                NavMeshAgent.Warp(hit.position);
                _isInitialized = true;
                SetRandomDestination();
            }
        }
    }

    private void SetRandomDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * _radius;
        randomDirection += transform.position;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, _radius, NavMesh.AllAreas))
        {
            NavMeshAgent.SetDestination(hit.position);
        }
    }
}