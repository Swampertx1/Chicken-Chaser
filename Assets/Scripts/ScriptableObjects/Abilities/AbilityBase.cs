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
     private Coroutine _cooldownCoroutine;
     private Coroutine _activeCoroutine;
     private bool _isTryingToBeUsed;
     protected AbilitySystem abilitySystem;
     protected Chicken chicken;



     public void Bind(AbilitySystem abilitySystem)
     {
         this.abilitySystem = abilitySystem;
         chicken = abilitySystem.GetComponent<Chicken>();
     }
    public bool OnCooldown()
    {
       return _cooldownCoroutine != null; 
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
        yield return new WaitForSeconds(Cooldown);
        _cooldownCoroutine = null;
    }

    private IEnumerator Used()
    {
        while (_isTryingToBeUsed)
        {
            if (CanUse())
            {
               yield return Activate();
                _cooldownCoroutine = abilitySystem.StartCoroutine(CoolDown());
                yield return _cooldownCoroutine;
            }

            yield return null;
        }
        _activeCoroutine = null;
        
    }


}
