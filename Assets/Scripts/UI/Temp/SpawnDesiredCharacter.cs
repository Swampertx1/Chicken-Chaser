using System;
using Unity.Netcode;
using UnityEngine;

public class SpawnDesiredCharacter : NetworkBehaviour
{
    [SerializeField] private NetworkObject[] characterToSpawn;
    [SerializeField] private Controller controllerPrefab;
    
    public void SpawnCharacter(int id)
    {
        RequestSpawnCharacter_ServerRpc(id);
        
        //Keep ourselves alive if we've 
        if(IsServer) gameObject.SetActive(false);
        else  Destroy(gameObject);
    }
    
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void RequestSpawnCharacter_ServerRpc(int id, RpcParams @params = default)
    {
        var character = Instantiate(characterToSpawn[id]);
        character.SpawnAsPlayerObject(@params.Receive.SenderClientId);
      //  RequestSpawnClient_ClientRpc();
        
        Debug.Log($"A player ({@params.Receive.SenderClientId}) is spawning in as {character.name}", character);
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Server)]
    private void RequestSpawnClient_ClientRpc(ulong objectID, RpcParams @params)
    {
        //controllerPrefab.Possess();
    }

#if UNITY_EDITOR
    private int previous = 0;
    private void OnDrawGizmosSelected()
    {
        if (characterToSpawn.Length != previous)
        {
            previous = characterToSpawn.Length;
            foreach (NetworkObject obj in characterToSpawn)
            {
                if(!obj.TryGetComponent(out IControllable _))
                    Debug.LogError($"Prefab is invalid {obj.name}, must contain IControllable component");
            }
        }
    }
    #endif
}
