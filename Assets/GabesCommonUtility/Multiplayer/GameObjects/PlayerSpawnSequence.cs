using System;
using Cysharp.Threading.Tasks;
using GabesCommonUtility.Sequence;
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
        
        
        private bool spawnCompleted;

      

       

        [ServerRpc(RequireOwnership = false)]
        public void SpawnServerRpc(ulong clientId)
        {
           

            Transform spawnPoint = GetAvailableSpawnPoint();

            if (spawnPoint == null)
            {
                Debug.LogWarning($"All spawn points are occupied. Cannot spawn player for client {clientId}.");
                NotifySpawnFailedClientRpc(clientId);
                return;
            }

            // Spawn the player
            NetworkObject player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
            player.SpawnAsPlayerObject(clientId, true);
            
            NotifySpawnSuccessClientRpc(clientId);
        }

        [ClientRpc]
        private void NotifySpawnSuccessClientRpc(ulong clientId)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                spawnCompleted = true;
            }
        }

        [ClientRpc]
        private void NotifySpawnFailedClientRpc(ulong clientId)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                spawnCompleted = false;
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

        
        public bool IsCompleted => spawnCompleted;

        
        

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