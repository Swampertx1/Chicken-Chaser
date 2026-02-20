using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class WinCondition : NetworkBehaviour
{
    private LayEggs[] activeNests;
    [SerializeField] private int NumberofEggslayed;
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (activeNests == null) return;
        
        NumberofEggslayed = 0;
        foreach (var nest in activeNests)
            NumberofEggslayed += (int)nest.EggCount.Value;

        if (NumberofEggslayed >= activeNests.Length * 3)
        {
            animator.SetBool("IsOpen", true);
        }
    }

    public void SetActiveNests(LayEggs[] nests)
    {
        activeNests = nests;
        foreach (var nest in activeNests)
            nest.EggCount.OnValueChanged += (previous, current) => NumberofEggslayed++;
    }
}