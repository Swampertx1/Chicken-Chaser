using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using Random = System.Random;

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
        private const int ShortTimer = 2000;
        
        private CancellationTokenSource _cancellationTokenSource;

        public event Action PlayerJoinedReal;
        public event Action PlayerLeftReal;

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
    
            // Initialize local player
            _localPlayer = new Player
            {
                Profile = new("User: " + UnityEngine.Random.Range(0, 10000)), // AuthenticationService.Instance.PlayerId
                Data = new Dictionary<string, PlayerDataObject>()
            };
    
            // Register event handlers (these will work after subscription)
            Events.DataChanged += CheckStartGame;
            Events.PlayerJoined += OnPlayerJoined;
            Events.PlayerLeft += OnPlayerLeft;
            Events.LobbyChanged += OnLobbyChanged;
        }
        
        
        private void OnPlayerJoined(List<LobbyPlayerJoined> players)
        {
            Debug.Log($"Player(s) joined! Count: {players.Count}");
            foreach (var player in players)
            {
                Debug.Log($"Player joined - Index: {player.PlayerIndex}, ID: {player.Player.Id}");
            }
            PlayerLeftReal?.Invoke();
        }

        private void OnPlayerLeft(List<int> playerIndexes)
        {
            Debug.Log($"Player(s) left! Count: {playerIndexes.Count}");
            foreach (var index in playerIndexes)
            {
                Debug.Log($"Player at index {index} left");
            }

            PlayerJoinedReal?.Invoke();
        }

        private void OnLobbyChanged(ILobbyChanges changes)
        {
            Debug.Log("Lobby changed");
            if (changes.LobbyDeleted)
            {
                Debug.Log("Lobby was deleted");
            }
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
    
            Debug.Log($"Lobby created/joined: {_lobbyActual.Id}, Players: {_lobbyActual.Players.Count}");
    
            try
            {
                await LobbyService.Instance.SubscribeToLobbyEventsAsync(_lobbyActual.Id, Events);
                Debug.Log("Successfully subscribed to lobby events");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to subscribe to lobby events: {e}");
            }
    
            // Create cancellation token ONCE for both tasks
            _cancellationTokenSource = new CancellationTokenSource();
    
            HeartBeat().Forget();
            PollLobbyUpdates().Forget();
        }
        
        private async UniTaskVoid PollLobbyUpdates()
        {
            CancellationToken token = _cancellationTokenSource.Token;

            try
            {
                while (!token.IsCancellationRequested && _lobbyActual != null)
                {
                    await UniTask.Delay(ShortTimer, cancellationToken: token); // Poll every 2 seconds
    
                    if (_lobbyActual == null) return;
            
                    try
                    {
                        int lastPlayerCount = _lobbyActual.Players.Count; // Get count BEFORE update
                
                        var updatedLobby = await LobbyService.Instance.GetLobbyAsync(_lobbyActual.Id);
                        int currentPlayerCount = updatedLobby.Players.Count;
                
                        Debug.Log($"Poll: Last={lastPlayerCount}, Current={currentPlayerCount}"); // Debug log
                
                        // Update the lobby reference AFTER comparison
                        _lobbyActual = updatedLobby;
                
                        // Now trigger events based on comparison
                        if (currentPlayerCount > lastPlayerCount)
                        {
                            Debug.Log("Player joined detected!");
                            PlayerJoinedReal?.Invoke();
                        }
                        else if (currentPlayerCount < lastPlayerCount)
                        {
                            Debug.Log("Player left detected!");
                            PlayerLeftReal?.Invoke();
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Polling error: {e}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Polling cancelled");
            }
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
            CancellationToken token = _cancellationTokenSource.Token; // Use existing token

            try
            {
                while (true)
                {
                    
                    
                    await UniTask.Delay(HeartbeatTimer, cancellationToken: token); // Delay with cancellation token

                    if (!IsHost()) continue;
                    
                    if (_lobbyActual == null || token.IsCancellationRequested) return;

                    await LobbyService.Instance.SendHeartbeatPingAsync(_lobbyActual.Id);
                    if (token.IsCancellationRequested) return;

                    // You can decide whether to recursively call HeartBeat() or use a loop
                }
            }
            catch (Exception e)
            {
                if (!Application.isPlaying) return;
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