using UnityEngine;
using System.Threading;
using TMPro;

#if UNITASK
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

namespace GabesCommonUtility.Extensions
{
    // ===== AUDIO SOURCE EXTENSIONS =====
    public static class AudioSourceExtensions
    {
#if UNITASK
        /// <summary>
        /// Fades an AudioSource's volume from one value to another over a specified duration.
        /// </summary>
        public static async UniTask FadeVolume(this AudioSource audioSource, float from, float to, float duration,
            CancellationToken cancellationToken = default)
        {
#if UNITY_EDITOR
            Debug.Assert(audioSource != null, "AudioSource is null in FadeVolume");
#endif

            float elapsed = 0f;
            audioSource.volume = from;

            while (elapsed < duration)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying) return;
#endif

                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                audioSource.volume = Mathf.Lerp(from, to, t);

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            audioSource.volume = to;
        }

        /// <summary>
        /// Fades an AudioSource's volume from one value to another over a specified duration, following an animation curve.
        /// </summary>
        public static async UniTask FadeVolume(this AudioSource audioSource, float from, float to, float duration,
            AnimationCurve curve, CancellationToken cancellationToken = default)
        {
#if UNITY_EDITOR
            Debug.Assert(audioSource != null, "AudioSource is null in FadeVolume");
            Debug.Assert(curve != null, "AnimationCurve is null in FadeVolume");
#endif

            float elapsed = 0f;
            audioSource.volume = from;

            while (elapsed < duration)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying) return;
#endif

                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float curveValue = curve.Evaluate(t);
                audioSource.volume = Mathf.Lerp(from, to, curveValue);

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            audioSource.volume = to;
        }

        /// <summary>
        /// Fades the current AudioSource volume to 0, changes the clip, and fades back to the original volume.
        /// </summary>
        public static async UniTask FadeIntoTrack(this AudioSource audioSource, AudioClip newClip, float duration,
            CancellationToken cancellationToken = default)
        {
#if UNITY_EDITOR
            Debug.Assert(audioSource != null, "AudioSource is null in FadeIntoTrack");
            Debug.Assert(newClip != null, "AudioClip is null in FadeIntoTrack");
#endif

            float originalVolume = audioSource.volume;
            float halfDuration = duration * 0.5f;

            // Fade out
            await audioSource.FadeVolume(originalVolume, 0f, halfDuration, cancellationToken);

#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif

            // Change track
            audioSource.clip = newClip;
            audioSource.Play();

            // Fade in
            await audioSource.FadeVolume(0f, originalVolume, halfDuration, cancellationToken);
        }

        /// <summary>
        /// Fades the current AudioSource volume to 0, changes the clip, and fades back to the original volume, following an animation curve.
        /// The curve is applied twice: once for fade out and once for fade in.
        /// </summary>
        public static async UniTask FadeIntoTrack(this AudioSource audioSource, AudioClip newClip, float duration,
            AnimationCurve curve, CancellationToken cancellationToken = default)
        {
#if UNITY_EDITOR
            Debug.Assert(audioSource != null, "AudioSource is null in FadeIntoTrack");
            Debug.Assert(newClip != null, "AudioClip is null in FadeIntoTrack");
            Debug.Assert(curve != null, "AnimationCurve is null in FadeIntoTrack");
#endif

            float originalVolume = audioSource.volume;
            float halfDuration = duration * 0.5f;

            // Fade out
            await audioSource.FadeVolume(originalVolume, 0f, halfDuration, curve, cancellationToken);

#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif

            // Change track
            audioSource.clip = newClip;
            audioSource.Play();

            // Fade in
            await audioSource.FadeVolume(0f, originalVolume, halfDuration, curve, cancellationToken);
        }
#else
        /// <summary>
        /// Fades an AudioSource's volume from one value to another over a specified duration.
        /// </summary>
        public static async Task FadeVolume(this AudioSource audioSource, float from, float to, float duration,
            CancellationToken cancellationToken = default)
        {
#if UNITY_EDITOR
            Debug.Assert(audioSource != null, "AudioSource is null in FadeVolume");
#endif

            float elapsed = 0f;
            audioSource.volume = from;

            while (elapsed < duration)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying) return;
#endif

                cancellationToken.ThrowIfCancellationRequested();

                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                audioSource.volume = Mathf.Lerp(from, to, t);

                await Task.Yield();
            }

            audioSource.volume = to;
        }

        /// <summary>
        /// Fades an AudioSource's volume from one value to another over a specified duration, following an animation curve.
        /// </summary>
        public static async Task FadeVolume(this AudioSource audioSource, float from, float to, float duration,
            AnimationCurve curve, CancellationToken cancellationToken = default)
        {
#if UNITY_EDITOR
            Debug.Assert(audioSource != null, "AudioSource is null in FadeVolume");
            Debug.Assert(curve != null, "AnimationCurve is null in FadeVolume");
#endif

            float elapsed = 0f;
            audioSource.volume = from;

            while (elapsed < duration)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying) return;
#endif

                cancellationToken.ThrowIfCancellationRequested();

                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float curveValue = curve.Evaluate(t);
                audioSource.volume = Mathf.Lerp(from, to, curveValue);

                await Task.Yield();
            }

            audioSource.volume = to;
        }

        /// <summary>
        /// Fades the current AudioSource volume to 0, changes the clip, and fades back to the original volume.
        /// </summary>
        public static async Task FadeIntoTrack(this AudioSource audioSource, AudioClip newClip, float duration,
            CancellationToken cancellationToken = default)
        {
#if UNITY_EDITOR
            Debug.Assert(audioSource != null, "AudioSource is null in FadeIntoTrack");
            Debug.Assert(newClip != null, "AudioClip is null in FadeIntoTrack");
#endif

            float originalVolume = audioSource.volume;
            float halfDuration = duration * 0.5f;

            // Fade out
            await audioSource.FadeVolume(originalVolume, 0f, halfDuration, cancellationToken);

#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif

            // Change track
            audioSource.clip = newClip;
            audioSource.Play();

            // Fade in
            await audioSource.FadeVolume(0f, originalVolume, halfDuration, cancellationToken);
        }

        /// <summary>
        /// Fades the current AudioSource volume to 0, changes the clip, and fades back to the original volume, following an animation curve.
        /// The curve is applied twice: once for fade out and once for fade in.
        /// </summary>
        public static async Task FadeIntoTrack(this AudioSource audioSource, AudioClip newClip, float duration,
            AnimationCurve curve, CancellationToken cancellationToken = default)
        {
#if UNITY_EDITOR
            Debug.Assert(audioSource != null, "AudioSource is null in FadeIntoTrack");
            Debug.Assert(newClip != null, "AudioClip is null in FadeIntoTrack");
            Debug.Assert(curve != null, "AnimationCurve is null in FadeIntoTrack");
#endif

            float originalVolume = audioSource.volume;
            float halfDuration = duration * 0.5f;

            // Fade out
            await audioSource.FadeVolume(originalVolume, 0f, halfDuration, curve, cancellationToken);

#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif

            // Change track
            audioSource.clip = newClip;
            audioSource.Play();

            // Fade in
            await audioSource.FadeVolume(0f, originalVolume, halfDuration, curve, cancellationToken);
        }
#endif

        /// <summary>
        /// Plays an AudioClip once with a random pitch between minRandomPitch and maxRandomPitch.
        /// </summary>
        public static void PlayOneShot(this AudioSource audioSource, AudioClip clip, float volume,
            float minRandomPitch, float maxRandomPitch)
        {
#if UNITY_EDITOR
            Debug.Assert(audioSource != null, "AudioSource is null in PlayOneShot");
            Debug.Assert(clip != null, "AudioClip is null in PlayOneShot");
#endif

            float originalPitch = audioSource.pitch;
            audioSource.pitch = Random.Range(minRandomPitch, maxRandomPitch);
            audioSource.PlayOneShot(clip, volume);
            audioSource.pitch = originalPitch;
        }
    }

    // ===== GRAPHIC EXTENSIONS =====
    public static class GraphicsExtensions
    {
#if UNITASK
        /// <summary>
        /// Fades a Graphic's color from one value to another over a specified duration.
        /// </summary>
        public static async UniTask ChangeColorTransition(this UnityEngine.UI.Graphic graphic, Color from, Color to,
            float duration, CancellationToken cancellationToken = default)
        {
#if UNITY_EDITOR
            Debug.Assert(graphic != null, "Graphic is null in ChangeColor");
#endif

            float elapsed = 0f;
            graphic.color = from;

            while (elapsed < duration)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying) return;
#endif

                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                graphic.color = Color.Lerp(from, to, t);

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            graphic.color = to;
        }

        /// <summary>
        /// Fades a Graphic's color from one value to another over a specified duration, following an animation curve.
        /// </summary>
        public static async UniTask ChangeColorTransition(this UnityEngine.UI.Graphic graphic, Color from, Color to,
            float duration, AnimationCurve curve, CancellationToken cancellationToken = default)
        {
#if UNITY_EDITOR
            Debug.Assert(graphic != null, "Graphic is null in ChangeColor");
            Debug.Assert(curve != null, "AnimationCurve is null in ChangeColor");
#endif

            float elapsed = 0f;
            graphic.color = from;

            while (elapsed < duration)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying) return;
#endif

                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float curveValue = curve.Evaluate(t);
                graphic.color = Color.Lerp(from, to, curveValue);

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            graphic.color = to;
        }

        /// <summary>
        /// Fades a Graphic's color through a gradient over a specified duration.
        /// </summary>
        public static async UniTask ChangeColorTransition(this UnityEngine.UI.Graphic graphic, Gradient gradient,
            float duration, CancellationToken cancellationToken = default)
        {
#if UNITY_EDITOR
            Debug.Assert(graphic != null, "Graphic is null in ChangeColor");
            Debug.Assert(gradient != null, "Gradient is null in ChangeColor");
#endif

            float elapsed = 0f;

            while (elapsed < duration)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying) return;
#endif

                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                graphic.color = gradient.Evaluate(t);

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            graphic.color = gradient.Evaluate(1f);
        }
#else
        /// <summary>
        /// Fades a Graphic's color from one value to another over a specified duration.
        /// </summary>
        public static async Task ChangeColorTransition(this UnityEngine.UI.Graphic graphic, Color from, Color to,
            float duration, CancellationToken cancellationToken = default)
        {
#if UNITY_EDITOR
            Debug.Assert(graphic != null, "Graphic is null in ChangeColor");
#endif

            float elapsed = 0f;
            graphic.color = from;

            while (elapsed < duration)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying) return;
#endif

                cancellationToken.ThrowIfCancellationRequested();

                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                graphic.color = Color.Lerp(from, to, t);

                await Task.Yield();
            }

            graphic.color = to;
        }

        /// <summary>
        /// Fades a Graphic's color from one value to another over a specified duration, following an animation curve.
        /// </summary>
        public static async Task ChangeColorTransition(this UnityEngine.UI.Graphic graphic, Color from, Color to,
            float duration, AnimationCurve curve, CancellationToken cancellationToken = default)
        {
#if UNITY_EDITOR
            Debug.Assert(graphic != null, "Graphic is null in ChangeColor");
            Debug.Assert(curve != null, "AnimationCurve is null in ChangeColor");
#endif

            float elapsed = 0f;
            graphic.color = from;

            while (elapsed < duration)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying) return;
#endif

                cancellationToken.ThrowIfCancellationRequested();

                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float curveValue = curve.Evaluate(t);
                graphic.color = Color.Lerp(from, to, curveValue);

                await Task.Yield();
            }

            graphic.color = to;
        }

        /// <summary>
        /// Fades a Graphic's color through a gradient over a specified duration.
        /// </summary>
        public static async Task ChangeColorTransition(this UnityEngine.UI.Graphic graphic, Gradient gradient,
            float duration, CancellationToken cancellationToken = default)
        {
#if UNITY_EDITOR
            Debug.Assert(graphic != null, "Graphic is null in ChangeColor");
            Debug.Assert(gradient != null, "Gradient is null in ChangeColor");
#endif

            float elapsed = 0f;

            while (elapsed < duration)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying) return;
#endif

                cancellationToken.ThrowIfCancellationRequested();

                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                graphic.color = gradient.Evaluate(t);

                await Task.Yield();
            }

            graphic.color = gradient.Evaluate(1f);
        }
#endif
    }

    // ===== TEXT MESH PRO EXTENSIONS =====
    public static class TextMeshProExtensions
    {
#if UNITASK
        /// <summary>
        /// Smoothly transitions a TextMeshProUGUI font size from its current value to a target value over a duration.
        /// </summary>
        public static async UniTask ChangeFontSizeTransition(
            this TMP_Text tmp,
            float to,
            float duration,
            CancellationToken cancellationToken = default)
        {
#if UNITY_EDITOR
            Debug.Assert(tmp != null, "TextMeshProUGUI is null in ChangeFontSizeTransition");
#endif
            float from = tmp.fontSize;
            float elapsed = 0f;

            while (elapsed < duration)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying) return;
#endif
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                tmp.fontSize = Mathf.Lerp(from, to, t);

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            tmp.fontSize = to;
        }

        /// <summary>
        /// Smoothly transitions a TextMeshProUGUI font size from its current value to a target value over a duration, using an AnimationCurve.
        /// </summary>
        public static async UniTask ChangeFontSizeTransition(
            this TMP_Text tmp,
            float to,
            float duration,
            AnimationCurve curve,
            CancellationToken cancellationToken = default)
        {
#if UNITY_EDITOR
            Debug.Assert(tmp != null, "TMP_Text is null in ChangeFontSizeTransition (curve overload)");
            Debug.Assert(curve != null, "AnimationCurve is null in ChangeFontSizeTransition (curve overload)");
#endif
            float from = tmp.fontSize;
            float elapsed = 0f;

            while (elapsed < duration)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying) return;
#endif
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float curvedT = curve.Evaluate(t);
                tmp.fontSize = Mathf.Lerp(from, to, curvedT);

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            tmp.fontSize = Mathf.Lerp(from, to, curve.Evaluate(1));
        }
#else
        /// <summary>
        /// Smoothly transitions a TextMeshProUGUI font size from its current value to a target value over a duration.
        /// </summary>
        public static async Task ChangeFontSizeTransition(
            this TMP_Text tmp,
            float to,
            float duration,
            CancellationToken cancellationToken = default)
        {
#if UNITY_EDITOR
            Debug.Assert(tmp != null, "TextMeshProUGUI is null in ChangeFontSizeTransition");
#endif
            float from = tmp.fontSize;
            float elapsed = 0f;

            while (elapsed < duration)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying) return;
#endif

                cancellationToken.ThrowIfCancellationRequested();

                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                tmp.fontSize = Mathf.Lerp(from, to, t);

                await Task.Yield();
            }

            tmp.fontSize = to;
        }

        /// <summary>
        /// Smoothly transitions a TextMeshProUGUI font size from its current value to a target value over a duration, using an AnimationCurve.
        /// </summary>
        public static async Task ChangeFontSizeTransition(
            this TMP_Text tmp,
            float to,
            float duration,
            AnimationCurve curve,
            CancellationToken cancellationToken = default)
        {
#if UNITY_EDITOR
            Debug.Assert(tmp != null, "TMP_Text is null in ChangeFontSizeTransition (curve overload)");
            Debug.Assert(curve != null, "AnimationCurve is null in ChangeFontSizeTransition (curve overload)");
#endif
            float from = tmp.fontSize;
            float elapsed = 0f;

            while (elapsed < duration)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying) return;
#endif

                cancellationToken.ThrowIfCancellationRequested();

                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float curvedT = curve.Evaluate(t);
                tmp.fontSize = Mathf.Lerp(from, to, curvedT);

                await Task.Yield();
            }

            tmp.fontSize = Mathf.Lerp(from, to, curve.Evaluate(1));
        }
#endif
    }
}