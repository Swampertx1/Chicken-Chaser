using UnityEngine;
using UnityEngine.EventSystems;
using GabesCommonUtility.UI.ScriptableObjects;

#if UNITASK
using Cysharp.Threading.Tasks;
using System.Threading;
#else
using System.Collections;
#endif

namespace GabesCommonUtility.UI.Select
{
    [RequireComponent(typeof(RectTransform))]
    public class SelectableScale : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private AnimationDataSo animationData;

        private Vector2 _originalScale;
        private RectTransform _rectTransform;
        private float _currentTransitionTime;
        private bool _isLocked;

#if UNITASK
        private CancellationTokenSource _transitionCts;
#else
        private Coroutine _scaleCoroutine;
#endif
        
        private void Awake()
        {
            _rectTransform = transform as RectTransform;
            
            if (_rectTransform == null) 
                throw new UnityException("Missing component of RectTransform");
            
            if (animationData == null)
                throw new UnityException("AnimationDataSO is not assigned");
            
            _originalScale = animationData.UseLiteralScale ? (Vector2)_rectTransform.localScale : _rectTransform.sizeDelta;
        }

        private void OnDestroy()
        {
#if UNITASK
            _transitionCts?.Cancel();
            _transitionCts?.Dispose();
#endif
        }

        public void OnSelect(BaseEventData eventData)
        {
            Grow();   
        }

        public void OnDeselect(BaseEventData eventData)
        {         
            Shrink();
        }

        public void Grow(bool byPass = false)
        {
            if (_isLocked && !byPass) return;
            
            StopCurrentTransition();
            
            Vector2 currentValue = animationData.UseLiteralScale ? (Vector2)_rectTransform.localScale : _rectTransform.sizeDelta;
            
#if UNITASK
            _transitionCts = new CancellationTokenSource();
            TransitionAsync(currentValue, animationData.HoverSize, _transitionCts.Token).Forget();
#else
            _scaleCoroutine = StartCoroutine(Transition(currentValue, animationData.HoverSize));
#endif
        }

        public void Shrink(bool byPass = false)
        {
            if (_isLocked && !byPass) return;
            
            StopCurrentTransition();
            
            Vector2 currentValue = animationData.UseLiteralScale ? (Vector2)_rectTransform.localScale : _rectTransform.sizeDelta;
            
#if UNITASK
            _transitionCts = new CancellationTokenSource();
            TransitionAsync(currentValue, _originalScale, _transitionCts.Token).Forget();
#else
            _scaleCoroutine = StartCoroutine(Transition(currentValue, _originalScale));
#endif
        }

        private void StopCurrentTransition()
        {
#if UNITASK
            if (_transitionCts != null)
            {
                _transitionCts.Cancel();
                _transitionCts.Dispose();
                _transitionCts = null;
            }
#else
            if (_scaleCoroutine != null)
            {
                StopCoroutine(_scaleCoroutine);
            }
#endif
            _currentTransitionTime = 0;
        }

#if UNITASK
        private async UniTaskVoid TransitionAsync(Vector2 start, Vector2 end, CancellationToken cancellationToken)
        {
            try
            {
                if (animationData.UseLiteralScale)
                {
                    Vector3 currentScale = _rectTransform.localScale;
                    
                    while (_currentTransitionTime < animationData.TransitionDuration)
                    {
                        float t = animationData.TransitionCurve.Evaluate(_currentTransitionTime / animationData.TransitionDuration);
                        
                        currentScale.x = Mathf.LerpUnclamped(start.x, end.x, t);
                        currentScale.y = Mathf.LerpUnclamped(start.y, end.y, t);
                        _rectTransform.localScale = currentScale;
                        
                        _currentTransitionTime += Time.deltaTime;
                        await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
                    }

                    currentScale.x = end.x;
                    currentScale.y = end.y;
                    _rectTransform.localScale = currentScale;
                }
                else
                {
                    while (_currentTransitionTime < animationData.TransitionDuration)
                    {
                        float t = animationData.TransitionCurve.Evaluate(_currentTransitionTime / animationData.TransitionDuration);
                        _rectTransform.sizeDelta = Vector2.LerpUnclamped(start, end, t);
                        _currentTransitionTime += Time.deltaTime;
                        await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
                    }

                    _rectTransform.sizeDelta = end;
                }

                _currentTransitionTime = 0;
            }
            catch (System.OperationCanceledException)
            {
                // Task was cancelled, this is expected behavior
            }
        }
#else
        private IEnumerator Transition(Vector2 start, Vector2 end)
        {
            if (animationData.UseLiteralScale)
            {
                Vector3 currentScale = _rectTransform.localScale;
                
                while (_currentTransitionTime < animationData.TransitionDuration)
                {
                    float t = animationData.TransitionCurve.Evaluate(_currentTransitionTime / animationData.TransitionDuration);
                    
                    currentScale.x = Mathf.LerpUnclamped(start.x, end.x, t);
                    currentScale.y = Mathf.LerpUnclamped(start.y, end.y, t);
                    _rectTransform.localScale = currentScale;
                    
                    _currentTransitionTime += Time.deltaTime;
                    yield return null;
                }

                currentScale.x = end.x;
                currentScale.y = end.y;
                _rectTransform.localScale = currentScale;
            }
            else
            {
                while (_currentTransitionTime < animationData.TransitionDuration)
                {
                    float t = animationData.TransitionCurve.Evaluate(_currentTransitionTime / animationData.TransitionDuration);
                    _rectTransform.sizeDelta = Vector2.LerpUnclamped(start, end, t);
                    _currentTransitionTime += Time.deltaTime;
                    yield return null;
                }

                _rectTransform.sizeDelta = end;
            }

            _currentTransitionTime = 0;
            _scaleCoroutine = null;
        }
#endif

        public void Lock(bool state)
        {
            _isLocked = state;
        }
    }
}