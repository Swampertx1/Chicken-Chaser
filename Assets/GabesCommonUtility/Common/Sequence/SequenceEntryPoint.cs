#if UNITASK
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GabesCommonUtility.Sequence
{
    /*
     * The SequenceStartPoint is responsible for running a chain of IEntrySequence steps in order.
     * Each step is awaited asynchronously using UniTask.
     */
    public class SequenceEntryPoint : MonoBehaviour
    {
        [SerializeField] private Behaviour start;
        
        [SerializeField] private bool activateOnStart = true;
        [SerializeField] private bool persist = false;

        private int _chainLength;
        private int _currentLength;
        public float Progress => _chainLength == 0 ? 1f : (float)_currentLength / _chainLength;

        public event Action OnProgress;
        public event Action OnFinish;

        private IEntrySequence _next;
        private CancellationTokenSource _cts;

        private void Awake()
        {
            if (persist) DontDestroyOnLoad(gameObject);

            // Precount total chain length
            _chainLength = CountChain(start as IEntrySequence);
        }

        private void Start()
        {
            if (activateOnStart) ActivateAndForget();
        }

        private void OnDestroy()
        {
            // Cancel ongoing sequence when destroyed
            _cts?.Cancel();
            _cts?.Dispose();
        }

        private int CountChain(IEntrySequence startPoint)
        {
            int count = 0;
            var current = startPoint;
            while (current != null)
            {
                count++;
                current = current.Default;
            }
            return count;
        }

        public void ActivateAndForget() => Activate().Forget();

        public async UniTaskVoid Activate()
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            var token = _cts.Token;

            try
            {
                _currentLength = 0;
                _next = start as IEntrySequence;

                Debug.Log("Starting Loading Sequence...");

                while (_next != null && !token.IsCancellationRequested)
                {
                    _next.DisplayMessage += OnDisplayMessage;
                    
                    var next = await _next.ExecuteSequence().AttachExternalCancellation(token);

                    _currentLength++;
                    OnProgress?.Invoke();
                    
                    _next = next;
                    
                    Debug.Log("Success, currently on stage number: " +  _currentLength, gameObject);
                    
                }

                if (!token.IsCancellationRequested)
                {
                    Debug.Log("Loading Sequence Complete");
                    OnFinish?.Invoke();
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("SequenceStartPoint sequence canceled.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"SequenceStartPoint encountered an error: {ex}");
            }
        }

        private void OnDisplayMessage(string obj)
        {
            Debug.LogWarning("IMPLEMENT THIS CORRECTLY");
            Debug.Log(obj);
        }
    }
}
#endif
