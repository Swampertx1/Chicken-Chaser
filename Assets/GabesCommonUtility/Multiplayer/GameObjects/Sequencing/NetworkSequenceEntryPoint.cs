using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GabesCommonUtility.Sequence;
using Unity.Netcode;
using UnityEngine;

namespace GabesCommonUtility
{
    public class NetworkSequenceEntryPoint : NetworkBehaviour
    {
        [SerializeField] private Behaviour runOnHost;
        [SerializeField] private Behaviour runOnClient;
        
        [SerializeField] private bool activateOnSpawn = true;
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
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            // Determine which sequence to run based on network role
            IEntrySequence startSequence = IsServer 
                ? runOnHost as IEntrySequence 
                : runOnClient as IEntrySequence;
            
            // Precount total chain length
            _chainLength = CountChain(startSequence);
            
            if (activateOnSpawn) ActivateAndForget();
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            
            // Cancel ongoing sequence when despawned
            _cts?.Cancel();
            _cts?.Dispose();
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
                
                // Select sequence based on network role
                _next = IsServer 
                    ? runOnHost as IEntrySequence 
                    : runOnClient as IEntrySequence;

                string role = IsServer ? "Host/Server" : "Client";
                Debug.Log($"Starting Network Loading Sequence as {role}...");

                while (_next != null && !token.IsCancellationRequested)
                {
                    _next.DisplayMessage += OnDisplayMessage;
                    
                    var next = await _next.ExecuteSequence().AttachExternalCancellation(token);

                    _currentLength++;
                    OnProgress?.Invoke();
                    
                    _next = next;
                    
                    Debug.Log($"[{role}] Success, currently on stage number: {_currentLength}", gameObject);
                }

                if (!token.IsCancellationRequested)
                {
                    Debug.Log($"[{role}] Loading Sequence Complete");
                    OnFinish?.Invoke();
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("NetworkSequenceEntryPoint sequence canceled.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"NetworkSequenceEntryPoint encountered an error: {ex}");
            }
        }

        private void OnDisplayMessage(string obj)
        {
            Debug.LogWarning("IMPLEMENT THIS CORRECTLY");
            Debug.Log(obj);
        }
    }
}