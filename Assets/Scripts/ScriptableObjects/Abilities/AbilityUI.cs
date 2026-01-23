using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityUI : MonoBehaviour
{


    [SerializeField] private Image icon;
    [SerializeField] private Image progressBar;
    [SerializeField] private TextMeshProUGUI abilityKey;

     AbilityBase _ability;


    public void SetUpAbility(AbilityBase ability)
    {
        _ability = ability;
        icon.sprite = _ability?.Icon;
        gameObject.SetActive(ability);
    }

    public void SetKey(string key)
    {
        abilityKey.text = key;
    }

    private void LateUpdate()
    {
        progressBar.fillAmount = _ability.CooldownPercent;
    }
    
    
}
