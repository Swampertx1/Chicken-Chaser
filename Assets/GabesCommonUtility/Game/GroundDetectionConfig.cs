using UnityEngine;

namespace GabesCommonUtility.Game
{
    /// <summary>
    /// Scriptable Object containing ground detection configuration
    /// </summary>
    [CreateAssetMenu(fileName = "GroundDetectionConfig", menuName = "GabesCommonUtility/GroundConfig")]
    public class GroundDetectionConfig : ScriptableObject
    {
        [Header("Cast Settings")]
        [Tooltip("Use sphere casting instead of raycasting")]
        public bool useSpherecast = false;
    
        [Tooltip("Radius for sphere cast (ignored if using raycast)")]
        public float sphereRadius = 0.2f;
    
        [Tooltip("Maximum distance to cast downward")]
        public float maxDistance = 0.1f;
    
        [Header("Layer Settings")]
        [Tooltip("Layers to detect as ground")]
        public LayerMask groundLayers = 1; // Default layer
    

    
        [Header("Gizmo Settings")]
        public Color gizmoColorGrounded = Color.green;
        public Color gizmoColorAirborne = Color.red;
        public bool showGizmos = true;
    }
}