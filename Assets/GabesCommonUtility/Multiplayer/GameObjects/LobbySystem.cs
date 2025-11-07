using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace GabesCommonUtility.Multiplayer.GameObjects
{
    [DefaultExecutionOrder(-1000)]
    public class LobbySystem : MonoBehaviour
    {
        [Header("Settings")] 
        [SerializeField] private bool useHostMigration;
        
        private static LobbySystem _instance;
        private Lobby _lobbyActual;
        private Player _localPlayer;
        
        private readonly Dictionary<string, DataObject> _lobbyData = new();
        
        public readonly LobbyEventCallbacks Events = new();
        public Lobby CurrentLobby => _lobbyActual;
        public static LobbySystem Instance => _instance;
        public bool IsHost() => _lobbyActual != null && _lobbyActual.HostId == AuthenticationService.Instance.PlayerId;
        public string LobbyCode()=> _lobbyActual?.LobbyCode;
        
        private const int HeartbeatTimer = 15000;
        private const int LongTimer = 60000;

        private int _currentTimer =>HeartbeatTimer;
        
        private CancellationTokenSource _cancellationTokenSource;

        #region Initialization

        private void Awake()
        {
            if (_instance && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            Events.DataChanged += CheckStartGame;
            
            Events.PlayerJoined += changes => { Debug.Log("Player joined"); };

            Events.LobbyChanged += changes =>
            {
                Debug.Log("Something changed about the lobby...");
                if (changes.LobbyDeleted)
                {
                    Debug.Log("Hey does this lobby closed happen twice when deleted? Lobby closed");
                }
                else if (changes.PlayerJoined.Changed)
                {
                    Debug.Log("Player joined");
                }
                else if (changes.PlayerLeft.Changed)
                {
                    Debug.Log("Player left");
                }
            };
        }

        #endregion
        

        #region LobbyConnection
        public async UniTask CreateLobby(int maxLobbySize)
        {
            var lobby = await LobbyService.Instance.CreateLobbyAsync(AuthenticationService.Instance.PlayerId + "'s lobby", maxLobbySize, new CreateLobbyOptions()
            {
                IsPrivate = false,
                Player = _localPlayer,
                Data = new()
                {
                    { "Map", new DataObject(DataObject.VisibilityOptions.Member, "") },
                    { "RelayCode", new DataObject(DataObject.VisibilityOptions.Member, "") }
                }
            });
            await BecomeControlLobby(lobby);
        }

        public async UniTask QuickJoinLobby()
        {
            QuickJoinLobbyOptions options = new QuickJoinLobbyOptions
            {
                Filter = new List<QueryFilter>
                {
                    //new(QueryFilter.FieldOptions.AvailableSlots, (numberClients - 1).ToString(), QueryFilter.OpOptions.GT), // Check that there are open slots.
                    new(QueryFilter.FieldOptions.IsLocked, "0", QueryFilter.OpOptions.EQ) // Make sure lobby is not locked
                },
                Player = _localPlayer 
            };
            var lobby = await LobbyService.Instance.QuickJoinLobbyAsync(options);
            await BecomeControlLobby(lobby);
        }

        public async UniTask JoinLobby(string code)
        {
            JoinLobbyByCodeOptions options = new()
            {
                Player = _localPlayer,
            };
            var lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code, options);
            await BecomeControlLobby(lobby);
        }
        #endregion
   
        #region Lobby Updates
        private async UniTask BecomeControlLobby(Lobby newLobby)
        {
            await DisposeLobby();
            _lobbyActual = newLobby;
            await LobbyService.Instance.SubscribeToLobbyEventsAsync(_lobbyActual.Id, Events);
            HeartBeat().Forget();
        }

        public async UniTask UpdateKey(string key, string value, int visibility = 2)
        {
            if (!IsHost())
            {
                Debug.LogError(
                    $"{AuthenticationService.Instance.PlayerId}, just tried to illegally modify the server data");
                return;
            }

            var data = new DataObject((DataObject.VisibilityOptions)visibility, value);
            if (!_lobbyData.TryAdd(key, data)) _lobbyData[key] = data; //if we fail to add, then force it to be this value.
            _lobbyActual = await LobbyService.Instance.UpdateLobbyAsync(_lobbyActual.Id, new() { Data = _lobbyData });
            
            Debug.Log("GABE NOTE TO SELF: can probably cache the lobby ID with player prefs if we get disconnected, and then check if that lobby/relay still exists. If it does, then we should do something about it.");
        }
        
        private async UniTaskVoid HeartBeat()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = _cancellationTokenSource.Token;

            try
            {
                while (true)
                {
                    await UniTask.Delay(_currentTimer, cancellationToken: token); // Delay with cancellation token
                    if (_lobbyActual == null || token.IsCancellationRequested) return;

                    await LobbyService.Instance.SendHeartbeatPingAsync(_lobbyActual.Id);

                    if (token.IsCancellationRequested) return;

                    // You can decide whether to recursively call HeartBeat() or use a loop
                }
            }
            catch (Exception e)
            {
                if (!Application.isPlaying) return;
                // Handle cancellation if necessary
                Debug.LogError("Heartbeat stopped working! " + e);
            }
        }
        
        private async void CheckStartGame(Dictionary<string, ChangedOrRemovedLobbyValue<DataObject>> obj)
        {
            if (useHostMigration) return;
            
            foreach (var x in obj)
            {
                Debug.Log($"{x.Key} --> {x.Value.Value.Value}");
            }

            string code = obj["RelayCode"].Value.Value; // New way of checking if values have changed.
            if (code != "0")
            {
                await RelayHandler.Instance.JoinRelay(code);
                
            }
        }
        
        #endregion

        #region Disposal
        // ReSharper disable Unity.PerformanceAnalysis
        private async UniTask DisposeLobby()
        {
            _lobbyData.Clear();
            _cancellationTokenSource?.Cancel();
            
            if (_lobbyActual == null) return;
            try
            {
                if (_lobbyActual.Players.Count <= 1) await LobbyService.Instance.DeleteLobbyAsync(_lobbyActual.Id);
                else await LobbyService.Instance.RemovePlayerAsync(_lobbyActual.Id, _localPlayer.Id); // Host migration is automatic.
            }
            catch (LobbyServiceException e)
            {
                Debug.LogWarning($"DisposeLobbyAsync failed: {e}");
            }
            _lobbyActual = null;
        }

        private void OnDestroy()
        {
            _ = DisposeLobby();
        }
        #endregion
        
        
    }
}