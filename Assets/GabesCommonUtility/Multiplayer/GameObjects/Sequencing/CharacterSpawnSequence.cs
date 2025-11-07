#if UNITY_NETCODE_GAMEOBJECTS && UNITASK
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GabesCommonUtility.Sequence;
using Unity.Netcode;
using UnityEngine;

namespace GabesCommonUtility.Multiplayer.GameObjects.Sequencing
{
    public class CharacterSpawnSequence : NetworkBehaviour, IEntrySequence
    {
        [SerializeField] private Behaviour next;
        
        //[SerializeField] private PlayerOrderSOAP _playerOrderSOAP;
        [SerializeField] private Transform[] spawnPoints;
        private int _numPlayers = 0;

        public IEntrySequence Default => next as IEntrySequence;
        public bool IsCompleted => false;
        public event Action<string> DisplayMessage;

        public UniTask<IEntrySequence> ExecuteSequence()
        {
            PlayerPrefs.Save();
            /*
            uint[] Quares = new uint[SplitscreenManager.Instance.PlayerCount];
            for (int i = 0; i < SplitscreenManager.Instance.PlayerCount; i++)
            {
                Quares[i] = uint.Parse(PlayerPrefs.GetString("SelectedPlayer" + i));
            }
            SpawnPlayer_ServerRPC(Quares);
            */
            return UniTask.FromResult(Default);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SpawnPlayer_ServerRPC(uint[] prefabID, ServerRpcParams serverRpcParams = default) // We want to know WHO asked us to do this
        {
            IReadOnlyList<NetworkPrefab> prefabList =
                NetworkManager.NetworkConfig.Prefabs.NetworkPrefabsLists[0].PrefabList;
            foreach (uint prefab in prefabID)
            {
                foreach (NetworkPrefab networkPrefab in prefabList)
                {
                    if (networkPrefab.SourcePrefabGlobalObjectIdHash == prefab)
                    {
                        var selectedQuare = Instantiate(networkPrefab.Prefab, spawnPoints[_numPlayers].position,
                            spawnPoints[_numPlayers]
                                .rotation); // TODO: there is no safety here, players rejoining/leaving will shatter everything
                        _numPlayers += 1;
                        selectedQuare.GetComponent<NetworkObject>()
                            .SpawnWithOwnership(serverRpcParams.Receive.SenderClientId);
                        print("I spawned a Quare!");
                        break;
                    }
                }
            }

            Debug.LogError("Failed to join... No selected prefab?", gameObject);
        }

        private void OnDrawGizmos()
        {
            if (next && Default == null)
            {
                Debug.LogError("Success is INVALID", gameObject);
            }
        }
    }
}
#endif