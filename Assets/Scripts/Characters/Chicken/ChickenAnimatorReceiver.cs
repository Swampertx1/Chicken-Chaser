using System;
using UnityEngine;

namespace Characters
{
    public class ChickenAnimatorReceiver : MonoBehaviour
    {
      [SerializeField] private ParticleSystem particles;
      private Grounding grounding;
        public Action<float> OnLandEffect = null;
        private float time;
    
        //Let's also check if we're inside a volume maker,
        private void Awake()
        {
            grounding = GetComponentInParent<Grounding>();
            grounding.OnGroundStateChange += GroundingOnOnGroundStateChange;
        }

        private void GroundingOnOnGroundStateChange(bool obj)
        {
            if (obj)
            {
                PlayParticle(Time.time - time);
            }
            else
            {
                time = Time.time;
            }
        }

        //Magically called via animator
        private void LandEffect(float force)
        {
            OnLandEffect?.Invoke(force);
            PlayParticle(force);
        }

        private void PlayParticle(float force)
        {
            var main = particles.main;
            main.startSpeed = new ParticleSystem.MinMaxCurve(force, force * 3);
            var module = particles.emission;
            module.burstCount = (int)Mathf.Max(5, force * 2);
            
            particles.Play();
        }
    }
}
