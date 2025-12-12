using System;
using UnityEngine;

public class AbilitySystem : MonoBehaviour
{
    
    [SerializeField] AbilityBase abilityTemplate;
    private AbilityBase _ability;

    private void Awake()
    {
        ConstructAbilities();
    }

    public void ConstructAbilities()
    {
        _ability = Instantiate(abilityTemplate.GetType());
    }
}
