using System.Linq;
using UnityEngine;


namespace GabesCommonUtility.Game
{
    public class RagdollController : MonoBehaviour
    {
        [Header("Ragdoll Settings")]
        [Tooltip("Default state when the game starts")]
        public bool startRagdolled;
        
        [Header("Rigidbodies")]
        [SerializeField] private Rigidbody coreRigidbody;
        [SerializeField] private Rigidbody[] ragdollRigidbodies;
        
        [Header("Behaviors")]
        [Tooltip("These behaviors will be enabled when ragdoll is active")]
        [SerializeField] private MonoBehaviour[] enableOnRagdoll;
        
        [Tooltip("These behaviors will be disabled when ragdoll is active")]
        [SerializeField] private MonoBehaviour[] disableOnRagdoll;
        
        private bool _isRagdolled;
        
        public bool IsRagdolled => _isRagdolled;
        
        void Start()
        {
            // Set initial state
            SetRagdoll(startRagdolled);
        }
        
        /// <summary>
        /// Gathers all child rigidbodies and adds them to the array if not already present
        /// </summary>
        public void GatherRigidbodies()
        {
            coreRigidbody = GetComponent<Rigidbody>();
            ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
            if (coreRigidbody != null) ragdollRigidbodies = ragdollRigidbodies.Skip(1).ToArray();
            Debug.Log($"Gathered {ragdollRigidbodies.Length} rigidbodies for ragdoll");
        }

        /// <summary>
        /// Enable or disable ragdoll physics
        /// </summary>
        public void SetRagdoll(bool enable)
        {
            _isRagdolled = enable;

            if (coreRigidbody)
            {
                coreRigidbody.isKinematic = enable;
                coreRigidbody.detectCollisions = !enable;
            }
            // Set rigidbody states
            foreach (Rigidbody rb in ragdollRigidbodies)
            {
                rb.isKinematic = !enable;
                rb.detectCollisions = enable;
            }
            
            
            // Enable specified behaviors
            foreach (MonoBehaviour behaviour in enableOnRagdoll)
            {
                behaviour.enabled = enable;
            }
            
            // Disable specified behaviors
            foreach (MonoBehaviour behaviour in disableOnRagdoll)
            {
                behaviour.enabled = !enable;
            }
        }
        
        /// <summary>
        /// Toggle ragdoll state
        /// </summary>
        public void ToggleRagdoll()
        {
            SetRagdoll(!_isRagdolled);
        }
        
        /// <summary>
        /// Apply force to all ragdoll rigidbodies
        /// </summary>
        public void ApplyForce(Vector3 force, ForceMode mode = ForceMode.Impulse)
        {
            if (!_isRagdolled)
            {
                coreRigidbody .AddForce(force,mode);
            }
            else
            {
                foreach (Rigidbody rb in ragdollRigidbodies)
                {
                    if (rb) rb.AddForce(force, mode);
                }
            }
        }
        
        /// <summary>
        /// Apply explosion force to all ragdoll rigidbodies
        /// </summary>
        public void ApplyExplosionForce(float force, Vector3 position, float radius)
        {
            if (!_isRagdolled)
                return;
                
            foreach (Rigidbody rb in ragdollRigidbodies)
            {
                if (rb != null)
                    rb.AddExplosionForce(force, position, radius);
            }
        }
    }
}