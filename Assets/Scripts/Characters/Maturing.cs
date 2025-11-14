using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
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
 

     public override void Interact(GameObject interactor)
  {
      
      
    base.Interact(interactor);
    onInteract_ServerRpc(interactor.GetComponent<NetworkObject>().NetworkObjectId);
     
  }

  [Rpc(SendTo.ClientsAndHost,  InvokePermission = RpcInvokePermission.Everyone)]
  private void onInteract_ServerRpc(ulong id)
  {
      if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(id, out var networkObject))
      {
          Debug.LogError("Network Object Not Found");
          return;
      }
      player = networkObject.transform;
      StartCoroutine(HandleGrowing());
    
  }
  
  
  

 

  private IEnumerator HandleGrowing()
  {
      
      Vector3 startSize = player.localScale;
      Vector3 endSize = player.localScale * playerScale;
      yield return player.AnimateLocalScale(startSize, endSize, playerGrowingAnim, growAnimationCurve);
      if(HasAuthority)
        SpawnParticles_ClientRpc(player.position, player.rotation,player.localScale);
    yield return new WaitForSeconds(secondsUntilUnMature);
      yield return player.AnimateLocalScale(endSize, startSize, playerShrinkAnim, shrinkAnimationCurve);
  }

  [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Server)]
  private void SpawnParticles_ClientRpc(Vector3 pos, Quaternion rot, Vector3 scale)
  {
      ParticleSystem ps = Instantiate(onGrowComplete, pos, rot);
      var shape = ps.shape;
      shape.radius = scale.x * 0.1f;
      Destroy(ps.gameObject, ps.main.duration);

  }
}
