using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Utilities
{
    public static class StaticUtilities
    {
       
        /// </summary>
        /// <typeparam name="T">The type of the array elements.</typeparam>
        /// <param name="array">The array to shuffle.</param>
        public static void Shuffle<T>(this T[] array)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            int n = array.Length;
            for (int i = n - 1; i > 0; i--)
            {
                // UnityEngine.Random.Range is min inclusive and max exclusive.
                int j = Random.Range(0, i + 1);

                // Swap elements
                (array[i], array[j]) = (array[j], array[i]);
            }
        }
        public static readonly int WallLayer = 1 << LayerMask.NameToLayer("Default");
        
        public static readonly int WaterLayer = 1 << LayerMask.NameToLayer("Water");
        public static readonly int BushLayer = 1 << LayerMask.NameToLayer("HidingZone");
        
        
        public static readonly int HumanLayer = 1 << LayerMask.NameToLayer("Human");
        public static readonly int PlayerLayer = 1 << LayerMask.NameToLayer("Player");
        public static readonly int ChickenAiLayer = 1 << LayerMask.NameToLayer("AiChicken");
        
        //Describes layers for detection
        public static readonly int EverythingButChicken = ~(PlayerLayer | HumanLayer | ChickenAiLayer);
        
        //Describes the layers we cannot see/pass through
        public static readonly int VisibilityLayer =  WallLayer | HumanLayer;
        
        //What layers are we looking for
        public static readonly int DetectableLayer = PlayerLayer | ChickenAiLayer;
        
        //Describes the layers that will count as grounded if we are in or touching
        public static readonly int GroundLayers = WallLayer | WaterLayer | BushLayer;
    
    
        //Animations
        public static readonly int MoveSpeedAnimID = Animator.StringToHash("moveSpeed");
        public static readonly int CluckAnimID = Animator.StringToHash("IsDancing");
        public static readonly int JumpAnimID = Animator.StringToHash("Jump");
        public static readonly int DashAnimID = Animator.StringToHash("Dash");
        public static readonly int IsEatingAnimID = Animator.StringToHash("IsEating");
        
        public static readonly int IsGroundedAnimID = Animator.StringToHash("isGrounded");
        public static readonly int IsSearchingAnimID = Animator.StringToHash("isSearching");
        public static readonly int CaptureAnimID = Animator.StringToHash("Dive");
        public static readonly int BeginCaptureAnimID = Animator.StringToHash("HasChicken");


        public static readonly int FillMatID = Shader.PropertyToID("_Fill");

        public static IEnumerator AnimateLocalScale(this Transform target, Vector3 start, Vector3 end, float duration,
            AnimationCurve curve)
        {float curTime = 0;
            while (curTime < duration)
            {
                
                float percent = curTime / duration;
                curTime += Time.deltaTime;
                target.localScale = Vector3.LerpUnclamped(start, end, curve.Evaluate(percent));
          
                yield return null;
            }
          target.localScale = Vector3.LerpUnclamped(start, end, curve.Evaluate(1));
          
        }
        
        //The "this" keyword will allow us to say source.TransitionSound anywhere.
        /// <summary>
        /// This is a coroutine function that transitions audio. It is not async because web support
        /// </summary>
        /// <param name="source"></param>
        /// <param name="nextClip"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public static IEnumerator TransitionSound(this AudioSource source, AudioClip nextClip, float duration)
        {
            float curTime = 0;
            float volume = source.volume;
            bool hasChanged = false;
            while (curTime < duration)
            {
                curTime += Time.deltaTime;
                float percent = curTime / duration;
                
                //Make a parabolic function, in which when percent is 0, currentVolume is volume, and when percent is 0.5, volume is 0, and when percent is currentVolume is volume
                var currentVolume = 4 * volume * (percent - 0.5f) * (percent - 0.5f);

                if (!hasChanged && percent > 0.5f)
                {
                    hasChanged = true;
                    source.clip = nextClip;
                    source.Play();
                }
                source.volume = currentVolume;

                yield return null;
            }
            source.volume = volume;
        }
    }
}
