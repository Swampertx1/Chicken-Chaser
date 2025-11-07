using System;
using UnityEngine;

public class Grounding : MonoBehaviour
{
    [Header("Grounding")]
    [SerializeField]private Transform foot;
    [SerializeField] private float groundedRadius;
    [SerializeField] private float maxGroundedRadius;
    [SerializeField] private LayerMask groundMask;
    private bool isGrounded;
    public event Action<bool> OnGroundStateChange;
    public bool IsGrounded => isGrounded;

    // Update is called once per frame
    void FixedUpdate()
    {
        CheckGrounded();
    }
    
    private void CheckGrounded()
    {
        // Get the scale multiplier from the parent transform
        float scaleMultiplier = transform.localScale.y;
        
        // Scale the detection parameters
        float scaledRadius = groundedRadius * scaleMultiplier;
        float scaledMaxDistance = maxGroundedRadius * scaleMultiplier;
        
        bool CurrentGroundState = Physics.SphereCast(foot.position, scaledRadius, Vector3.down, out RaycastHit hit, scaledMaxDistance, groundMask);
        if (isGrounded != CurrentGroundState)
        {
            isGrounded = CurrentGroundState;
            OnGroundStateChange?.Invoke(isGrounded);
        }
    }

    private void OnDrawGizmos()
    {
        if (foot == null) return;

        // Get the scale multiplier
        float scaleMultiplier = transform.localScale.y;
        float scaledRadius = groundedRadius * scaleMultiplier;
        float scaledMaxDistance = maxGroundedRadius * scaleMultiplier;

        // Set color based on grounded state
        Gizmos.color = isGrounded ? Color.green : Color.red;

        // Draw the starting sphere (at foot position)
        Gizmos.DrawWireSphere(foot.position, scaledRadius);

        // Draw the end sphere (at maximum cast distance)
        Vector3 endPosition = foot.position + Vector3.down * scaledMaxDistance;
        Gizmos.DrawWireSphere(endPosition, scaledRadius);

        // Draw lines connecting the two spheres to show the cast volume
        Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
        foreach (Vector3 dir in directions)
        {
            Vector3 offset = dir * scaledRadius;
            Gizmos.DrawLine(foot.position + offset, endPosition + offset);
        }

        // Draw a line showing the cast direction
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(foot.position, endPosition);

        // If grounded, draw a small sphere at the hit point
        if (Application.isPlaying && isGrounded)
        {
            Physics.SphereCast(foot.position, scaledRadius, Vector3.down, out RaycastHit hit, scaledMaxDistance, groundMask);
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(hit.point, 0.1f * scaleMultiplier);
        }
    }
}