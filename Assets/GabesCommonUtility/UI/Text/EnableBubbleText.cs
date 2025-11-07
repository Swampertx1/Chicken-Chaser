using System.Threading;
using GabesCommonUtility.Extensions;
using TMPro;
using UnityEngine;

namespace GabesCommonUtility.UI.Text
{
    public class EnableBubbleText : MonoBehaviour
    {
        [SerializeField] private float duration = 0.4f;
        [SerializeField] private int minTextSize = 120;
        [SerializeField] private int maxTextSize = 180;
        [SerializeField] private AnimationCurve textScaleCurve;

        private TMP_Text _tmp;
        private CancellationTokenSource _cts;

        private void OnEnable()
        {
            _tmp ??= GetComponent<TMP_Text>();

            _cts = new CancellationTokenSource();

            // Start bubble animation using the new UniTask helper
            _ = _tmp.ChangeFontSizeTransition(
                maxTextSize,
                duration,
                textScaleCurve,
                _cts.Token
            );
        }

        private void OnDisable()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;

            // Optionally reset to min size when disabled
            _tmp.fontSize = minTextSize;
        }
    }
}