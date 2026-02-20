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
        [SerializeField] private Controller controllerPrefab;

        
        private bool _spawnCompleted;

      

       

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        public void Spawn_ServerRpc(ulong clientId)
        {
           
            Transform spawnPoint = GetAvailableSpawnPoint();

            if (spawnPoint == null)
            {
                NotifySpawnFailed_ClientRpc(clientId);
                return;
            }

            // Spawn the player
            NetworkObject player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
            player.SpawnAsPlayerObject(clientId, true);

            var clientRpcParams = RpcTarget.Single(clientId, RpcTargetUse.Temp);

            RequestSpawnClient_ClientRpc(player.NetworkObjectId, clientRpcParams);

            
            NotifySpawnSuccess_ClientRpc(clientId, player.NetworkObjectId);

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

        [Rpc(SendTo.SpecifiedInParams, InvokePermission = RpcInvokePermission.Server)]
        private void RequestSpawnClient_ClientRpc(ulong objectID, RpcParams @params)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectID, out NetworkObject networkObject))
            {
                Instantiate(controllerPrefab).Possess(networkObject.gameObject);
                
                //Keep ourselves alive if we've 
                gameObject.SetActive(false);
            }
            else
            {
                Debug.LogError($"Could not find spawned object with ID {objectID}");
            }
        }
        

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