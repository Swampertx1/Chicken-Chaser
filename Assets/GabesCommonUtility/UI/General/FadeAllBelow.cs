using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GabesCommonUtility.UI.General
{
   [DefaultExecutionOrder(-250)]
    public class FadeAllBelow : MonoBehaviour
    {
       public UnityEvent onFaded;
       public UnityEvent onUnFaded;

   
       [Header("Transition")]
       [SerializeField] private float transitionDuration = 0.5f;
       [SerializeField] private AnimationCurve transitionCurve;
       [SerializeField] private bool isVisible = true;
       [SerializeField] private bool shouldDisableObject;
       
       private Graphic[] _graphics;
       private float[] _cachedColors;
       
       public bool IsVisible => isVisible;
       public bool IsFading { get; private set; }
      
       private float _currentTransitionTime;
       private Coroutine _actionRoutine;

       private void OnEnable()
       {
          _graphics = GetComponentsInChildren<Graphic>();
          
          _cachedColors = new float[_graphics.Length];
                
          for(int i = 0; i < _graphics.Length; i++)
             _cachedColors[i] = _graphics[i].color.a;
       }

       [ContextMenu("Toggle")]
             public void Toggle()
             {
                if(isVisible) FadeAway();
                else BecomeVisible();
             }
       
             public void SetState(bool state)
             {
                if(!state) FadeAway();
                else BecomeVisible();
             }
       
             public void BecomeVisible()
             {
                if (_actionRoutine != null)
                {
                   _currentTransitionTime = transitionDuration - _currentTransitionTime;
                   StopCoroutine(_actionRoutine);
                }
                _actionRoutine = StartCoroutine(FadeInTranition());
             }
       
             public void FadeAway()
             {
                Debug.Log("fading away object: " + name, gameObject);
                if (_actionRoutine != null)
                {
                   _currentTransitionTime = transitionDuration - _currentTransitionTime;
                   StopCoroutine(_actionRoutine);
                }
                
                _actionRoutine = StartCoroutine(FadeAwayTransition());
             }

             private IEnumerator FadeInTranition()
             {
                onUnFaded?.Invoke();
                IsFading = true;
                isVisible = true;

                if (shouldDisableObject)
                {
                   foreach (Graphic g in _graphics)
                   {
                      g.gameObject.SetActive(true);
                   }
                }

                while (_currentTransitionTime < transitionDuration)
                {
                   float t = transitionCurve.Evaluate(_currentTransitionTime / transitionDuration);
                   for (int index = 0; index < _graphics.Length; index++)
                   {
                      Graphic g = _graphics[index];
                      if (!g.enabled) continue;
                      var color = g.color;
                      color.a = Mathf.Lerp(0, _cachedColors[index], t);
                      g.color = color;
                   }
                   _currentTransitionTime += Time.deltaTime;
                   yield return null;
                }
                
                for (int index = 0; index < _graphics.Length; index++)
                {
                   Graphic g = _graphics[index];
                   if (!g.enabled) continue;
                   var color = g.color;
                   color.a = _cachedColors[index];
                   g.color = color;
                }
                IsFading = false;
                
             }

             private IEnumerator FadeAwayTransition()
             {

                IsFading = true;
                
                while (_currentTransitionTime < transitionDuration)
                {
                   float t = transitionCurve.Evaluate(_currentTransitionTime / transitionDuration);
                   for (int index = 0; index < _graphics.Length; index++)
                   {
                      Graphic g = _graphics[index];
                      var color = g.color;
                      color.a = Mathf.Lerp(_cachedColors[index], 0, t);
                      g.color = color;
                   }
                   _currentTransitionTime += Time.deltaTime;
                   yield return null;
                }
                
                for (int index = 0; index < _graphics.Length; index++)
                {
                   Graphic g = _graphics[index];
                   var color = g.color;
                   color.a = 0;
                   g.color = color;
                }
                
                if (shouldDisableObject)
                {
                   foreach (Graphic g in _graphics)
                   {
                      g.gameObject.SetActive(false);
                   }
                }
                
                isVisible = false;
                IsFading = false;
                onFaded?.Invoke();
             }
             
             
    }
}
