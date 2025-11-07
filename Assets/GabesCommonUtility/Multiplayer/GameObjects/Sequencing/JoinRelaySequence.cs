using System;
using Cysharp.Threading.Tasks;
using GabesCommonUtility.Sequence;
using UnityEngine;

namespace GabesCommonUtility.Multiplayer.GameObjects.Sequencing
{
    public class JoinRelaySequence : MonoBehaviour, IEntrySequence
    {
        [SerializeField] private Behaviour success;
        [SerializeField] private Behaviour failure;

        public event Action<string> DisplayMessage;

        public async UniTask<IEntrySequence> ExecuteSequence()
        {
            Debug.Log("Trying to Join a lobby");
            try
            {
                if (LobbySystem.Instance.CurrentLobby == null) return failure as  IEntrySequence;
                if (!LobbySystem.Instance.CurrentLobby.Data.TryGetValue("RelayCode", out var code)) return failure as  IEntrySequence;
                await RelayHandler.Instance.JoinRelay(code.Value);
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to join lobby: " + e);
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