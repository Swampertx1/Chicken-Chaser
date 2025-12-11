using System;
using Cysharp.Threading.Tasks;
using GabesCommonUtility.Sequence;
using Unity.Netcode;
using UnityEngine;

namespace GabesCommonUtility
{
    public class SpawnNetworkObjectSequence : MonoBehaviour, IEntrySequence
    {
        [SerializeField] private GameObject networkPrefab;
        [SerializeField] private Behaviour success;
        [SerializeField] private Behaviour failure;
        [SerializeField] private bool destroyAfterSpawn = false;
        
        private GameObject _spawnedObject;
        
        public event Action<string> DisplayMessage;
        
        public async UniTask<IEntrySequence> ExecuteSequence()
        {
            Debug.Log("Attempting to spawn network object");
            
            try
            {
                if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
                {
                    Debug.LogError("NetworkManager is not active");
                    DisplayMessage?.Invoke("Failed to spawn: NetworkManager not active");
                    return failure as IEntrySequence;
                }
                
                if (!NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsHost)
                {
                    Debug.LogError("Cannot spawn network object: Not server or host");
                    DisplayMessage?.Invoke("Failed to spawn: Not server or host");
                    return failure as IEntrySequence;
                }
                
                if (networkPrefab == null)
                {
                    Debug.LogError("Network prefab is not assigned");
                    DisplayMessage?.Invoke("Failed to spawn: No prefab assigned");
                    return failure as IEntrySequence;
                }
                
                _spawnedObject = Instantiate(networkPrefab, transform.position, transform.rotation);
                
                var networkObject = _spawnedObject.GetComponent<NetworkObject>();
                if (networkObject == null)
                {
                    Debug.LogError("Prefab does not have a NetworkObject component");
                    DisplayMessage?.Invoke("Failed to spawn: Invalid network prefab");
                    Destroy(_spawnedObject);
                    return failure as IEntrySequence;
                }
                
                networkObject.Spawn();
                
                Debug.Log("Successfully spawned network object");
                DisplayMessage?.Invoke("Network object spawned successfully");
                
                if (destroyAfterSpawn)
                {
                    Destroy(gameObject);
                }
                
                return Default;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to spawn network object: {e.Message}");
                DisplayMessage?.Invoke($"Failed to spawn: {e.Message}");
                
                if (_spawnedObject != null)
                {
                    Destroy(_spawnedObject);
                }
                
                return failure as IEntrySequence;
            }
        }
        
        public IEntrySequence Default => success as IEntrySequence;
        
        public bool IsCompleted => _spawnedObject != null;
        
        private void OnDrawGizmos()
        {
            if (success && success is not IEntrySequence)
            {
                Debug.LogError("Success is INVALID", gameObject);
            }
            if (failure && failure is not IEntrySequence)
            {
                Debug.LogError("Failure is INVALID", gameObject);
            }
        }
    }
}