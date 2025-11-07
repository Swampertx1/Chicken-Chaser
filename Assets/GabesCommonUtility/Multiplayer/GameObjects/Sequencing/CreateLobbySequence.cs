using System;
using Cysharp.Threading.Tasks;
using GabesCommonUtility.Sequence;
using Unity.Services.Lobbies;
using UnityEngine;

namespace GabesCommonUtility.Multiplayer.GameObjects.Sequencing
{
    public class CreateLobbySequence : MonoBehaviour, IEntrySequence
    {
        [SerializeField] private Behaviour success; 
        [SerializeField] private Behaviour failure;
        [SerializeField] private int maxLobbySize = 8;
        
        public event Action<string> DisplayMessage;
        public async UniTask<IEntrySequence> ExecuteSequence()
        {
            Debug.Log("Trying to create a fresh lobby");
            try
            {
                //8 Seats total, we restrict seats belonging to our children, excluding our selves. 4 players, but 3 children.
                int childCount = 0; //SplitscreenManager.Instance.PlayerCount - 1;
                int n = maxLobbySize - childCount;

                Debug.Log("This lobby will be locked to " + n + " players as there is a child count of: " + (childCount));

                await LobbySystem.Instance.CreateLobby(n);
                
                Debug.Log("We successfully created the lobby");

                return Default;
            }
            catch (LobbyServiceException e2)
            {
                Debug.LogError("Failed to create lobby: " + e2.Reason);
                DisplayMessage?.Invoke("Failed to create lobby: " + e2.Reason);
                return failure as  IEntrySequence;
            }
        }

        public IEntrySequence Default => success as  IEntrySequence;
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