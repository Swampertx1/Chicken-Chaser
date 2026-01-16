using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GabesCommonUtility.Multiplayer.GameObjects
{
    public class PlayerSpawnSequence : NetworkBehaviour
    {
       
        [SerializeField] private NetworkObject playerPrefab;
        [SerializeField] private Transform[] randomSpawnPoint;
        [SerializeField] private LayerMask occupiedCheckLayer;
        [SerializeField] private float occupiedCheckRadius = 1f;
        
        
        private bool _spawnCompleted;

      

       

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        public void Spawn_ServerRpc(ulong clientId)
        {
           
            Debug.Log("Spawn_ServerRpc A");
            Transform spawnPoint = GetAvailableSpawnPoint();
            Debug.Log("Spawn_ServerRpc B");

            if (spawnPoint == null)
            {
                Debug.LogWarning($"All spawn points are occupied. Cannot spawn player for client {clientId}.");
                NotifySpawnFailed_ClientRpc(clientId);
                return;
            }
            Debug.Log("Spawn_ServerRpc C");

            // Spawn the player
            NetworkObject player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
            player.SpawnAsPlayerObject(clientId, true);
            Debug.Log("Spawn_ServerRpc D");

            NotifySpawnSuccess_ClientRpc(clientId, player.NetworkObjectId);
            Debug.Log("Spawn_ServerRpc E");

        }

        [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Server)]
        private void NotifySpawnSuccess_ClientRpc(ulong clientId, ulong objectId)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                _spawnCompleted = true;
             
            }
        }

        [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Server)]
        private void NotifySpawnFailed_ClientRpc(ulong clientId)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                _spawnCompleted = false;
            }
        }

        private Transform GetAvailableSpawnPoint()
        {
            if (randomSpawnPoint == null || randomSpawnPoint.Length == 0)
            {
                return transform;
            }

            // Find all unoccupied spawn points
            System.Collections.Generic.List<Transform> availableSpawns = new System.Collections.Generic.List<Transform>();

            foreach (Transform spawn in randomSpawnPoint)
            {
                if (spawn && !IsSpawnOccupied(spawn))
                {
                    availableSpawns.Add(spawn);
                }
            }

            // If we have available spawns, choose one randomly
            if (availableSpawns.Count > 0)
            {
                return availableSpawns[Random.Range(0, availableSpawns.Count)];
            }

            // All spawns occupied - return a random one anyway
            return randomSpawnPoint[Random.Range(0, randomSpawnPoint.Length)];
        }

        private bool IsSpawnOccupied(Transform spawnPoint)
        {
            return Physics.CheckSphere(spawnPoint.position, occupiedCheckRadius, occupiedCheckLayer);
        }

        
        public bool IsCompleted => _spawnCompleted;

        
        

        private void OnDrawGizmosSelected()
        {
            if (randomSpawnPoint == null) return;

            Gizmos.color = Color.yellow;
            foreach (Transform spawn in randomSpawnPoint)
            {
                if (spawn != null)
                {
                    Gizmos.DrawWireSphere(spawn.position, occupiedCheckRadius);
                }
            }
        }
    }
}