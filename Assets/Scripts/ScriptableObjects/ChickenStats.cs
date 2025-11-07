using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "ChickenStats", menuName = "Scriptable Objects/ChickenStats")]
    public class ChickenStats : ScriptableObject
    {
    
        [SerializeField]private float maxSpeed; 
        [SerializeField]private float moveSpeed;
        [SerializeField]private float jumpCooldown;
        [SerializeField]private float jumpForce;
        [SerializeField] private float maxRaycastDistance;
        public float MaxSpeed => maxSpeed;
        public float MoveSpeed => moveSpeed;
        public float JumpCooldown => jumpCooldown;
        public float JumpForce => jumpForce;
        public float MaxRaycastDistance => maxRaycastDistance;
    
    }
}
