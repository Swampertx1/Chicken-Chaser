#if UNITASK
using System;
using Cysharp.Threading.Tasks;
using GabesCommonUtility.Sequence;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

#if SCENE_REFERENCE
using Eflatun.SceneReference;
#endif

namespace GabesCommonUtility.Multiplayer.GameObjects.Sequencing
{
    public class LoadSceneSequence : MonoBehaviour, IEntrySequence
    {
        [SerializeField] private Behaviour next;
        
#if SCENE_REFERENCE
        [SerializeField] private SceneReference selectionScene;
#else
        [SerializeField] private string sceneName;
#endif
        [SerializeField] private LoadSceneMode loadType = LoadSceneMode.Single;
        
        public IEntrySequence Default => next as IEntrySequence;
        
#if SCENE_REFERENCE
        public bool IsCompleted => SceneManager.GetActiveScene().buildIndex == selectionScene.BuildIndex;
#else
        public bool IsCompleted => SceneManager.GetActiveScene().name == sceneName;
#endif

        public event Action<string> DisplayMessage;

        public async UniTask<IEntrySequence> ExecuteSequence()
        {
            
            
            if (NetworkManager.Singleton == null || NetworkManager.Singleton.SceneManager == null)
            {
                Debug.LogError("NetworkManager or SceneManager is not initialized!");
                return Default;
            }

            // Only server/host can load networked scenes
            if (!NetworkManager.Singleton.IsServer)
            {
                Debug.LogWarning("Only the server can load networked scenes!");
                return Default;
            }

            var completionSource = new UniTaskCompletionSource();
            
            // Subscribe to scene load completion
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneLoadCompleted;

            void OnSceneLoadCompleted(string sceneName, LoadSceneMode loadSceneMode, System.Collections.Generic.List<ulong> clientsCompleted, System.Collections.Generic.List<ulong> clientsTimedOut)
            {
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnSceneLoadCompleted;
                completionSource.TrySetResult();
            }

#if SCENE_REFERENCE
            var sceneEventProgress = NetworkManager.Singleton.SceneManager.LoadScene(
                selectionScene.Name, 
                loadType
            );
#else
            var sceneEventProgress = NetworkManager.Singleton.SceneManager.LoadScene(
                sceneName, 
                loadType
            );
#endif

            if (sceneEventProgress != SceneEventProgressStatus.Started)
            {
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnSceneLoadCompleted;
                Debug.LogError($"Failed to start scene loading. Status: {sceneEventProgress}");
                return Default;
            }

            // Wait for the scene load to complete
            await completionSource.Task;

            return Default;
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