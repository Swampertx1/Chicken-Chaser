using UnityEngine;
using UnityEngine.InputSystem;

public interface IControllable
{
  /*  public void Move(Vector2 direction);
    public void Look(Vector2 direction);
    public void Jump();
    public void Collect();
    
    
    public void Ability1();
    public void Ability2();
    public void Ability3();
    public void Ability4(); */
  
  public void OnControlsGained(PlayerInput input);

  //public void OnControlsLost();

}