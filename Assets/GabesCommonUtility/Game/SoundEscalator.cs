using System.Threading.Tasks;
using UnityEngine;

namespace GabesCommonUtility.Game
{
 
    public class SoundEscalator : MonoBehaviour
    {
        private enum EEscalationType
        {
            Unconstrained,
            Constrained,
            Wrap,
            PingPong,
            
        }
        
        [SerializeField] private AudioClip clip;
        [SerializeField] private AudioSource targetSource;

        [Header("Settings")] 
        [SerializeField] private EEscalationType escalationType;
        [SerializeField] private int maxNoteIncrease = 8;
        [SerializeField] private int startingNoteShift;
        [SerializeField] private int semiTones = 1;
        
        private int _counter;
        private int _increment = 1;
        
        public void Play(bool autoChange = true)
        {
            float pitch = GetPitch(_counter + startingNoteShift);
            targetSource.pitch = pitch;
            targetSource.PlayOneShot(clip);

            if (!autoChange) return;
           
           _counter += _increment;
           ApplyMode();
        }
        
        public void Play(int index)
        {
            float pitch = GetPitch(index);
            targetSource.pitch = pitch;
            targetSource.PlayOneShot(clip);
        }

        public void PlayManual(AudioClip targetClip, int index)
        {
            float pitch = GetPitch(index);
            targetSource.pitch = pitch;
            targetSource.PlayOneShot(targetClip);
        }


        public void Increment()
        {
            _counter += 1;
            ApplyMode();
        }

        public void Decrement()
        {
            _counter -= 1;
            ApplyMode();
        }

        private void ApplyMode()
        {
            if (escalationType is EEscalationType.Unconstrained) return;   

            if (_counter >= maxNoteIncrease)
            {
                if (escalationType is EEscalationType.Wrap) _counter = 0;
                else
                {
                    if(escalationType is EEscalationType.PingPong) _increment = -1;
                    _counter = maxNoteIncrease - 1;
                }
            }
            else if (_counter < 0)
            {
                
                if (escalationType is EEscalationType.Wrap) _counter = maxNoteIncrease -1;
                else
                {
                    if(escalationType is EEscalationType.PingPong) _increment = 1;
                    _counter = 0;
                }
            }
        }
        
        private float GetPitch(int note) => Mathf.Pow(2f, note * semiTones / 12f);


        public void ResetProgression()
        {
            _counter = 0;
            _increment = 1;
        }

        public void SetClip(AudioClip newClip)
        {
            clip = newClip;
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private int debugDelay = 100;
        [ContextMenu("PlayTest")]
        private async void PlayTest()
        {
            ResetProgression();
            for (int i = 0; i < maxNoteIncrease * 2; i++)
            {
                 Play();
                await Task.Delay(debugDelay);
            }
        }
        
        #endif
        
    }
}
