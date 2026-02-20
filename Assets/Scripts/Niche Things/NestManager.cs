using UnityEngine;
using Unity.Netcode;
using Utilities;

public class NestManager : NetworkBehaviour
{
    [SerializeField] private LayEggs[] allNests;
    [SerializeField] private int numberOfNestsToEnable = 3;
    private WinCondition winCondition;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        winCondition = FindObjectOfType<WinCondition>();

        if (IsServer)
            EnableRandomNests();
    }

    private void EnableRandomNests()
    {
        int nestsToActivate = Mathf.Min(numberOfNestsToEnable, allNests.Length);
        int[] array = new int[nestsToActivate];
        int[] nums = new int[allNests.Length];
        for (int i = 0; i < allNests.Length; i++)
        {
            nums[i] = i;
        }
        nums.Shuffle();
        for (int i = 0; i < nestsToActivate; i++)
        {
            array[i] = nums[i];
        }

        EnableNestsRpc(array);
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
    private void EnableNestsRpc(int[] nestsToEnable)
    {
        foreach (var nest in allNests)
            nest.gameObject.SetActive(false);

        int nestsToActivate = nestsToEnable.Length;
        for (int i = 0; i < nestsToActivate; i++)
            allNests[nestsToEnable[i]].gameObject.SetActive(true);

        LayEggs[] active = new LayEggs[nestsToEnable.Length];
        for (int i = 0; i < nestsToEnable.Length; i++)
            active[i] = allNests[nestsToEnable[i]];

        winCondition.SetActiveNests(active);
    }
}