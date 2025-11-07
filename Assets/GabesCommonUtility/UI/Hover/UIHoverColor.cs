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
    [RequireComponent(typeof(Graphic))]
    public class UIHoverColor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private ColorAnimationDataSo animationData;

        private Color _originalColor;
        private Color _hoverColor;
        private Graphic _graphic;
        private Selectable _selectable;
        private float _currentTransitionTime;
        private bool _isLocked;
        private Gradient _runtimeGradient;

#if UNITASK
        private CancellationTokenSource _transitionCts;
#else
        private Coroutine _colorCoroutine;
#endif

        private void Awake()
        {          
            _selectable = GetComponent<Selectable>();
            _graphic = GetComponent<Graphic>();
            
            if (_graphic == null)
                throw new UnityException("Missing Graphic component (Image, Text, etc.)");
            
            if (animationData == null)
                throw new UnityException("ColorAnimationDataSo is not assigned");

            _originalColor = _graphic.color;
            
            // Create a runtime copy of the gradient to avoid modifying the ScriptableObject
            if (animationData.ColorGradient != null)
            {
                _runtimeGradient = new Gradient();
                _runtimeGradient.SetKeys(
                    animationData.ColorGradient.colorKeys, 
                    animationData.ColorGradient.alphaKeys
                );
                
                // If using original color as start, modify the runtime gradient
                if (animationData.UseOriginalColorAsStart)
                {
                    GradientColorKey[] colorKeys = _runtimeGradient.colorKeys;
                    if (colorKeys.Length > 0)
                    {
                        colorKeys[0] = new GradientColorKey(_originalColor, 0f);
                        _runtimeGradient.SetKeys(colorKeys, _runtimeGradient.alphaKeys);
                    }
                }
            }
            
            _hoverColor = _runtimeGradient?.Evaluate(1f) ?? Color.white;
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
            if (_selectable != null && _selectable.IsInteractable()) return;
            ChangeToHoverColor();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ChangeToOriginalColor();
        }

        public void ChangeToHoverColor(bool byPass = false)
        {
            if (_isLocked && !byPass) return;
            
            StopCurrentTransition();
            
#if UNITASK
            _transitionCts = new CancellationTokenSource();
            TransitionColorAsync(_graphic.color, _hoverColor, _transitionCts.Token).Forget();
#else
            _colorCoroutine = StartCoroutine(TransitionColor(_graphic.color, _hoverColor));
#endif
        }

        public void ChangeToOriginalColor(bool byPass = false)
        {
            if (_isLocked && !byPass) return;
            
            StopCurrentTransition();
            
#if UNITASK
            _transitionCts = new CancellationTokenSource();
            TransitionColorAsync(_graphic.color, _originalColor, _transitionCts.Token).Forget();
#else
            _colorCoroutine = StartCoroutine(TransitionColor(_graphic.color, _originalColor));
#endif
        }

        private void StopCurrentTransition()
        {
#if UNITASK
            if (_transitionCts != null)
            {
                _currentTransitionTime = animationData.TransitionDuration - _currentTransitionTime;
                _transitionCts.Cancel();
                _transitionCts.Dispose();
                _transitionCts = null;
            }
#else
            if (_colorCoroutine != null)
            {
                _currentTransitionTime = animationData.TransitionDuration - _currentTransitionTime;
                StopCoroutine(_colorCoroutine);
            }
#endif
        }

#if UNITASK
        private async UniTaskVoid TransitionColorAsync(Color startColor, Color endColor, CancellationToken cancellationToken)
        {
            try
            {
                while (_currentTransitionTime < animationData.TransitionDuration)
                {
                    float normalizedTime = _currentTransitionTime / animationData.TransitionDuration;
                    float curveValue = animationData.TransitionCurve.Evaluate(normalizedTime);
                    
                    _graphic.color = _runtimeGradient != null 
                        ? _runtimeGradient.Evaluate(curveValue)
                        : Color.LerpUnclamped(startColor, endColor, curveValue);
                    
                    _currentTransitionTime += Time.deltaTime;
                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
                }

                _graphic.color = endColor;
                _currentTransitionTime = 0;
            }
            catch (System.OperationCanceledException)
            {
                // Task was cancelled, this is expected behavior
            }
        }
#else
        private IEnumerator TransitionColor(Color startColor, Color endColor)
        {
            while (_currentTransitionTime < animationData.TransitionDuration)
            {
                float normalizedTime = _currentTransitionTime / animationData.TransitionDuration;
                float curveValue = animationData.TransitionCurve.Evaluate(normalizedTime);
                
                _graphic.color = _runtimeGradient != null 
                    ? _runtimeGradient.Evaluate(curveValue)
                    : Color.LerpUnclamped(startColor, endColor, curveValue);
                
                _currentTransitionTime += Time.deltaTime;
                yield return null;
            }

            _graphic.color = endColor;
            _currentTransitionTime = 0;
            _colorCoroutine = null;
        }
#endif

        public void SetHoverColor(Color newHoverColor)
        {
            _hoverColor = newHoverColor;
        }

        public void SetOriginalColor(Color newOriginalColor)
        {
            _originalColor = newOriginalColor;
            if (!IsHovering())
            {
                _graphic.color = _originalColor;
            }
        }

        public void UpdateGradient(Gradient newGradient)
        {
            _runtimeGradient = new Gradient();
            _runtimeGradient.SetKeys(newGradient.colorKeys, newGradient.alphaKeys);
            _hoverColor = _runtimeGradient.Evaluate(1f);
        }

        public bool IsHovering()
        {
#if UNITASK
            return _transitionCts != null || _graphic.color != _originalColor;
#else
            return _colorCoroutine != null || _graphic.color != _originalColor;
#endif
        }

        public void Lock(bool state)
        {
            _isLocked = state;
        }

        [ContextMenu("Preview Hover Color")]
        private void PreviewHoverColor()
        {
            if (Application.isPlaying)
                ChangeToHoverColor(true);
        }

        [ContextMenu("Preview Original Color")]
        private void PreviewOriginalColor()
        {
            if (Application.isPlaying)
                ChangeToOriginalColor(true);
        }
    }
}