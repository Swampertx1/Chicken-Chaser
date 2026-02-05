using UnityEngine;
using Utilities;

namespace Characters
{
    public abstract class Human : MonoBehaviour, IHuman
    {
        protected HumanController _controller;
        protected Animator Animator  => _controller.Animator;
        protected AudioSource AudioSource  => _controller.AudioSource;

        public abstract float Speed { get; }


        public abstract void OnControllerEnabled(HumanController controller);
        public abstract void OnControllerDisabled();
        public abstract void EndRoll();

        protected virtual void UpdateAnimation()
        {
            Animator.SetFloat(StaticUtilities.MoveSpeedAnimID, Speed);
        }

        protected void FaceTarget(Vector3 suggestedForward)
        {
            if (suggestedForward == Vector3.zero) return;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(suggestedForward.x, 0, suggestedForward.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * _controller.Stats.BaseMoveSpeed);
        }
    }
}