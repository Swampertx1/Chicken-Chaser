using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class InteractibleObject : NetworkBehaviour
{
    [SerializeField] private UnityEvent onInteract;
    
    public virtual void Interact(GameObject interactor)
    {
     
        
        onInteract.Invoke();
    }
}