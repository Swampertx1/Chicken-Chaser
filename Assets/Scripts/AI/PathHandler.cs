using UnityEngine;
using UnityEngine.AI;

namespace AI
{
    //These need to execute early
    [DefaultExecutionOrder(-100)]
    public class PathHandler : MonoBehaviour
    {
        [Header("AI")] 
        [SerializeField] private WaypointSoap patrolPoints;
        [SerializeField, Min(0)] private int currentPatrolPoint;
        
        private NavMeshAgent _agent;
        private WayPoint currentWaypoint =>  patrolPoints.waypoints[currentPatrolPoint % patrolPoints.waypoints.Length];
        
        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            currentPatrolPoint -= 1;
        }

        public bool HasReachedDestination(out float suggestedDelay)
        {
            suggestedDelay = currentWaypoint.SuggestedDelay;
            return _agent.remainingDistance <= _agent.stoppingDistance;
        }
        
        public void SetNextPatrolPoint()
        {
            ++currentPatrolPoint;
            _agent.SetDestination(currentWaypoint.Position);
        }

        public Vector3 GetSuggestedForward()
        {
            return currentWaypoint.Forward;
        }
    }
    
}
