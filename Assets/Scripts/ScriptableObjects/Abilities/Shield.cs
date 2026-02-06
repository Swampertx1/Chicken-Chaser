using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Shield", menuName = "Abilities/Shield")]
public class Shield : AbilityBase
{
    public bool shieldIsActive;
    [SerializeField]private float duration;
    protected override void BuildParticles()
    {
        base.BuildParticles();
        var mainModule = Particles[0].main;
        mainModule.duration = duration;
        mainModule.startLifetime = duration;
    }

    protected override IEnumerator Activate()
    {
        float Timer = 0;
        
        ActivateShield();
        while (Timer < duration && _isTryingToBeUsed)
        {
            Timer += Time.deltaTime;
            yield return null;
        }

        DeactivateShield();
        
    }

    private void ActivateShield()
    {
        _chicken.PlayParticleRPC(Particles[0].name);
        shieldIsActive = true;
        Debug.Log("Shield activated");
    }
    private void DeactivateShield()
    {
        _chicken.StopParticleRPC(Particles[0].name,0.1f);
        shieldIsActive = false;
        Debug.Log("Shield deactivated");
    }
}