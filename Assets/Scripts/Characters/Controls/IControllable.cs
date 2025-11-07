using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.InputSystem;

public interface IControllable
{
    public void Move(Vector2 direction);
    public void Look(Vector2 direction);
    
    
    public void Jump();
    void Collect();
}
