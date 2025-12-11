using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using GabesCommonUtility.Sequence;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_NETCODE_GAMEOBJECTS
using Unity.Netcode;
using System.Collections.Generic;
#endif

#if SCENE_REFERENCE
using Eflatun.SceneReference;
#endif

namespace GabesCommonUtility.Multiplayer.GameObjects.Sequencing
{
    [ExecuteInEditMode]
    public class LoadSceneSequence : MonoBehaviour, IEntrySequence
    {
        [Serializable]
        private struct LoadScene
        {
#if UNITY_NETCODE_GAMEOBJECTS
            public bool isLocal;
#endif
#if SCENE_REFERENCE
            [SerializeField] private SceneReference sceneReference;
            public string SceneName => sceneReference.Name;
#else
            [SerializeField] private string sceneName; 
            public string SceneName => sceneName;
#endif
        }
        
        [SerializeField] private Behaviour next;
        
        [SerializeField] private LoadSceneMode loadMode;
        [SerializeField] private LoadScene[] scenesToLoad;
        [SerializeField] private LoadScene[] scenesToUnload;
        
        [SerializeField] private bool useLoadingScreen;
        
        public IEntrySequence Default => next as IEntrySequence;
        
        public bool IsCompleted
        {
            get
            {
                if (scenesToLoad == null || scenesToLoad.Length == 0) return true;
                for (int i = 0; i < SceneManager.sceneCount; ++i)
                {
                    var s = SceneManager.GetSceneAt(i);
                    if(scenesToLoad.All(x => x.SceneName != s.name)) return false;
                }
                return true;
            }
        }

        public event Action<string> DisplayMessage;

        public async UniTask<IEntrySequence> ExecuteSequence()
        {
            
            string displayMessage = "Loading Offline only.";
#if UNITY_NETCODE_GAMEOBJECTS
            if(NetworkManager.Singleton)
            {
                displayMessage = NetworkManager.Singleton.IsServer  
                    ? "Loading as server, BE AWARE: Make sure clients are also loading. "
                    : "Loading as client, BE AWARE: Only offline scenes are loaded. ";
                
                NetworkManager.Singleton.SceneManager.SetClientSynchronizationMode(loadMode);
            }
#endif
            Debug.Log($"LoadSceneSequence Initiating, with a loading screen? {useLoadingScreen}. {displayMessage}");

            if (useLoadingScreen) await LoadingScreen.Instance.PlayOpenTransitionAsync();

            if (loadMode == LoadSceneMode.Single) 
                await LoadSingleScene();
            else 
                await HandleAdditiveScenes();

            if (useLoadingScreen)
                await LoadingScreen.Instance.PlayCloseTransitionAsync();

            return Default;
        }

        private async UniTask HandleAdditiveScenes()
        {
            await UnloadAdditive();
            await LoadAdditive();
        }
        
        private async UniTask UnloadAdditive()
        {
            if (scenesToUnload == null || scenesToUnload.Length == 0) return;

            foreach (var scene in scenesToUnload)
            {
#if UNITY_NETCODE_GAMEOBJECTS
                if (!scene.isLocal)
                {
                    if (NetworkManager.Singleton)
                    {
                        if (NetworkManager.Singleton.IsServer)
                        {
                            await UnloadNetworkedSceneAsync(scene.SceneName);
                        }
                        else if (NetworkManager.Singleton.IsClient)
                        {
                            await WaitForNetworkedSceneUnloadAsync(scene.SceneName);
                        }
                        continue;
                    }
                    Debug.LogError($"Tried to unload networked scene '{scene.SceneName}' while not connected to a server");
                    continue;
                }
#endif
                await SceneManager.UnloadSceneAsync(scene.SceneName);
            }
        }
        
        private async UniTask LoadAdditive()
        {
            if (scenesToLoad == null || scenesToLoad.Length == 0) return;

            foreach (var scene in scenesToLoad)
            {
#if UNITY_NETCODE_GAMEOBJECTS
                if (!scene.isLocal)
                {
                    if (NetworkManager.Singleton)
                    {
                        if (NetworkManager.Singleton.IsServer)
                        {
                            await LoadNetworkedSceneAsync(scene.SceneName, LoadSceneMode.Additive);
                        }
                        else if (NetworkManager.Singleton.IsClient)
                        {
                            await WaitForNetworkedSceneLoadAsync(scene.SceneName);
                        }
                        continue;
                    }
                    Debug.LogError($"Tried to load networked scene '{scene.SceneName}' while not connected to a server");
                    continue;
                }
#endif
                await SceneManager.LoadSceneAsync(scene.SceneName, LoadSceneMode.Additive);
            }
        }

        private async UniTask LoadSingleScene()
        {
            #if UNITY_EDITOR
            if (scenesToLoad.Length != 1)
            {
                Debug.LogError("THERE IS AN INVALID NUMBER OF SCENES IN scenesToLoad. LOADING HAS BEEN SKIPPED");
                return;
            }
            #endif
            
            LoadScene scene = scenesToLoad[0];
            
            #if UNITY_NETCODE_GAMEOBJECTS
            if (!scene.isLocal)
            {
                if (NetworkManager.Singleton)
                {
                    // Server/Host initiates the load, clients wait for it
                    if (NetworkManager.Singleton.IsServer)
                    {
                        await LoadNetworkedSceneAsync(scene.SceneName, LoadSceneMode.Single);
                    }
                    else if (NetworkManager.Singleton.IsClient)
                    {
                        await WaitForNetworkedSceneLoadAsync(scene.SceneName);
                    }
                    return;
                }
                Debug.LogError("Tried to load a networked scene while not being connected to a server");
                return;
            }
            #endif

            await SceneManager.LoadSceneAsync(scene.SceneName, LoadSceneMode.Single);
        }

#if UNITY_NETCODE_GAMEOBJECTS
        private UniTask LoadNetworkedSceneAsync(string sceneName, LoadSceneMode mode)
        {
            var tcs = new UniTaskCompletionSource();
            
            void OnLoadEventCompleted(string loadedSceneName, LoadSceneMode loadMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
            {
                if (loadedSceneName == sceneName)
                {
                    NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
                    
                    if (clientsTimedOut.Count > 0)
                    {
                        Debug.LogWarning($"Scene '{sceneName}' loaded, but {clientsTimedOut.Count} client(s) timed out.");
                    }
                    
                    tcs.TrySetResult();
                }
            }
            
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
            
            var status = NetworkManager.Singleton.SceneManager.LoadScene(sceneName, mode);
            
            if (status != SceneEventProgressStatus.Started)
            {
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
                Debug.LogError($"Failed to start networked scene load: {status}");
                tcs.TrySetResult();
            }
            
            return tcs.Task;
        }

        private UniTask WaitForNetworkedSceneLoadAsync(string sceneName)
        {
            var tcs = new UniTaskCompletionSource();
            
            void OnLoadComplete(ulong clientId, string loadedSceneName, LoadSceneMode mode)
            {
                if (loadedSceneName == sceneName && clientId == NetworkManager.Singleton.LocalClientId)
                {
                    NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnLoadComplete;
                    tcs.TrySetResult();
                }
            }
            
            // Check if we're already in the target scene
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i).name == sceneName)
                {
                    tcs.TrySetResult();
                    return tcs.Task;
                }
            }
            
            NetworkManager.Singleton.SceneManager.OnLoadComplete += OnLoadComplete;
            
            return tcs.Task;
        }

        private UniTask UnloadNetworkedSceneAsync(string sceneName)
        {
            var tcs = new UniTaskCompletionSource();
            
            void OnUnloadEventCompleted(string unloadedSceneName, LoadSceneMode loadSceneMode, List<ulong> clientsTimedOut, List<ulong> ulongs)
            {
                if (unloadedSceneName == sceneName)
                {
                    NetworkManager.Singleton.SceneManager.OnUnloadEventCompleted -= OnUnloadEventCompleted;
                    
                    if (clientsTimedOut.Count > 0)
                    {
                        Debug.LogWarning($"Scene '{sceneName}' unloaded, but {clientsTimedOut.Count} client(s) timed out.");
                    }
                    
                    tcs.TrySetResult();
                }
            }
            
            NetworkManager.Singleton.SceneManager.OnUnloadEventCompleted += OnUnloadEventCompleted;
            
            var status = NetworkManager.Singleton.SceneManager.UnloadScene(SceneManager.GetSceneByName(sceneName));
            
            if (status != SceneEventProgressStatus.Started)
            {
                NetworkManager.Singleton.SceneManager.OnUnloadEventCompleted -= OnUnloadEventCompleted;
                Debug.LogError($"Failed to start networked scene unload: {status}");
                tcs.TrySetResult();
            }
            
            return tcs.Task;
        }

        private UniTask WaitForNetworkedSceneUnloadAsync(string sceneName)
        {
            var tcs = new UniTaskCompletionSource();
            
            void OnUnloadComplete(ulong clientId, string unloadedSceneName)
            {
                if (unloadedSceneName == sceneName && clientId == NetworkManager.Singleton.LocalClientId)
                {
                    NetworkManager.Singleton.SceneManager.OnUnloadComplete -= OnUnloadComplete;
                    tcs.TrySetResult();
                }
            }
            
            // Check if scene is already unloaded
            bool sceneExists = false;
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i).name == sceneName)
                {
                    sceneExists = true;
                    break;
                }
            }
            
            if (!sceneExists)
            {
                tcs.TrySetResult();
                return tcs.Task;
            }
            
            NetworkManager.Singleton.SceneManager.OnUnloadComplete += OnUnloadComplete;
            
            return tcs.Task;
        }
#endif

        #if UNITY_EDITOR
        private void Start()
        {
            if (Application.isPlaying) return;
            
            if (next && Default == null)
                Debug.LogError("Success is INVALID", gameObject);
            
            if (useLoadingScreen && !FindFirstObjectByType<LoadingScreen>())
                Debug.LogError("The loading GameObject is requested, but is not being used. Check common utility for a default example.");
        }
        #endif
    }
}