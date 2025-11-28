using System.Collections;
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

        public static LoadingScreen Instance => _instance;

        private void Awake()
        {
            _instance = this;
            
            if (transitionImage != null)
            {
                _transitionMaterial = transitionImage.material;
            }
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

        /// <summary>
        /// Plays the closing transition (reveals the scene behind)
        /// </summary>
        public void PlayCloseTransition(System.Action onComplete = null)
        {
            StartCoroutine(TransitionScreen(closeTime, closeCurve, false, onComplete));
        }

        /// <summary>
        /// Plays the opening transition (covers the scene)
        /// </summary>
        public void PlayOpenTransition(System.Action onComplete = null)
        {
            StartCoroutine(TransitionScreen(openTime, openCurve, true, onComplete));
        }

        private IEnumerator TransitionScreen(float duration, AnimationCurve curve, bool isOpen, System.Action onComplete)
        {
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
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}