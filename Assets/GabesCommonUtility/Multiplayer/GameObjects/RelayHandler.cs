#if UNITY_SERVICES && UNITY_NETCODE_GAMEOBJECTS

using System;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GabesCommonUtility.Multiplayer.GameObjects
{
    public class RelayHandler : IDisposable
    {
        private const string ConnectionType = "udp";

        private static RelayHandler _instance;
        public static RelayHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new RelayHandler();
                }
                return _instance;
            }
        }

        private bool _isInitialized;

        private RelayHandler()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (_isInitialized) return;

            if (NetworkManager.Singleton == null)
            {
                Debug.LogError("NetworkManager.Singleton is null. Cannot initialize RelayHandler.");
                return;
            }

            NetworkManager.Singleton.OnServerStarted += OnServerStarted;
            NetworkManager.Singleton.OnClientStarted += OnClientStarted;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

            _isInitialized = true;
        }

        private void OnServerStarted()
        {
            Debug.LogWarning("Server Started");
        }

        private void OnClientStarted()
        {
            Debug.LogWarning("Client Started");
        }

        private void OnClientDisconnect(ulong clientId)
        {
            Debug.LogWarning($"Client {clientId} disconnected.");
        }

        private void OnClientConnected(ulong id)
        {
            Debug.Log("I connected as: " + id);
            if (!NetworkManager.Singleton.IsHost) return;
            
            foreach (var variable in NetworkManager.Singleton.ConnectedClientsIds)
            {
                Debug.Log("Clients connected: " + variable);
            }
        }

        public bool IsConnected() => NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient;

        public async UniTask<string> CreateRelay(int maxPlayers, string region = null)
        {
            try
            {
                var hostAllocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers, region);
                string joinCode = await RelayService.Instance.GetJoinCodeAsync(hostAllocation.AllocationId);
                
                Debug.Log("Creating Relay: " + joinCode);

                var relayServerData = hostAllocation.ToRelayServerData(ConnectionType);

                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
                NetworkManager.Singleton.StartHost();

                NetworkManager.Singleton.SceneManager.ActiveSceneSynchronizationEnabled = true;
                NetworkManager.Singleton.SceneManager.SetClientSynchronizationMode(LoadSceneMode.Single);

                Debug.Log("Connected to Relay as host: " + NetworkManager.Singleton.IsHost);
                NetworkManager.Singleton.SceneManager.OnLoad += OnSceneLoad;
                
                return joinCode;
            }
            catch (RelayServiceException e)
            {
                Debug.LogError("Failed while trying to create relay: " + e);
            }
            return null;
        }

        public async UniTask<bool> JoinRelay(string joinCode)
        {
            try
            {
                Debug.Log("Joining with code: " + joinCode);

                var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
                var relayServerData = joinAllocation.ToRelayServerData(ConnectionType);

                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
                NetworkManager.Singleton.StartClient();
                NetworkManager.Singleton.SceneManager.OnLoad += OnSceneLoad;
                
                Debug.Log("Connected to Relay as client: " + NetworkManager.Singleton.IsClient);
                return true;
            }
            catch (RelayServiceException e)
            {
                Debug.LogError("Failed while trying to join relay: " + e);
            }

            return false;
        }

        private void OnSceneLoad(ulong clientId, string sceneName, LoadSceneMode loadSceneMode, AsyncOperation asyncOperation)
        {
            Debug.Log($"Client {clientId} is loading scene {sceneName} with mode {loadSceneMode}");
        }

        public void Dispose()
        {
            if (!_isInitialized) return;

            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
                NetworkManager.Singleton.OnClientStarted -= OnClientStarted;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                
                if (NetworkManager.Singleton.SceneManager != null)
                {
                    NetworkManager.Singleton.SceneManager.OnLoad -= OnSceneLoad;
                }
            }

            _isInitialized = false;
            _instance = null;
        }
    }
}
#endif