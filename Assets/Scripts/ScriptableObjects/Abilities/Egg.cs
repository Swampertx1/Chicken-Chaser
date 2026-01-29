using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Egg : NetworkBehaviour
{
    [SerializeField] private float lifetimeTimer;
    private Rigidbody _rb;
    private Collider _coll;
    private TrailRenderer _tr;
    [SerializeField] private GameObject visibility;

    private Coroutine _lifeCycle;
    
    public void Spawn(Vector3 velocity, ulong ownerId)
    { 
       NetworkObject.SpawnWithOwnership(ownerId, false);
       NetworkObject.DontDestroyWithOwner = true;
       _rb.linearVelocity = velocity;
    }

    public void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _coll = GetComponentInChildren<Collider>();
        _tr = GetComponentInChildren<TrailRenderer>();
        
    }

    private IEnumerator EggCollisionTimer()
    {
        _coll.enabled = false;
        yield return new WaitForSeconds(0.1f);
        _coll.enabled = true;
    }

    private IEnumerator EggTimer()
    {
        yield return new WaitForSeconds(lifetimeTimer);
        _lifeCycle = null;
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
        _tr.emitting = false;
    }

    public override void OnNetworkSpawn()
    {
        _tr.Clear();

        _tr.emitting = true;
        visibility.SetActive(true);
        StartCoroutine(EggCollisionTimer());
        if (!IsServer) return;
        if(_lifeCycle != null) StopCoroutine(_lifeCycle);
        _lifeCycle =  StartCoroutine(EggTimer());
    }

    private void OnCollisionEnter(Collision other)
    {
        if (IsServer)
        {
            DespawnServerRpc();
        }
    }
}
