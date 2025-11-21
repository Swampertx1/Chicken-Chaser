using GabesCommonUtility.Multiplayer.GameObjects;
using Unity.Services.Lobbies;
using UnityEngine;

namespace Niche_Things
{
    public class NetowkredLobbyController : MonoBehaviour
    {
        [SerializeField] private ChickenLightUp[] chickensInGame;
        private void OnEnable()
        {
            Homework();
         UpdateButtons(null);
         LobbySystem.Instance.Events.LobbyChanged +=  UpdateButtons;
        }

        private void OnDisable()
        {
            LobbySystem.Instance.Events.LobbyChanged -=  UpdateButtons;
        }

        private void UpdateButtons(ILobbyChanges obj)
        {
            
       
            for (int i = chickensInGame.Length -1; i >= 0 ; i -= 1)
            {
                chickensInGame[i].enabled = LobbySystem.Instance.CurrentLobby.Players.Count > i;
                //  Debug.Log("Saint "+i);
            }
            
           
        }

        private void Homework()
        {
            //Starting at 0, counting until 10, increasing by 1.
            for(int i = 0; i < 10; i+=1)
            {
                Debug.Log("idk" + i);
            }
        }
    
    
    }
}

