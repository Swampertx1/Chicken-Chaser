using GabesCommonUtility.Multiplayer.GameObjects;
using Unity.Services.Lobbies;
using UnityEngine;

namespace Niche_Things
{
    public class NetowkredLobbyController : MonoBehaviour
    {
        [SerializeField] private ChickenLightUp[] chickensInGame;
        [SerializeField] private GameObject[] hostOnlyButtons;
        private void OnEnable()
        {
            UpdateButtons(null);
            LobbySystem.Instance.Events.LobbyChanged +=  UpdateButtons;
            LobbySystem.Instance.Events.LobbyEventConnectionStateChanged += LobbyEventConnectionChanged;
        }

        private void LobbyEventConnectionChanged(LobbyEventConnectionState obj)
        {
            Debug.Log("Lobby state connection has changed: " + obj);
            UpdateButtons(null);
        }

        private void OnDisable()
        {
            LobbySystem.Instance.Events.LobbyChanged -=  UpdateButtons;
            LobbySystem.Instance.Events.LobbyEventConnectionStateChanged -= LobbyEventConnectionChanged;

        }

        private void UpdateButtons(ILobbyChanges obj)
        {
            if (LobbySystem.Instance.CurrentLobby == null) return;
            Debug.Log("Lobby has changed... Updating Buttons");
            for (int i = chickensInGame.Length -1; i >= 0 ; i -= 1)
            {
                chickensInGame[i].enabled = i< LobbySystem.Instance.CurrentLobby.Players.Count;
                //  Debug.Log("Saint "+i);
            }

            foreach (GameObject g in hostOnlyButtons)
            {
                g.SetActive(LobbySystem.Instance.IsHost());
            }
            
           
        }

    
    }
}

