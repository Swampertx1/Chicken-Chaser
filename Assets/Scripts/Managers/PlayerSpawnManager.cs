using GabesCommonUtility.Multiplayer.GameObjects;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawnManager : NetworkBehaviour
{
   private NetworkVariable<int> playersInGame = new();
   private NetworkVariable<int> chickensInGame = new();
   [SerializeField] private GameSettingsSOAP gameSettingsSOAP;
   [SerializeField] private PlayerSpawnSequence chickenSpawnSequence;
   [SerializeField] private PlayerSpawnSequence humanSpawnSequence;

   
   public override void OnNetworkSpawn()
   {
      base.OnNetworkSpawn();  
      SpawnPlayer_ServerRpc();
   }

   private void SpawnChicken(ulong playerID)
   {
      Debug.Log("Trying to spawn a chicken...");
      chickenSpawnSequence.Spawn_ServerRpc(playerID);
      chickensInGame.Value += 1;
   }

   private void SpawnHuman(ulong playerID)
   {
      humanSpawnSequence.Spawn_ServerRpc(playerID);
   }

   [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]

   private void SpawnPlayer_ServerRpc(RpcParams param = default)
   {
      playersInGame.Value += 1;
      if (NetworkManager.ConnectedClients.Count - playersInGame.Value < gameSettingsSOAP.numChickens - chickensInGame.Value) 
      {
         SpawnChicken(param.Receive.SenderClientId);
         return;
      }
      if (chickensInGame.Value < gameSettingsSOAP.numChickens)
      {
         int rng = Random.Range(0, 2);
         if (rng == 0)
         {
            SpawnChicken(param.Receive.SenderClientId);
         }
         else
         {
            SpawnHuman(param.Receive.SenderClientId);
         }
         return;
      }
      SpawnHuman(param.Receive.SenderClientId);
   }
   
}
