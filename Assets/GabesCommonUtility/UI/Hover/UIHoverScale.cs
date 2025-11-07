using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using GabesCommonUtility.UI.ScriptableObjects;

#if UNITASK
using Cysharp.Threading.Tasks;
using System.Threading;
#else
using System.Collections;
#endif

namespace GabesCommonUtility.UI.Hover
{
    [RequireComponent(typeof(RectTransform))]
    public class UIHoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private AnimationDataSo animationData;
        
        private Vector2 _originalScale;
        private RectTransform _rectTransform;
        private Selectable _selectable;
        private float _currentTransitionTime;
        private bool _isLocked;

#if UNITASK
        private CancellationTokenSource _transitionCts;
#else
        private Coroutine _hoverCoroutine;
#endif

        private void Awake()
        {
            _rectTransform = transform as RectTransform;
            _selectable = GetComponent<Selectable>();
            
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

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_selectable == null || _selectable.IsInteractable()) 
                Grow();
        }

        public void OnPointerExit(PointerEventData eventData)
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
            _hoverCoroutine = StartCoroutine(Transition(currentValue, animationData.HoverSize));
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
            _hoverCoroutine = StartCoroutine(Transition(currentValue, _originalScale));
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
            if (_hoverCoroutine != null)
            {
                StopCoroutine(_hoverCoroutine);
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
            _hoverCoroutine = null;
        }
#endif

        public void Lock(bool state)
        {
            _isLocked = state;
        }
    }
}