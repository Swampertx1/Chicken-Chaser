using AI;
using UnityEngine;

[CreateAssetMenu(fileName = "WaypointSOAP", menuName = "ChickenChaser/WaypointSOAP")]
public class WaypointSoap : ScriptableObject
{
    public WayPoint[] waypoints { get; set; }
}
