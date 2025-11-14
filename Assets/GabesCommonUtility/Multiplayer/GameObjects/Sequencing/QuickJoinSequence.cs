using System;
using Cysharp.Threading.Tasks;
using GabesCommonUtility.Sequence;
using Unity.Services.Lobbies;
using UnityEngine;

namespace GabesCommonUtility.Multiplayer.GameObjects.Sequencing
{
    public class QuickJoinSequence : MonoBehaviour, IEntrySequence
    {
        [SerializeField] private Behaviour successAsClient;
        [SerializeField] private bool createNewLobbyOnFail;
        [SerializeField] private Behaviour successAsHost;
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
                    try
                    {
                        //8 Seats total, we restrict seats belonging to our children, excluding our selves. 4 players, but 3 children.
                        int childCount = 0; //SplitscreenManager.Instance.PlayerCount - 1;
                        int n = maxLobbySize - childCount;

                        Debug.Log("This lobby will be locked to " + n + " players as there is a child count of: " + (childCount));

                        await LobbySystem.Instance.CreateLobby(n);
                
                        Debug.Log("We successfully created the lobby");

                        return successAsHost as  IEntrySequence;
                    }
                    catch (LobbyServiceException e2)
                    {
                        Debug.LogError("Failed to create lobby: " + e2.Reason);
                        DisplayMessage?.Invoke("Failed to create lobby: " + e2.Reason);
                        return failure as  IEntrySequence;
                    }
                }
            }
            return failure as IEntrySequence;
        }

        public IEntrySequence Default => successAsClient as IEntrySequence;
        public bool IsCompleted => LobbySystem.Instance.CurrentLobby != null;

        private void OnDrawGizmos()
        {
            if (successAsClient && successAsClient is not IEntrySequence)
            {
                Debug.LogError("success is INVALID", gameObject);
            }
            
            if (successAsHost && successAsHost is not IEntrySequence)
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