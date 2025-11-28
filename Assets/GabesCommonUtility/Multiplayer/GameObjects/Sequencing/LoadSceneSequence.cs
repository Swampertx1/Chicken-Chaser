#if UNITASK
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GabesCommonUtility.Sequence;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_NETCODE_GAMEOBJECTS
using Unity.Netcode;
#endif

#if SCENE_REFERENCE
using Eflatun.SceneReference;
#endif

namespace GabesCommonUtility.Multiplayer.GameObjects.Sequencing
{
    public class LoadSceneSequence : MonoBehaviour, IEntrySequence
    {
        [SerializeField] private Behaviour next;
        
#if SCENE_REFERENCE
        [SerializeField] private SceneReference[] scenesToLoad;
        [SerializeField] private SceneReference[] scenesToUnload;
        [SerializeField] private SceneReference loadingScene;
#else
        [SerializeField] private string[] sceneNamesToLoad;
        [SerializeField] private string[] sceneNamesToUnload;
        [SerializeField] private string loadingSceneName;
#endif
        [SerializeField] private LoadSceneMode loadType = LoadSceneMode.Single;
        
#if UNITY_NETCODE_GAMEOBJECTS
        [SerializeField] private bool waitForAllClientsBeforeUnload = true;
#endif
        
        public IEntrySequence Default => next as IEntrySequence;
        
#if SCENE_REFERENCE
        public bool IsCompleted
        {
            get
            {
                if (scenesToLoad == null || scenesToLoad.Length == 0) return false;
                return SceneManager.GetActiveScene().buildIndex == scenesToLoad[0].BuildIndex;
            }
        }
#else
        public bool IsCompleted
        {
            get
            {
                if (sceneNamesToLoad == null || sceneNamesToLoad.Length == 0) return false;
                return SceneManager.GetActiveScene().name == sceneNamesToLoad[0];
            }
        }
#endif

        public event Action<string> DisplayMessage;

        public async UniTask<IEntrySequence> ExecuteSequence()
        {
            // Validate that we have scenes to load
#if SCENE_REFERENCE
            if (scenesToLoad == null || scenesToLoad.Length == 0)
            {
                Debug.LogError("No scenes to load!");
                return Default;
            }
#else
            if (sceneNamesToLoad == null || sceneNamesToLoad.Length == 0)
            {
                Debug.LogError("No scenes to load!");
                return Default;
            }
#endif

            // Load loading screen first if it exists
            bool hasLoadingScreen = await LoadLoadingScreen();
            
#if UNITY_NETCODE_GAMEOBJECTS
            // Check if we should use networked scene loading
            if (NetworkManager.Singleton != null && 
                NetworkManager.Singleton.IsListening && 
                NetworkManager.Singleton.SceneManager != null)
            {
                return await ExecuteNetworkedSceneLoad(hasLoadingScreen);
            }
#endif
            // Fall back to regular scene loading
            return await ExecuteRegularSceneLoad(hasLoadingScreen);
        }

        private async UniTask<bool> LoadLoadingScreen()
        {
#if SCENE_REFERENCE
            // Check if loading scene reference is set and valid
            if (loadingScene != null && loadingScene.State == SceneReferenceState.Regular)
            {
                await SceneManager.LoadSceneAsync(loadingScene.BuildIndex, LoadSceneMode.Additive);
                
                // Wait for LoadingScreen instance to be available
                await UniTask.WaitUntil(() => LoadingScreen.Instance != null);
                return true;
            }
#else
            // Check if loading scene name is set
            if (!string.IsNullOrEmpty(loadingSceneName))
            {
                await SceneManager.LoadSceneAsync(loadingSceneName, LoadSceneMode.Additive);
                
                // Wait for LoadingScreen instance to be available
                await UniTask.WaitUntil(() => LoadingScreen.Instance != null);
                return true;
            }
#endif
            return false;
        }

        private async UniTask<IEntrySequence> ExecuteRegularSceneLoad(bool hasLoadingScreen)
        {
            // Play open transition (cover screen) if loading screen exists
            if (hasLoadingScreen && LoadingScreen.Instance != null)
            {
                bool transitionComplete = false;
                LoadingScreen.Instance.PlayOpenTransition(() => transitionComplete = true);
                await UniTask.WaitUntil(() => transitionComplete);
            }
            
            if (loadType == LoadSceneMode.Single)
            {
                // Single mode - load the first scene in Single mode
#if SCENE_REFERENCE
                if (scenesToLoad[0] != null && scenesToLoad[0].State == SceneReferenceState.Regular)
                {
                    await SceneManager.LoadSceneAsync(scenesToLoad[0].BuildIndex, LoadSceneMode.Single);
                }
#else
                if (!string.IsNullOrEmpty(sceneNamesToLoad[0]))
                {
                    await SceneManager.LoadSceneAsync(sceneNamesToLoad[0], LoadSceneMode.Single);
                }
#endif
            }
            else
            {
                // Additive mode - unload scenes first
                await UnloadScenes();
                
                // Load all scenes additively
                await LoadScenes();
            }

            // Play close transition (reveal scene) and unload loading screen if it was loaded
            if (hasLoadingScreen)
            {
                if (LoadingScreen.Instance != null)
                {
                    bool transitionComplete = false;
                    LoadingScreen.Instance.PlayCloseTransition(() => transitionComplete = true);
                    await UniTask.WaitUntil(() => transitionComplete);
                }
                
                await UnloadLoadingScreen();
            }

            return Default;
        }

        private async UniTask UnloadScenes()
        {
#if SCENE_REFERENCE
            if (scenesToUnload != null && scenesToUnload.Length > 0)
            {
                List<UniTask> unloadTasks = new List<UniTask>();
                
                foreach (var sceneRef in scenesToUnload)
                {
                    if (sceneRef != null && sceneRef.State == SceneReferenceState.Regular)
                    {
                        // Check if scene is actually loaded before trying to unload
                        Scene scene = SceneManager.GetSceneByBuildIndex(sceneRef.BuildIndex);
                        if (scene.isLoaded)
                        {
                            unloadTasks.Add(SceneManager.UnloadSceneAsync(sceneRef.BuildIndex).ToUniTask());
                        }
                    }
                }
                
                if (unloadTasks.Count > 0)
                {
                    await UniTask.WhenAll(unloadTasks);
                }
            }
#else
            if (sceneNamesToUnload != null && sceneNamesToUnload.Length > 0)
            {
                List<UniTask> unloadTasks = new List<UniTask>();
                
                foreach (var sceneName in sceneNamesToUnload)
                {
                    if (!string.IsNullOrEmpty(sceneName))
                    {
                        // Check if scene is actually loaded before trying to unload
                        Scene scene = SceneManager.GetSceneByName(sceneName);
                        if (scene.isLoaded)
                        {
                            unloadTasks.Add(SceneManager.UnloadSceneAsync(sceneName).ToUniTask());
                        }
                    }
                }
                
                if (unloadTasks.Count > 0)
                {
                    await UniTask.WhenAll(unloadTasks);
                }
            }
#endif
        }

        private async UniTask LoadScenes()
        {
#if SCENE_REFERENCE
            if (scenesToLoad != null && scenesToLoad.Length > 0)
            {
                List<UniTask> loadTasks = new List<UniTask>();
                
                foreach (var sceneRef in scenesToLoad)
                {
                    if (sceneRef != null && sceneRef.State == SceneReferenceState.Regular)
                    {
                        loadTasks.Add(SceneManager.LoadSceneAsync(sceneRef.BuildIndex, LoadSceneMode.Additive).ToUniTask());
                    }
                }
                
                if (loadTasks.Count > 0)
                {
                    await UniTask.WhenAll(loadTasks);
                }
            }
#else
            if (sceneNamesToLoad != null && sceneNamesToLoad.Length > 0)
            {
                List<UniTask> loadTasks = new List<UniTask>();
                
                foreach (var sceneName in sceneNamesToLoad)
                {
                    if (!string.IsNullOrEmpty(sceneName))
                    {
                        loadTasks.Add(SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive).ToUniTask());
                    }
                }
                
                if (loadTasks.Count > 0)
                {
                    await UniTask.WhenAll(loadTasks);
                }
            }
#endif
        }

#if UNITY_NETCODE_GAMEOBJECTS
        private async UniTask<IEntrySequence> ExecuteNetworkedSceneLoad(bool hasLoadingScreen)
        {
            // Only server/host can load networked scenes
            if (!NetworkManager.Singleton.IsServer)
            {
                Debug.LogWarning("Only the server can load networked scenes!");
                
                // Unload loading screen if it was loaded
                if (hasLoadingScreen)
                {
                    if (LoadingScreen.Instance != null)
                    {
                        bool transitionComplete = false;
                        LoadingScreen.Instance.PlayCloseTransition(() => transitionComplete = true);
                        await UniTask.WaitUntil(() => transitionComplete);
                    }
                    await UnloadLoadingScreen();
                }
                
                return Default;
            }

            // Play open transition (cover screen) if loading screen exists
            if (hasLoadingScreen && LoadingScreen.Instance != null)
            {
                bool transitionComplete = false;
                LoadingScreen.Instance.PlayOpenTransition(() => transitionComplete = true);
                await UniTask.WaitUntil(() => transitionComplete);
            }

            if (loadType == LoadSceneMode.Single)
            {
                // Single mode - load the first scene
                bool sceneLoaded = await LoadNetworkedScene(
#if SCENE_REFERENCE
                    scenesToLoad[0].Name,
#else
                    sceneNamesToLoad[0],
#endif
                    LoadSceneMode.Single
                );
                
                if (!sceneLoaded)
                {
                    Debug.LogError("Failed to load networked scene!");
                    if (hasLoadingScreen)
                    {
                        if (LoadingScreen.Instance != null)
                        {
                            bool transitionComplete = false;
                            LoadingScreen.Instance.PlayCloseTransition(() => transitionComplete = true);
                            await UniTask.WaitUntil(() => transitionComplete);
                        }
                        await UnloadLoadingScreen();
                    }
                    return Default;
                }
            }
            else
            {
                // Additive mode
                // Unload scenes first
                await UnloadScenes();
                
                // Load all scenes
                await LoadNetworkedScenes();
            }

            // Play close transition and unload loading screen
            if (hasLoadingScreen)
            {
                if (!waitForAllClientsBeforeUnload)
                {
                    // Don't wait for clients, transition and unload immediately
                    if (LoadingScreen.Instance != null)
                    {
                        bool transitionComplete = false;
                        LoadingScreen.Instance.PlayCloseTransition(() => transitionComplete = true);
                        await UniTask.WaitUntil(() => transitionComplete);
                    }
                    await UnloadLoadingScreen();
                }
                else
                {
                    // Wait for all clients (already handled by LoadNetworkedScene)
                    if (LoadingScreen.Instance != null)
                    {
                        bool transitionComplete = false;
                        LoadingScreen.Instance.PlayCloseTransition(() => transitionComplete = true);
                        await UniTask.WaitUntil(() => transitionComplete);
                    }
                    await UnloadLoadingScreen();
                }
            }

            return Default;
        }

        private async UniTask<bool> LoadNetworkedScene(string sceneNameToLoad, LoadSceneMode mode)
        {
            var completionSource = new UniTaskCompletionSource<bool>();
            
            // Subscribe to scene load completion
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneLoadCompleted;

            void OnSceneLoadCompleted(string sceneName, LoadSceneMode loadSceneMode, 
                List<ulong> clientsCompleted, 
                List<ulong> clientsTimedOut)
            {
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnSceneLoadCompleted;
                completionSource.TrySetResult(clientsTimedOut.Count == 0);
            }

            var sceneEventProgress = NetworkManager.Singleton.SceneManager.LoadScene(
                sceneNameToLoad, 
                mode
            );

            if (sceneEventProgress != SceneEventProgressStatus.Started)
            {
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnSceneLoadCompleted;
                Debug.LogError($"Failed to start networked scene loading. Status: {sceneEventProgress}");
                return false;
            }

            // Wait for the scene load to complete
            return await completionSource.Task;
        }

        private async UniTask LoadNetworkedScenes()
        {
#if SCENE_REFERENCE
            if (scenesToLoad != null && scenesToLoad.Length > 0)
            {
                foreach (var sceneRef in scenesToLoad)
                {
                    if (sceneRef != null && sceneRef.State == SceneReferenceState.Regular)
                    {
                        bool loaded = await LoadNetworkedScene(sceneRef.Name, LoadSceneMode.Additive);
                        if (!loaded)
                        {
                            Debug.LogWarning($"Failed to load networked scene: {sceneRef.Name}");
                        }
                    }
                }
            }
#else
            if (sceneNamesToLoad != null && sceneNamesToLoad.Length > 0)
            {
                foreach (var sceneName in sceneNamesToLoad)
                {
                    if (!string.IsNullOrEmpty(sceneName))
                    {
                        bool loaded = await LoadNetworkedScene(sceneName, LoadSceneMode.Additive);
                        if (!loaded)
                        {
                            Debug.LogWarning($"Failed to load networked scene: {sceneName}");
                        }
                    }
                }
            }
#endif
        }
#endif

        private async UniTask UnloadLoadingScreen()
        {
#if SCENE_REFERENCE
            if (loadingScene != null && loadingScene.State == SceneReferenceState.Regular)
            {
                await SceneManager.UnloadSceneAsync(loadingScene.BuildIndex);
            }
#else
            if (!string.IsNullOrEmpty(loadingSceneName))
            {
                await SceneManager.UnloadSceneAsync(loadingSceneName);
            }
#endif
        }

        private void OnDrawGizmos()
        {
            if (next && Default == null)
            {
                Debug.LogError("Success is INVALID", gameObject);
            }
        }
    }
}
#endif