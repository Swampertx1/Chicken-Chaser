using UnityEngine;

namespace AI
{
    /// <summary>
    /// To optimize this script, create a method to auto generate a scriptable object and cache the values.
    /// </summary>
    public class WayPoint : MonoBehaviour
    {
        [SerializeField] private float suggestedDelay;
        
        // (Make a private variable accessible, but not editable in other files)
        public float SuggestedDelay => suggestedDelay;
        public Vector3 Forward => transform.forward;
        public Vector3 Position => transform.position;
        
        private static readonly Color Orange = new Color(1F, 0.5F, 0);

        private void OnDrawGizmos()
        {
            Gizmos.color = Orange;
            if(suggestedDelay > 0)
                Gizmos.DrawRay(transform.position, transform.forward * Mathf.Max(1,suggestedDelay));
        }
    }
}
