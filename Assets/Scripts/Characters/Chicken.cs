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
    [SerializeField] private CinemachineCamera playerCamera;
    
    [Header("Grenade")]
    [SerializeField] private NetworkObject grenadePrefab;
    [SerializeField] private float grenadeThrowForce = 15f;
    [SerializeField] private float grenadeCooldown = 2f;
    private NetworkVariable<bool> canThrowGrenade = new(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
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
        delay = new WaitForSeconds(chickenStats.JumpCooldown);
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

    public void ThrowGrenadeInput()
    {
     ThrowGrenade_ServerRpc();  
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]

    private void ThrowGrenade_ServerRpc(RpcParams param = default)
    {
        if (canThrowGrenade.Value)
        {
            ThrowGrenade(param.Receive.SenderClientId);
        } 
    }

    private void ThrowGrenade(ulong id)
    {
        Debug.Log("ThrowGrenade() called!");
        
        if (grenadePrefab == null)
        {
            Debug.LogWarning("Grenade prefab is not assigned!");
            return;
        }

        Debug.Log("Grenade prefab is assigned, spawning grenade...");

        // Determine spawn position (use grenadeSpawnPoint if assigned, otherwise use camera)
        Vector3 spawnPos =  cam.position;
        Vector3 throwDirection = cam.forward;

        Debug.Log("Spawn Position: " + spawnPos + " | Direction: " + throwDirection);

        // Instantiate grenade
        var grenade = Instantiate(grenadePrefab, spawnPos, Quaternion.identity);
        grenade.SpawnWithOwnership(id, this);
        Rigidbody grenaderb = grenade.GetComponent<Rigidbody>();
        Debug.Log("Grenade instantiated: " + grenaderb.name);
        
        // Apply force to grenade
        grenaderb.AddForce(throwDirection * grenadeThrowForce, ForceMode.Impulse);

        // Start cooldown
        StartCoroutine(GrenadeCooldown());
    }

    private IEnumerator GrenadeCooldown()
    {
        canThrowGrenade.Value = false;
        yield return new WaitForSeconds(grenadeCooldown);
        canThrowGrenade.Value = true;
    }

    private void HandleObjectDetection()
    {
        if (Physics.Raycast(cam.position, cam.forward, out RaycastHit hit, chickenStats.MaxRaycastDistance, StaticUtilities.EverythingButChicken))
        {
            if ((hit.rigidbody && hit.rigidbody.TryGetComponent(out InteractibleObject interactible)) ||
                hit.transform.TryGetComponent(out interactible))
            { 
                if (interactibleObj == interactible) return;
                
                interactibleObj = interactible; 
                onInteractibleObjectChanged?.Invoke();
                return;
            }
        }
        if (!interactibleObj) return;
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
        if (!CanJump()) return;
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
        return grounding.IsGrounded && jumpCoroutine == null;
    }
    
    private void OnDrawGizmos()
    {
        if (cam == null) return;

        Vector3 origin = cam.position;
        Vector3 direction = cam.forward;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, chickenStats.MaxRaycastDistance, StaticUtilities.EverythingButChicken))
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(origin, hit.point);
            Gizmos.DrawWireSphere(hit.point, 0.1f);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(hit.point, origin + direction * chickenStats.MaxRaycastDistance);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(origin, origin + direction * chickenStats.MaxRaycastDistance);
        }
    }
}