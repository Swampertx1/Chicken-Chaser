using System;
using UnityEngine;

namespace GabesCommonUtility
{
    [SelectionBase, RequireComponent(typeof(Canvas))]
    public abstract class SettingsMenuBase : MonoBehaviour
    {
        private Canvas _canvas;
        protected bool _isDirty;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
        }

        public abstract void Load();
        public abstract void Save();
        public abstract void ResetSettings();
        public bool IsDirty => _isDirty;

    }
}
