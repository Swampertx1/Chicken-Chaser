
using GabesCommonUtility;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace GameCommonUtility.Multiplayer.QuickLoad
{
    public class SpawnDesiredCharacter : NetworkBehaviour
    {
        [SerializeField] private NetworkObject[] characterToSpawn;

        //[SerializeField] private Controller controllerPrefab;
        [SerializeField] private Button buttonPrefab;
        [SerializeField] private Transform parent;
        [SerializeField] private Controller controllerPrefab;

        private void Awake()
        {

            for (var index = 0; index < characterToSpawn.Length; index++)
            {
                var t = characterToSpawn[index];
                var x = Instantiate(buttonPrefab, parent);
                x.GetComponentInChildren<TextMeshProUGUI>().text = t.name;
                var index1 = index;
                x.onClick.AddListener(() => SpawnCharacter(index1));
            }
        }

        public override void OnNetworkSpawn()
        {
            if(!IsOwner)  gameObject.SetActive(false);
        }

        public void SpawnCharacter(int id)
        {
            RequestSpawnCharacter_ServerRpc(id);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void RequestSpawnCharacter_ServerRpc(int id, RpcParams @params = default)
        {
            var character = Instantiate(characterToSpawn[id]);
            character.SpawnAsPlayerObject(@params.Receive.SenderClientId);

            var clientRpcParams = RpcTarget.Single(@params.Receive.SenderClientId, RpcTargetUse.Temp);

            RequestSpawnClient_ClientRpc(character.NetworkObjectId, clientRpcParams);

            Debug.Log($"A player ({@params.Receive.SenderClientId}) is spawning in as {character.name}", character);
        }

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

#if UNITY_EDITOR
        private int _previous = 0;
        private void OnDrawGizmosSelected()
        {
            if (characterToSpawn.Length != _previous)
            {
                _previous = characterToSpawn.Length;
                foreach (NetworkObject obj in characterToSpawn)
                {
                    if (!obj.TryGetComponent(out IControllable _))
                        Debug.LogError($"Prefab is invalid {obj.name}, must contain IControllable component");
                }
            }
        }
#endif
    }
}


