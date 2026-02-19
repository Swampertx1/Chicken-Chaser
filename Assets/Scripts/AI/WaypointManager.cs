using System;
using AI;
using UnityEngine;

/// <summary>
/// This is a dirty way to do this, better if we use soap, but we don't have resources.
/// </summary>
[DefaultExecutionOrder(-1000)]
public class WaypointManager : MonoBehaviour
{
    public WaypointGroups[] groups;
    
    [Serializable]
    public class WaypointGroups
    {
        #if UNITY_EDITOR
        public Color color = Color.yellow;
        #endif
        
        public WaypointSoap generatedSoap;
        public WayPoint[] waypoints;
    }

    private void Awake()
    {
        foreach (var group in groups)
        {
            group.generatedSoap.waypoints = group.waypoints;
        }
    }
    
    private void OnDrawGizmos()
    {
        foreach (var group in groups)
        {
            Gizmos.color = group.color;

            for (int i = 0; i < group.waypoints.Length; ++i)
            {
                Gizmos.DrawSphere(group.waypoints[i].transform.position, 0.25f);
                if (i == 0)
                {
                    Gizmos.DrawLine(group.waypoints[0].transform.position,
                        group.waypoints[^1].transform.position);
                    continue;
                }

                Gizmos.DrawLine(group.waypoints[i - 1].transform.position,
                    group.waypoints[i].transform.position);
            }
        }
    }
}
