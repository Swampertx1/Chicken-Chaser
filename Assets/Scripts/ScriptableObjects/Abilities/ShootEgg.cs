using System.Collections;
using UnityEngine;
using Unity.Netcode;

[CreateAssetMenu(fileName = "ShootEggs", menuName = "Abilities/ShootEggs")]
public class ShootEggs : AbilityBase
{
    [SerializeField] private Rigidbody eggPrefab;
    [SerializeField] private float shootForce = 15f;
    [SerializeField] private float upwardForce = 2f;

    protected override IEnumerator Activate()
    {
        Vector3 velocity = chicken.Cam.forward * shootForce + chicken.Cam.up * upwardForce;

      
  
       

        
         Eggpool.instance.SpawnEggServerRpc(chicken.FirePoint.position, Quaternion.LookRotation(velocity), velocity);
      
        

        
        
        yield return null;
    }
}

/*public class EggSpawner : NetworkBehaviour
{
    public void RequestSpawnEgg(GameObject prefab, Vector3 pos, Vector3 dir, float force, float upForce)
    {
        if (!IsOwner) return;
        
        // Find the prefab index in the NetworkManager's list
        int prefabIndex = -1;
        var networkPrefabs = NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs;
        for (int i = 0; i < networkPrefabs.Count; i++)
        {
            if (networkPrefabs[i].Prefab == prefab)
            {
                prefabIndex = i;
                break;
            }
        }

       

        SpawnEggServerRpc(prefabIndex, pos, dir, force, upForce);
    }

    [ServerRpc]
    private void SpawnEggServerRpc(int prefabIndex, Vector3 pos, Vector3 dir, float force, float upForce)
    {
        var networkPrefabs = NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs;
        GameObject prefab = networkPrefabs[prefabIndex].Prefab;
        
        GameObject egg = Instantiate(prefab, pos, Quaternion.identity);
        NetworkObject netObj = egg.GetComponent<NetworkObject>();
       
            netObj.Spawn();
        

        Rigidbody rb = egg.GetComponent<Rigidbody>();
       
        
            rb.AddForce(dir.normalized * force + Vector3.up * upForce, ForceMode.Impulse);
        
    }
}*/