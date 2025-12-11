using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace GabesCommonUtility
{
    public class LoadingScreen : MonoBehaviour
    {
        
        private static readonly int FillMatID = Shader.PropertyToID("_Fill");

        
        [SerializeField] private Image transitionImage;
        [SerializeField] private GameObject textBlocks;
        
        [SerializeField] private float closeTime = 1f;
        [SerializeField] private float openTime = 1f;
        [SerializeField] private AnimationCurve closeCurve;
        [SerializeField] private AnimationCurve openCurve;

        private Material _transitionMaterial;
        private static LoadingScreen _instance;
        private Canvas _canvas;

        public static LoadingScreen Instance => _instance;

        private void Awake()
        {
            // Handle singleton with DontDestroyOnLoad for prefab persistence
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Ensure we have a Canvas component
            _canvas = GetComponent<Canvas>();
            if (_canvas == null)
            {
                _canvas = gameObject.AddComponent<Canvas>();
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _canvas.sortingOrder = 9999; // Ensure it renders on top
                
                // Add required components for Canvas
                if (GetComponent<CanvasScaler>() == null)
                {
                    gameObject.AddComponent<CanvasScaler>();
                }
                if (GetComponent<GraphicRaycaster>() == null)
                {
                    gameObject.AddComponent<GraphicRaycaster>();
                }
            }
            
            if (transitionImage != null)
            {
                // Create instance of material to avoid modifying the shared material
                _transitionMaterial = new Material(transitionImage.material);
                transitionImage.material = _transitionMaterial;
            }
            
            SetActive(false);
        }

        private void Start()
        {
            // Start with the screen open (filled)
            if (_transitionMaterial != null)
            {
                _transitionMaterial.SetFloat(FillMatID, openCurve.Evaluate(1));
            }
            
            if (textBlocks != null)
            {
                textBlocks.SetActive(true);
            }
        }

        public void SetActive(bool isActive) => _canvas.enabled = isActive;

        /// <summary>
        /// Plays the closing transition (reveals the scene behind) - Awaitable
        /// </summary>
        public Task PlayCloseTransitionAsync()
        {
            var tcs = new TaskCompletionSource<bool>();
            StartCoroutine(TransitionScreen(closeTime, closeCurve, false, () => tcs.SetResult(true)));
            return tcs.Task;
        }

        /// <summary>
        /// Plays the opening transition (covers the scene) - Awaitable
        /// </summary>
        public Task PlayOpenTransitionAsync()
        {
            var tcs = new TaskCompletionSource<bool>();
            StartCoroutine(TransitionScreen(openTime, openCurve, true, () => tcs.SetResult(true)));
            return tcs.Task;
        }

        /// <summary>
        /// Plays the closing transition (reveals the scene behind) - Callback version
        /// </summary>
        public void PlayCloseTransition(System.Action onComplete = null)
        {
            StartCoroutine(TransitionScreen(closeTime, closeCurve, false, onComplete));
        }

        /// <summary>
        /// Plays the opening transition (covers the scene) - Callback version
        /// </summary>
        public void PlayOpenTransition(System.Action onComplete = null)
        {
            StartCoroutine(TransitionScreen(openTime, openCurve, true, onComplete));
        }

        private IEnumerator TransitionScreen(float duration, AnimationCurve curve, bool isOpen, System.Action onComplete)
        {
            SetActive(true);
            
            float elapsed = 0;

            if (!isOpen && textBlocks != null)
            {
                textBlocks.SetActive(false);
            }

            if (_transitionMaterial != null)
            {
                _transitionMaterial.SetFloat(FillMatID, curve.Evaluate(0));
            }
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float perc = elapsed / duration;
                float eval = curve.Evaluate(perc);

                if (_transitionMaterial != null)
                {
                    _transitionMaterial.SetFloat(FillMatID, eval);
                }
                
                yield return null;
            }

            if (isOpen && textBlocks != null)
            {
                textBlocks.SetActive(true);
            }

            if (_transitionMaterial != null)
            {
                _transitionMaterial.SetFloat(FillMatID, curve.Evaluate(1));
            }

            onComplete?.Invoke();
            
            SetActive(isOpen); 

        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
            
            // Clean up instantiated material
            if (_transitionMaterial != null)
            {
                Destroy(_transitionMaterial);
            }
        }
    }
}