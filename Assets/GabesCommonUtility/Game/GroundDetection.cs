using UnityEngine;
using System;

namespace GabesCommonUtility.Game
{
    /// <summary>
    /// Handles ground detection using raycast or spherecast from a single point
    /// </summary>
    public class GroundDetection : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private GroundDetectionConfig config;
        
        [Header("Cast Origin")]
        [Tooltip("Transform to cast from (downward direction)")]
        [SerializeField] private Transform castOrigin;
        
        // Public event fired when ground state changes
        public event Action<bool> OnGroundStateChanged;
        
        // Current grounded state
        private bool _isGrounded;
        public bool IsGrounded => _isGrounded;
        
        // Store successful hit information
        private RaycastHit _hitInfo;
        public RaycastHit HitInfo => _hitInfo;

        private void Awake()
        {
            if (config != null && castOrigin != null) return;
            UpdateGroundState(false);
            enabled = false;
        }

        private void FixedUpdate()
        {
            CheckGroundState();
        }
        
        /// <summary>
        /// Performs ground detection and updates grounded state
        /// </summary>
        private void CheckGroundState()
        {
            // Perform cast from origin
            bool hitDetected = PerformCast(castOrigin, out RaycastHit hit);
            
            if (hitDetected)
            {
                _hitInfo = hit;
            }
            
            UpdateGroundState(hitDetected);
        }
        
        /// <summary>
        /// Performs a single cast (ray or sphere) from the given origin
        /// </summary>
        private bool PerformCast(Transform origin, out RaycastHit hit)
        {
            Vector3 position = origin.position;
            Vector3 direction = Vector3.down;
            
            if (config.useSpherecast)
            {
                return Physics.SphereCast(
                    position,
                    config.sphereRadius,
                    direction,
                    out hit,
                    config.maxDistance,
                    config.groundLayers,
                    QueryTriggerInteraction.Ignore
                );
            }
            return Physics.Raycast(
                position,
                direction,
                out hit,
                config.maxDistance,
                config.groundLayers,
                QueryTriggerInteraction.Ignore
            );
        }
        
        /// <summary>
        /// Updates grounded state and fires event if changed
        /// </summary>
        private void UpdateGroundState(bool newState)
        {
            if (_isGrounded != newState)
            {
                _isGrounded = newState;
                OnGroundStateChanged?.Invoke(_isGrounded);
            }
        }
        
        /// <summary>
        /// Draws visual debug gizmos for ground detection
        /// </summary>
        private void OnDrawGizmos()
        {
            if (config == null || !config.showGizmos || castOrigin == null) return;
        
            bool hitDetected = PerformCast(castOrigin, out RaycastHit hit);
            
            Color gizmoColor = hitDetected ? config.gizmoColorGrounded : config.gizmoColorAirborne;
            Gizmos.color = gizmoColor;
        
            Vector3 start = castOrigin.position;
            Vector3 end = start + Vector3.down * config.maxDistance;
        
            if (config.useSpherecast)
            {
                // Draw sphere at start and end positions
                Gizmos.DrawWireSphere(start, config.sphereRadius);
                Gizmos.DrawWireSphere(end, config.sphereRadius);
                Gizmos.DrawLine(start, end);
            }
            else
            {
                // Draw simple ray
                Gizmos.DrawLine(start, end);
                Gizmos.DrawWireSphere(end, 0.02f);
            }
        
            // Draw hit point if grounded
            if (hitDetected)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(_hitInfo.point, 0.05f);
            }
        }
    }
}