#if UNITY_EDITOR && UNITY_ANIMATION_RIGGING
using UnityEngine;
using UnityEngine.Animations.Rigging;
namespace GabesCommonUtility.TempEditor
{
    [ExecuteInEditMode]
    public class RigUpdater : MonoBehaviour
    {
        private RigBuilder _rigBuilder;
        
        void Start()
        {
            _rigBuilder = GetComponent<RigBuilder>();

            if(Application.isPlaying) Destroy(this);
        }
        
        [ContextMenu("Rebuild")]
        void Update()
        {
            _rigBuilder.Clear();
            _rigBuilder.Build();
        }
    }
}
#endif
