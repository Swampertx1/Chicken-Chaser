using System.Collections;
using Unity.Netcode;
using UnityEngine;
using Utilities;

namespace Characters
{
    public abstract class Human : NetworkBehaviour, IHuman
    {
        [SerializeField] protected HumanController controller;
        protected Animator Animator => controller.Animator;
        protected AudioSource AudioSource => controller.AudioSource;
        protected Rigidbody Rigidbody => controller.Rigidbody;

        public bool IsRolling { get; private set; }
        private Coroutine _rollCoroutine;

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

        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Owner)]
        protected void TryRoll_Rpc(Vector3 direction)
        {
            if (IsRolling) return;
            _rollCoroutine = StartCoroutine(RollCoroutine(direction));
        }

        private IEnumerator RollCoroutine(Vector3 direction)
        {
            IsRolling = true;
            Animator.SetTrigger(StaticUtilities.CaptureAnimID);

            float elapsed = 0f;
            float duration = controller.Stats.RollDuration;

            while (elapsed < duration)
            {
                if (!Physics.Raycast(transform.position + Vector3.up, direction, 1f, StaticUtilities.GroundLayers))
                {
                    if (Physics.Raycast(transform.position, Vector3.down, out var ground, 1f, StaticUtilities.GroundLayers))
                    {
                        transform.Translate(
                            Vector3.ProjectOnPlane(direction, ground.normal) * (controller.Stats.RollSpeed * Time.deltaTime),
                            Space.World
                        );
                    }
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            IsRolling = false;
            EndRoll();
        }

        protected void StopRoll()
        {
            if (_rollCoroutine != null)
            {
                StopCoroutine(_rollCoroutine);
                _rollCoroutine = null;
            }
            IsRolling = false;
        }
    }
}