using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Shield", menuName = "Abilities/Shield")]
public class Shield : AbilityBase
{
    public bool shieldIsActive;
    
    protected override IEnumerator Activate()
    {
        A
        yield return null; 
        
    }

    private void ActivateShield()
    {
        shieldIsActive = true;
    }
    private void DeactivateShield()
    {
        shieldIsActive = false;
    }
}