using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

public class Maturing : InteractibleObject

{
    [SerializeField] private float playerGrowingAnim;
    [SerializeField, Range(0,10)] private float playerScale;
     private Transform player;
     [SerializeField, Range(0,10)] private float secondsUntilUnMature;
    
     [SerializeField, Range(0,10)] private float playerShrinkAnim;
  
     [SerializeField] AnimationCurve growAnimationCurve;
     [SerializeField] AnimationCurve shrinkAnimationCurve;
     [SerializeField] ParticleSystem onGrowComplete;
     [SerializeField] private float[] nums;


     private void Awake()
     {
         nums.Shuffle();
     }

     public override void Interact(GameObject interactor)
  {
      
      player = interactor.transform;
    base.Interact(interactor);
    
    StartCoroutine(HandleGrowing());
     
  }

 

  private IEnumerator HandleGrowing()
  {
      
      Vector3 startSize = player.localScale;
      Vector3 endSize = player.localScale * playerScale;
      yield return player.AnimateLocalScale(startSize, endSize, playerGrowingAnim, growAnimationCurve);
      SpawnParticles();
    yield return new WaitForSeconds(secondsUntilUnMature);
      yield return player.AnimateLocalScale(endSize, startSize, playerShrinkAnim, shrinkAnimationCurve);
  }


  private void SpawnParticles()
  {
      ParticleSystem ps = Instantiate(onGrowComplete, player.position, player.rotation);
      var shape = ps.shape;
      shape.radius = player.localScale.x * 0.1f;
      Destroy(ps.gameObject, ps.main.duration);

  }
}
