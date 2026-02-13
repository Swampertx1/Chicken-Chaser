using UnityEngine;
using Unity.Netcode;

public class NestManager : NetworkBehaviour
{
    [SerializeField] private LayEggs[] allNests; 
    [SerializeField] private int numberOfNestsToEnable = 3; 

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        
        if (IsServer)
        {
            EnableRandomNests();
        }
    }

    private void EnableRandomNests()
    {
       
        LayEggs[] shuffledNests = (LayEggs[])allNests.Clone();
        ShuffleArray(shuffledNests);

       
        foreach (var nest in allNests)
        {
            nest.gameObject.SetActive(false);
        }

        
        int nestsToActivate = Mathf.Min(numberOfNestsToEnable, shuffledNests.Length);
        for (int i = 0; i < nestsToActivate; i++)
        {
            shuffledNests[i].gameObject.SetActive(true);
        }

        Debug.Log($"Enabled {nestsToActivate} random nests");
    }

    private void ShuffleArray<T>(T[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            T temp = array[i];
            array[i] = array[randomIndex];
            array[randomIndex] = temp;
        }
    }
}