using System;
using Cysharp.Threading.Tasks;
using GabesCommonUtility.Sequence;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GabesCommonUtility.Multiplayer.GameObjects
{
    public class PlayerSpawnSequence : NetworkBehaviour, IEntrySequence
    {
        [SerializeField] private Behaviour success;
        [SerializeField] private Behaviour failure;
        [SerializeField] private NetworkObject playerPrefab;
        [SerializeField] private Transform[] randomSpawnPoint;
        [SerializeField] private LayerMask occupiedCheckLayer;
        [SerializeField] private float occupiedCheckRadius = 1f;

        public event Action<string> DisplayMessage;
        
        private bool spawnCompleted;

        public async UniTask<IEntrySequence> ExecuteSequence()
        {
            Debug.Log("Attempting to spawn player");
            try
            {
                spawnCompleted = false;
                SpawnServerRpc();

                // Wait for spawn to complete (you may need to adjust this timeout)
                await UniTask.WaitUntil(() => spawnCompleted, cancellationToken: this.GetCancellationTokenOnDestroy())
                    .Timeout(TimeSpan.FromSeconds(5));

                Debug.Log("Player spawned successfully");
                return Default;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to spawn player: {e.Message}");
                DisplayMessage?.Invoke($"Failed to spawn player: {e.Message}");
                return failure as IEntrySequence;
            }
        }

        [ContextMenu("Spawn")]
        public void Spawn()
        {
            SpawnServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        private void SpawnServerRpc(ServerRpcParams rpcParams = default)
        {
            ulong clientId = rpcParams.Receive.SenderClientId;

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

        public IEntrySequence Default => success as IEntrySequence;
        public bool IsCompleted => spawnCompleted;

        private void OnDrawGizmos()
        {
            if (success && success is not IEntrySequence)
            {
                Debug.LogError("Success is INVALID", gameObject);
            }
            if (failure && failure is not IEntrySequence)
            {
                Debug.LogError("failure is INVALID", gameObject);
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