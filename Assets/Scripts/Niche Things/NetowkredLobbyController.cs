using GabesCommonUtility.Multiplayer.GameObjects;
using Unity.Services.Lobbies;
using UnityEngine;

namespace Niche_Things
{
    public class NetworkedLobbyController : MonoBehaviour
    {
        [SerializeField] private ChickenLightUp[] chickensInGame;
        [SerializeField] private GameObject[] hostOnlyButtons;
        
        private void OnEnable()
        {
            // Initial update
            UpdateButtons();
            
            // Subscribe to lobby events
            LobbySystem.Instance.Events.LobbyChanged += GeneralLobbyChange;
            LobbySystem.Instance.Events.LobbyEventConnectionStateChanged += LobbyEventConnectionChanged;
            LobbySystem.Instance.PlayerJoinedReal += OnPlayerJoined;
            LobbySystem.Instance.PlayerLeftReal += OnPlayerLeft;
        }

        private void OnDisable()
        {
            if (LobbySystem.Instance != null && LobbySystem.Instance.Events != null)
            {
                LobbySystem.Instance.Events.LobbyChanged -= GeneralLobbyChange;
                LobbySystem.Instance.Events.LobbyEventConnectionStateChanged -= LobbyEventConnectionChanged;
                LobbySystem.Instance.PlayerJoinedReal -= OnPlayerJoined;
                LobbySystem.Instance.PlayerLeftReal -= OnPlayerLeft;
            }
        }

        private void LobbyEventConnectionChanged(LobbyEventConnectionState state)
        {
            //Debug.Log($"Lobby connection state changed: {state}");
            UpdateButtons();
        }

        private void OnPlayerJoined()
        {
            //Debug.Log($"Players joined at indices: {string.Join(", ", playerIndices)}");
            UpdateButtons();
        }

        private void OnPlayerLeft()
        {
            //Debug.Log($"Players left at indices: {string.Join(", ", playerIndices)}");
            UpdateButtons();
            
            // Check if host left and handle migration
            CheckForHostMigration();
        }

        private void CheckForHostMigration()
        {
            if (LobbySystem.Instance?.CurrentLobby == null) return;
            
            // Check if we became the host
            if (LobbySystem.Instance.IsHost())
            {
                // Check if we weren't the host before (you may need to track previous state)
                Debug.Log("Handle host migration???");
            }
        }

        private void GeneralLobbyChange(ILobbyChanges changes)
        {
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            if (LobbySystem.Instance?.CurrentLobby == null)
            {
                // Hide all chickens and host buttons if no lobby
                foreach (var chicken in chickensInGame)
                {
                    if (chicken != null) chicken.enabled = false;
                }
                foreach (var button in hostOnlyButtons)
                {
                    if (button != null) button.SetActive(false);
                }
                return;
            }

            Debug.Log("Lobby has changed... Updating Buttons");
            
            // Update chicken displays based on player count
            int playerCount = LobbySystem.Instance.CurrentLobby.Players.Count;
            for (int i = 0; i < chickensInGame.Length; i++)
            {
                if (chickensInGame[i] != null)
                {
                    chickensInGame[i].enabled = i < playerCount;
                }
            }

            // Update host-only buttons
            bool isHost = LobbySystem.Instance.IsHost();
            foreach (GameObject button in hostOnlyButtons)
            {
                if (button != null)
                {
                    button.SetActive(isHost);
                }
            }
        }
    }
}