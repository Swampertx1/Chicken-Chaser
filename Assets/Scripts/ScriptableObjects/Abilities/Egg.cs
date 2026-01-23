using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Egg : NetworkBehaviour
{
    [SerializeField] private float lifetimeTimer;
    private Rigidbody rb;
    private Collider coll;
    private TrailRenderer tr;
    [SerializeField] private GameObject visibility;
    
    public void Spawn(Vector3 velocity, ulong ownerId)
    { 
       NetworkObject.SpawnWithOwnership(ownerId, false);
       NetworkObject.DontDestroyWithOwner = true;
       rb.linearVelocity = velocity;
    }

    public void Awake()
    {
        rb = GetComponent<Rigidbody>();
        coll = GetComponentInChildren<Collider>();
        tr = GetComponentInChildren<TrailRenderer>();
        
    }

    private IEnumerator eggCollisionTimer()
    {
        coll.enabled = false;
        yield return new WaitForSeconds(0.1f);
        coll.enabled = true;
    }

    private IEnumerator eggTimer()
    {
        yield return new WaitForSeconds(lifetimeTimer);
        DespawnServerRpc();
    }
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Server)]
    private void DespawnServerRpc()
    {
        NetworkObject.Despawn(false);
        Eggpool.instance.returnToPool(this);
        
    }

    public override void OnNetworkDespawn()
    {
        visibility.SetActive(false);
        tr.emitting = false;
    }

    public override void OnNetworkSpawn()
    {
        tr.Clear();

        tr.emitting = true;
        visibility.SetActive(true);
        StartCoroutine(eggCollisionTimer());
        if (IsServer)
        {
            StartCoroutine(eggTimer());
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (IsServer)
        {
            DespawnServerRpc();
        }
    }
}
