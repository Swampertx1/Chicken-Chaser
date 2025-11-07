#if UNITASK
using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

#if SCENE_REFERENCE
using Eflatun.SceneReference;
#endif

namespace GabesCommonUtility.Sequence
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
        public bool IsCompleted => SceneManager.GetActiveScene().buildIndex == selectionScene.BuildIndex;

        public event Action<string> DisplayMessage;

        public async UniTask<IEntrySequence> ExecuteSequence()
        {
#if SCENE_REFERENCE
            await SceneManager.LoadSceneAsync(selectionScene.BuildIndex, loadType);
#else
            await SceneManager.LoadSceneAsync(sceneName, loadType);
#endif
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