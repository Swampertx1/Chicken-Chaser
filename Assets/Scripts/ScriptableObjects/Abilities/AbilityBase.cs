using System.Collections;
using UnityEngine;


public abstract class AbilityBase : ScriptableObject
{
    [field: SerializeField] public Sprite Icon { get; private set; }
    [field: SerializeField] protected float Cooldown { get; private set; }
    [field: SerializeField] protected bool CanBeHeld { get; private set; }
    [field: SerializeField] protected AudioClip Sound { get; private set; }
    [field: SerializeField] protected ParticleSystem Particles { get; private set; }
    private float _cooldownTime;
     private Coroutine _activeCoroutine;
     private bool _isTryingToBeUsed;
     protected AbilitySystem _abilitySystem;
     protected Chicken _chicken;
     public float CooldownPercent => _cooldownTime / Cooldown;



     public void Bind(AbilitySystem abilitySystem)
     {
         _abilitySystem = abilitySystem;
         _chicken = abilitySystem.GetComponent<Chicken>();
         _cooldownTime = Cooldown;
         
     }
    public bool OnCooldown()
    {
     return _cooldownTime < Cooldown; 
    }

    
    public  void StopActivate()
    {
        _isTryingToBeUsed = false;
        
    }
    public  void StartActivate()
    {
        _isTryingToBeUsed = true;
        if (_activeCoroutine != null) return;
        _activeCoroutine = _abilitySystem.StartCoroutine(Used());
        
    }

    public virtual bool CanUse() => !OnCooldown();
    protected abstract IEnumerator Activate();

    private IEnumerator CoolDown()
    {
        while (OnCooldown())
        {
            _cooldownTime += Time.deltaTime;
            yield return null;
        }

        _cooldownTime = Cooldown;
    }

    private IEnumerator Used()
    {
        if (!CanBeHeld)
        {
            if (CanUse())
            {
                yield return Activate();
                _cooldownTime = 0;
                _abilitySystem.StartCoroutine(CoolDown());
            }
            _activeCoroutine = null;
            yield break;
        }

        while (_isTryingToBeUsed)
        {
            if (CanUse())
            {
               yield return Activate();
               _cooldownTime = 0;
               _abilitySystem.StartCoroutine(CoolDown());
            }

            yield return null;
        }
        _activeCoroutine = null;
    }

}
