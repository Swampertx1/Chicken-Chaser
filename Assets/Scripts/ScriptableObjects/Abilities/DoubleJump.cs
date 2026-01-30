using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "DoubleJump", menuName = "Abilities/DoubleJump")]
public class DoubleJump : AbilityBase
{
    int numJumps = 0;
    [SerializeField] int numExtraJumps = 1;
    protected override IEnumerator Activate()
    {
        Rigidbody rb = _chicken.Rb;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * _chicken.ChickenStats.JumpForce, ForceMode.Impulse);
        _chicken.PlayParticleRPC(Particles[0].name);
            _chicken.PlaysoundRPC(Sound[0].name);
        
        numJumps += 1;
        if (numJumps >= numExtraJumps)
        {
            yield return new WaitUntil(() => _chicken.Grounding.IsGrounded);
            numJumps = 0;
        }
    }

    public override bool CanUse()
    {
        return base.CanUse() && !_chicken.Grounding.IsGrounded && numJumps < numExtraJumps;
    }
}
