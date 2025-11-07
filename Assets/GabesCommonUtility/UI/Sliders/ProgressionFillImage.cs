using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace GabesCommonUtility.UI.Sliders
{
    public class ProgressionFillImage : Image
    {
        [SerializeField] private float delay = 0.5f;
        [SerializeField] private float speed = 1f;
        [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        private float _targetFillAmount;
        private float _displayFillAmount;
        private Coroutine _animationCoroutine;

        protected override void Awake()
        {
            base.Awake();
            type = Type.Filled;
            _displayFillAmount = fillAmount;
            _targetFillAmount = fillAmount;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
            }
        }

        public float fillAmountAnimated
        {
            get => _displayFillAmount;
            set
            {
                float clampedValue = Mathf.Clamp01(value);
                
                if (Mathf.Approximately(_targetFillAmount, clampedValue)) return;
                
                _targetFillAmount = clampedValue;
                
                // Cancel existing animation and start new one
                if (_animationCoroutine != null)
                {
                    StopCoroutine(_animationCoroutine);
                }
                
                _animationCoroutine = StartCoroutine(AnimateToTarget());
            }
        }

        private IEnumerator AnimateToTarget()
        {
            // Wait for delay
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            // Animate to target
            float startValue = _displayFillAmount;
            float distance = Mathf.Abs(_targetFillAmount - startValue);
            float duration = distance / speed;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float curveValue = animationCurve.Evaluate(t);
                
                _displayFillAmount = Mathf.Lerp(startValue, _targetFillAmount, curveValue);
                fillAmount = _displayFillAmount;

                yield return null;
            }

            // Ensure we end exactly at target
            _displayFillAmount = _targetFillAmount;
            fillAmount = _displayFillAmount;
        }

        public void SetFillAmountImmediate(float newFillAmount)
        {
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
                _animationCoroutine = null;
            }

            _targetFillAmount = Mathf.Clamp01(newFillAmount);
            _displayFillAmount = _targetFillAmount;
            fillAmount = _displayFillAmount;
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            if (!Application.isPlaying)
            {
                type = Type.Filled;
            }
        }

        public float GetActualValue => _targetFillAmount;

        public float Delay
        {
            get => delay;
            set => delay = Mathf.Max(0f, value);
        }

        public float Speed
        {
            get => speed;
            set => speed = Mathf.Max(0.001f, value);
        }

        public AnimationCurve AnimationCurve
        {
            get => animationCurve;
            set => animationCurve = value ?? AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        }
    }
}