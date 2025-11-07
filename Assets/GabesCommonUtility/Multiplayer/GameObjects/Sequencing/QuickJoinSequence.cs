using System;
using Cysharp.Threading.Tasks;
using GabesCommonUtility.Sequence;
using Unity.Services.Lobbies;
using UnityEngine;

namespace GabesCommonUtility.Multiplayer.GameObjects.Sequencing
{
    public class QuickJoinSequence : MonoBehaviour, IEntrySequence
    {
        [SerializeField] private Behaviour success;
        [SerializeField] private Behaviour noOpenLobbyResponse;
        [SerializeField] private Behaviour failure;
        
        [SerializeField] private int maxLobbySize = 8;

        public event Action<string> DisplayMessage;

        public async UniTask<IEntrySequence> ExecuteSequence()
        {
            Debug.Log("Initializing Quick play.");
            try
            {
                await LobbySystem.Instance.QuickJoinLobby();
                Debug.Log("We have completed joined a lobby!");
                return Default;
            }
            catch (LobbyServiceException e)
            {
                if (e.Reason == LobbyExceptionReason.NoOpenLobbies)
                {
                    return noOpenLobbyResponse as IEntrySequence;
                }
            }
            return failure as IEntrySequence;
        }

        public IEntrySequence Default => success as IEntrySequence;
        public bool IsCompleted => LobbySystem.Instance.CurrentLobby != null;

        private void OnDrawGizmos()
        {
            if (success && success is not IEntrySequence)
            {
                Debug.LogError("success is INVALID", gameObject);
            }
            
            if (noOpenLobbyResponse && noOpenLobbyResponse is not IEntrySequence)
            {
                Debug.LogError("noOpenLobbyResponse is INVALID", gameObject);
            }


            if (failure && failure is not IEntrySequence)
            {
                Debug.LogError("failure is INVALID", gameObject);
            }
        }
    }
}