using UnityEngine;
using UnityEngine.Events;

public class InteractibleObject : MonoBehaviour
{
    [SerializeField] private UnityEvent onInteract;
    
    public virtual void Interact(GameObject interactor)
    {
     
        
        onInteract.Invoke();
    }
}