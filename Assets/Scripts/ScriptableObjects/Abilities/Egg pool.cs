using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Eggpool : NetworkBehaviour
{
    private Queue<Egg> inactiveEggs = new();
    HashSet<Egg> activeEggs = new();
  public static Eggpool instance;
  [SerializeField]private Egg eggPrefab;
    public void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        instance = this;
    }

[Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SpawnEggServerRpc(Vector3 position, Quaternion rotation, Vector3 velocity, RpcParams rpcParams = default)
    {
        Egg e = getOrCreateEgg();
        e.transform.SetPositionAndRotation(position, rotation);
        //e.NetworkObject.Spawn();
        e.Spawn(velocity, rpcParams.Receive.SenderClientId);
        activeEggs.Add(e);
       
    }

    private Egg getOrCreateEgg()
    {
        if (inactiveEggs.TryDequeue(out Egg e ))
        {
            return e;
        }
        return Instantiate(eggPrefab, transform);
    }

    public void returnToPool(Egg egg)
    {
        activeEggs.Remove(egg);
        inactiveEggs.Enqueue(egg);
    }
}
