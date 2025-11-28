#if UNITASK
using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

#if SCENE_REFERENCE
using Eflatun.SceneReference;
#endif

namespace GabesCommonUtility.Sequence
{
    public class UnityEventSequence : MonoBehaviour, IEntrySequence
    {
        [SerializeField] private Behaviour next;
        [SerializeField] private UnityEvent action;

        [SerializeField, TextArea] private string optionalDebugMessage;
        public IEntrySequence Default => next as IEntrySequence;
        public bool IsCompleted => false;

        public event Action<string> DisplayMessage;

        public UniTask<IEntrySequence> ExecuteSequence()
        {
            action.Invoke();
            if(!string.IsNullOrEmpty(optionalDebugMessage)) DisplayMessage?.Invoke(optionalDebugMessage);
            return UniTask.FromResult(Default);
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