using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class AbilityBase : ScriptableObject
{
    [field: SerializeField] public Sprite Icon { get; private set; }
    [field: SerializeField] protected float Cooldown { get; private set; }
    [field: SerializeField] protected bool CanBeHeld { get; private set; }
    [field: SerializeField] protected AudioClip Sound { get; private set; }
    [field: SerializeField] protected ParticleSystem Particles { get; private set; }
    private float cooldownTime;
     private Coroutine _activeCoroutine;
     private bool _isTryingToBeUsed;
     protected AbilitySystem abilitySystem;
     protected Chicken chicken;
     public float CooldownPercent => cooldownTime / Cooldown;



     public void Bind(AbilitySystem abilitySystem)
     {
         this.abilitySystem = abilitySystem;
         chicken = abilitySystem.GetComponent<Chicken>();
         cooldownTime = Cooldown;
         
     }
    public bool OnCooldown()
    {
     return cooldownTime < Cooldown; 
    }

    
    public  void StopActivate()
    {
        _isTryingToBeUsed = false;
        
    }
    public  void StartActivate()
    {
        _isTryingToBeUsed = true;
        if(_activeCoroutine  != null)
            return;
        _activeCoroutine = abilitySystem.StartCoroutine(Used());
    }

    public virtual bool CanUse() => !OnCooldown();
    protected abstract IEnumerator Activate();

    private IEnumerator CoolDown()
    {
        while (OnCooldown())
        {
            cooldownTime += Time.deltaTime;
            yield return null;
        }

        cooldownTime = Cooldown;
    }

    private IEnumerator Used()
    {
        while (_isTryingToBeUsed)
        {
            if (CanUse())
            {
               yield return Activate();
               cooldownTime = 0;
                 abilitySystem.StartCoroutine(CoolDown());
                
            }

            yield return null;
        }
        _activeCoroutine = null;
        
    }


}
