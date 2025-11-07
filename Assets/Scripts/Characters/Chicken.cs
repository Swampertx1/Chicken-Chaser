using System;
using System.Collections;
using System.Collections.Generic;
using ScriptableObjects;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using Utilities;

[RequireComponent(typeof(Grounding))]
public class Chicken : NetworkBehaviour, IControllable
{
    private Vector3 currentMoveDirection;
    private Rigidbody rb;
    private Grounding grounding;

    
    
    [Header("Rotation")] 
    [SerializeField] private Transform head;

    [SerializeField] private ChickenStats chickenStats;
    [SerializeField] private Transform body;
    [SerializeField] private Transform cam;
    [SerializeField] private float mouseSensitivity = 1;
    [SerializeField, Range(0,89.9f)] private float maxPitch = 80;
    private InteractibleObject interactibleObj;
  [SerializeField]  private CinemachineCamera playerCamera;
    public InteractibleObject InteractibleObj => interactibleObj;
    public event Action onInteractibleObjectChanged;
    
    
    
    
    private Coroutine jumpCoroutine;
    #if !UNITY_EDITOR
    private WaitForSeconds delay;  
    #endif
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        grounding = GetComponent<Grounding>();
       
#if !UNITY_EDITOR
     delay = new WaitForSeconds(jumpCooldown);
#endif
       
    }

    public override void OnNetworkSpawn()
    {
        
       
        enabled = IsOwner;
        if (IsOwner)
        {
            Controller.BindController(this);
        }

        if (!IsLocalPlayer)
        {
            playerCamera.enabled = false;
        }
     
    }

    private void Update()
    {
        HandleObjectDetection();
    }

    private void HandleObjectDetection()
    {
        if (Physics.Raycast(cam.position, cam.forward, out RaycastHit hit, chickenStats.MaxRaycastDistance, StaticUtilities.EverythingButChicken))
        {
            // The ray hit an object, so now we destroy it
          //  Debug.Log("Raycast hit and 'E' key pressed. Destroying object: " + hit.transform.name);
            if ((hit.rigidbody && hit.rigidbody.TryGetComponent(out InteractibleObject interactible)) ||
                hit.transform.TryGetComponent(out interactible))
            { 
                if (interactibleObj == interactible) return;
                
                interactibleObj = interactible; 
                onInteractibleObjectChanged?.Invoke();
                return;
            }
        }
        if (!interactibleObj ) return;
        interactibleObj = null;
        onInteractibleObjectChanged?.Invoke();
    }

    private void FixedUpdate()
    {
        MovePlayer();
       
        
    }
    

    private void MovePlayer()
    {
        rb.AddForce(transform.rotation * currentMoveDirection * chickenStats.MoveSpeed, ForceMode.VelocityChange);
        Vector3 velocity = rb.linearVelocity;
        velocity.y = 0;
        float currentSpeed = velocity.magnitude;
        if (currentSpeed > chickenStats.MaxSpeed)
        {
            Vector3 direction = velocity / currentSpeed;
            direction.y = rb.linearVelocity.y;
            rb.linearVelocity = new Vector3(direction.x * chickenStats.MaxSpeed, direction.y, direction.z * chickenStats.MaxSpeed);
        }
    }
    public void Move(Vector2 direction)
    {
       currentMoveDirection = new Vector3(direction.x, 0, direction.y);
       
    }

    public void Look(Vector2 direction)
    {
     direction *= mouseSensitivity;
     body.Rotate(Vector3.up, direction.x);
     float pitch = head.localEulerAngles.x + direction.y;
     
     if (pitch > maxPitch && pitch < 180)
         pitch = maxPitch;
     else if (pitch < 360 - maxPitch && pitch > 180)
         pitch = 360-maxPitch;
         
     Debug.Log(pitch);
     head.localEulerAngles = new Vector3(pitch, 0, 0);
    }

    public void Jump()
    {
        if (!CanJump())  return;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * chickenStats.JumpForce, ForceMode.Impulse);
        jumpCoroutine = StartCoroutine(JumpCooldown());
    }

    public void Collect()
    {
       interactibleObj?.Interact(gameObject);
    }

    private IEnumerator JumpCooldown()
    {
#if !UNITY_EDITOR
    yield return delay;
    #else
        yield return new WaitForSeconds(chickenStats.JumpCooldown);
#endif
       jumpCoroutine = null;
    }

    public bool CanJump()
    {
        return grounding.IsGrounded && jumpCoroutine ==  null;
    }
    private void OnDrawGizmos()
    {
        if (cam == null) return;

        // Set the starting point and direction
        Vector3 origin = cam.position;
        Vector3 direction = cam.forward;

        // Check if the raycast hits something
        if (Physics.Raycast(origin, direction, out RaycastHit hit, chickenStats.MaxRaycastDistance, StaticUtilities.EverythingButChicken))
        {
            // Ray hit something - draw in green to the hit point
            Gizmos.color = Color.green;
            Gizmos.DrawLine(origin, hit.point);
        
            // Draw a sphere at the hit point
            Gizmos.DrawWireSphere(hit.point, 0.1f);
        
            // Optionally draw the remaining distance in yellow
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(hit.point, origin + direction * chickenStats.MaxRaycastDistance);
        }
        else
        {
            // Ray didn't hit anything - draw in red for the full distance
            Gizmos.color = Color.red;
            Gizmos.DrawLine(origin, origin + direction * chickenStats.MaxRaycastDistance);
        }
    }

 
    
}
