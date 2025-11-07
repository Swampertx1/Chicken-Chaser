using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GabesCommonUtility.UI.Hover
{
   // [RequireComponent(typeof(RectTransform))]
    public class UIHoverMove : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Vector2 localOffset = Vector2.zero;
        [SerializeField] private float transitionDuration;
        [SerializeField] private AnimationCurve transitionCurve;

        private Vector2 _originalLocation;
        private Vector2 _newLocation;
        private RectTransform  _rectTransform;
        private Coroutine _hoverCoroutine;
        private Selectable _selectable;


        private float _currentTransitionTime;
        private void Awake()
        {
            _selectable = GetComponent<Selectable>();
            _rectTransform = transform as RectTransform;
            if (_rectTransform == null) return;//throw new UnityException("Missing component of RectTransform");
            _originalLocation =  _rectTransform.anchoredPosition;
            _newLocation = _originalLocation + localOffset;
        
        }


        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_selectable == null || _selectable.IsInteractable()) return;
            
            Debug.Log("OnPointerEnter");
            if (_hoverCoroutine != null)
            {
                _currentTransitionTime = transitionDuration - _currentTransitionTime;
                StopCoroutine(_hoverCoroutine);
            }
            _hoverCoroutine = StartCoroutine(Transition( _rectTransform.anchoredPosition, _newLocation));
        }

  
        public void OnPointerExit(PointerEventData eventData)
        {
            
            if (_hoverCoroutine != null)
            {
                _currentTransitionTime = transitionDuration - _currentTransitionTime;
                StopCoroutine(_hoverCoroutine);
            }
            _hoverCoroutine = StartCoroutine(Transition( _rectTransform.anchoredPosition, _originalLocation));
        }
        
        private IEnumerator Transition(Vector3 start, Vector3 end)
        {
            while (_currentTransitionTime < transitionDuration)
            {
                _rectTransform.anchoredPosition = Vector2.LerpUnclamped(start, end, transitionCurve.Evaluate(_currentTransitionTime / transitionDuration));
                _currentTransitionTime += Time.deltaTime;
                yield return null;
            }
            _currentTransitionTime = 0;
            _hoverCoroutine = null;
        }

    }
}
