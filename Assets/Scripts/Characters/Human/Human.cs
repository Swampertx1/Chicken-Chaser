using UnityEngine;
using Utilities;

namespace Characters
{
    public abstract class Human : MonoBehaviour, IHuman
    {
        [SerializeField] protected HumanController controller;
        protected Animator Animator  => controller.Animator;
        protected AudioSource AudioSource  => controller.AudioSource;
        protected Rigidbody Rigidbody  => controller.Rigidbody;

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
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * controller.Stats.BaseMoveSpeed);
        }
    }
}