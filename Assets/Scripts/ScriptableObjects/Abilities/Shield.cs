using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Shield", menuName = "Abilities/Shield")]
public class Shield : AbilityBase
{
    public bool shieldIsActive;
    [SerializeField]private float duration;
    
    protected override IEnumerator Activate()
    {
        ActivateShield();
        yield return new WaitForSeconds(duration); 
        
        DeactivateShield();
        
    }

    private void ActivateShield()
    {
        shieldIsActive = true;
        Debug.Log("Shield activated");
    }
    private void DeactivateShield()
    {
        shieldIsActive = false;
        Debug.Log("Shield deactivated");
    }
}