using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class AbilitySystem : MonoBehaviour, IControllable
{
    [SerializeField] AbilityBase []abilityTemplate;
    [SerializeField] AbilityUI []abilityUI;
    private AbilityBase []_ability;

    private void Awake()
    {
        ConstructAbilities();
    }

    public void ConstructAbilities()
    {
        _ability = new AbilityBase[abilityTemplate.Length];
        for (var index = 0; index < abilityTemplate.Length; index++)
        {
            var ability = abilityTemplate[index];
            _ability[index] = Instantiate(ability);
            _ability[index].Bind(this);
            abilityUI[index].SetUpAbility(_ability[index]);
           
        }
    }
    
    
    

    private void OnDestroy()
    {
        foreach (var ability in _ability)
            Destroy(ability);
    }

    public void OnControlsGained(PlayerInput input)
    {
        input.actions["Ability1"].performed += ctx => Use(0, ctx.ReadValueAsButton());
        input.actions["Ability2"].performed += ctx => Use(1, ctx.ReadValueAsButton());
        input.actions["Ability3"].performed += ctx => Use(2, ctx.ReadValueAsButton());
        input.actions["Ability4"].performed += ctx => Use(3, ctx.ReadValueAsButton());
        
      abilityUI[0].SetKey(input.actions["Ability1"].GetBindingDisplayString());  
      abilityUI[1].SetKey(input.actions["Ability2"].GetBindingDisplayString());  
      abilityUI[2].SetKey(input.actions["Ability3"].GetBindingDisplayString());  
      abilityUI[3].SetKey(input.actions["Ability4"].GetBindingDisplayString());  
        
     
    }

   

    private void Use(int readValue, bool state)
    {
        if (state)
        {
            _ability[readValue].StartActivate();
        }
        else
        {
            _ability[readValue].StopActivate();
        }
    }
}
