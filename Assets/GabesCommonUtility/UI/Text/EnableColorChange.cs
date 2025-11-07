using System.Threading;
using GabesCommonUtility.Extensions;
using TMPro;
using UnityEngine;

namespace GabesCommonUtility.UI.Text
{
    public class EnableColorChange : MonoBehaviour
    {
        [SerializeField] private float duration = 0.4f;

        [SerializeField] private Gradient gradient;

        private TextMeshProUGUI _tmp;
        private CancellationTokenSource _cts;

        private void OnEnable()
        {
            _tmp ??= GetComponent<TextMeshProUGUI>();

            _cts = new CancellationTokenSource();
            _ = _tmp.ChangeColorTransition(gradient, duration, _cts.Token);
        }

        private void OnDisable()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _tmp.color = gradient.Evaluate(0);
            _cts = null;
        }
    }
}