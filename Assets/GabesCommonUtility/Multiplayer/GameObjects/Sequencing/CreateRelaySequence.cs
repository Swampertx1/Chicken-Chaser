using System;
using Cysharp.Threading.Tasks;
using GabesCommonUtility.Sequence;
using UnityEngine;

namespace GabesCommonUtility.Multiplayer.GameObjects.Sequencing
{
    public class CreateRelaySequence : MonoBehaviour, IEntrySequence
    {
        [SerializeField] private Behaviour success;
        [SerializeField] private Behaviour failure;
        [SerializeField] private bool joinIfNotHost;
        public event Action<string> DisplayMessage;

        public async UniTask<IEntrySequence> ExecuteSequence()
        {
            Debug.Log("Trying to CreateRelaySequence");
            try
            {
                if (!LobbySystem.Instance.IsHost())
                {
                    if (!joinIfNotHost 
                        || LobbySystem.Instance.CurrentLobby == null 
                        || !LobbySystem.Instance.CurrentLobby.Data.TryGetValue("RelayCode", out var code)) 
                        return failure as IEntrySequence;
                    await RelayHandler.Instance.JoinRelay(code.Value);
                    return Default;
                }
                string value = await RelayHandler.Instance.CreateRelay(LobbySystem.Instance.CurrentLobby.MaxPlayers);
                await LobbySystem.Instance.UpdateKey("RelayCode", value);
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