using System;
using Cysharp.Threading.Tasks;
using GabesCommonUtility.Sequence;
using TMPro;
using Unity.Services.Lobbies;
using UnityEngine;

namespace GabesCommonUtility.Multiplayer.GameObjects.Sequencing
{
    public class JoinLobbySequence : MonoBehaviour, IEntrySequence
    {
        [SerializeField] private Behaviour success;
        [SerializeField] private Behaviour failure;
        [SerializeField] private TMP_InputField lobbyIdTMP;

        public event Action<string> DisplayMessage;

        public async UniTask<IEntrySequence> ExecuteSequence()
        {
            Debug.Log("Trying to Join a lobby");
            try
            {
                await LobbySystem.Instance.JoinLobby(lobbyIdTMP.text);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError("Failed to join lobby: " + e.Reason);
                return failure as IEntrySequence;
            }
            return Default;
        }

        public IEntrySequence Default => success as IEntrySequence;
        public bool IsCompleted => LobbySystem.Instance.CurrentLobby != null;

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
    }
}