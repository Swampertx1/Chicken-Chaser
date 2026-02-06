using System.Collections;
using UnityEngine;
using UnityEngine.Audio;


public abstract class AbilityBase : ScriptableObject
{
    [field: SerializeField] public Sprite Icon { get; private set; }
    [field: SerializeField] protected float Cooldown { get; private set; }
    [field: SerializeField] protected bool CanBeHeld { get; private set; }
    [field: SerializeField] protected AudioResource[] Sound { get; private set; }
    [field: SerializeField] protected ParticleSystem[] Particles { get; private set; }
    private float _cooldownTime;
     private Coroutine _activeCoroutine;
     protected bool _isTryingToBeUsed;
     protected AbilitySystem _abilitySystem;
     protected Chicken _chicken;
     public float CooldownPercent => _cooldownTime / Cooldown;



     public void Bind(AbilitySystem abilitySystem)
     {
         _abilitySystem = abilitySystem;
         _chicken = abilitySystem.GetComponent<Chicken>();
         _cooldownTime = Cooldown;
         OnBound();
         BuildParticles();
         BuildSounds();
     }

     protected virtual void BuildParticles()
     {
         foreach (var p in Particles)
         {
            
            var ps = Instantiate(p, _chicken.transform);
            _chicken.TryAddParticle(p.name, ps);
         }
         
     }

     protected virtual void BuildSounds()
     {
         foreach (var s in Sound)
         {
             _chicken.TryAddSound(s.name,s);

         }
     }

     protected virtual void OnBound()
     {
         
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
