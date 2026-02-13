using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Unity.Netcode;
using Utilities;

public class LayEggs : NetworkBehaviour
{
    private static readonly int T = Shader.PropertyToID("_T");

    [SerializeField] private float LayingEggDuration;
    [SerializeField] private int EggLimit = 2;
    [SerializeField] private NetworkObject[] Eggs; // Changed to NetworkObject[]
    [SerializeField] private DecalProjector Progression;
    
    private int EggCount;
    private Material ProgressMaterial;
    private Coroutine EggRoutine;

    // Network variable to sync progress across all clients
    private NetworkVariable<float> networkProgress = new NetworkVariable<float>(
        0f, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server
    );

    private void Awake()
    {
        ProgressMaterial = Progression.material;
        ProgressMaterial.SetFloat(T, 0);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        // Only server randomly enables nests at game start
        if (IsServer)
        {
            Eggs.Shuffle();
            EggLimit = Mathf.Min(EggLimit, Eggs.Length);
        }

        // All clients subscribe to progress updates
        networkProgress.OnValueChanged += OnProgressChanged;
        
        // Set initial progress on clients that join later
        ProgressMaterial.SetFloat(T, networkProgress.Value);
    }

    public override void OnNetworkDespawn()
    {
        networkProgress.OnValueChanged -= OnProgressChanged;
        base.OnNetworkDespawn();
    }

    private void OnProgressChanged(float previousValue, float newValue)
    {
        // Update visual progress on all clients when network variable changes
        ProgressMaterial.SetFloat(T, newValue);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only server handles game logic
        if (!IsServer) return;

        if (other.attachedRigidbody && other.attachedRigidbody.TryGetComponent(out Chicken chicken))
        {
            EggRoutine = StartCoroutine(StartLayingEggs());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Only server handles game logic
        if (!IsServer) return;

        if (other.attachedRigidbody && other.attachedRigidbody.TryGetComponent(out Chicken chicken))
        {
            if (EggRoutine != null)
            {
                StopCoroutine(EggRoutine);
                EggRoutine = null;
            }
            networkProgress.Value = 0;
        }
    }

    private IEnumerator StartLayingEggs()
    {
        while (EggCount < EggLimit)
        {
            float Timer = 0;

            while (Timer < LayingEggDuration)
            {
                Timer += Time.deltaTime;
                float percent = Timer / LayingEggDuration;
                
                // Update network variable (server only)
                networkProgress.Value = percent;
                
                yield return null;
            }

            LayEgg();
        }

        Debug.Log("Chicken Done Laying eggs");
    }

    private void LayEgg()
    {
        // Server spawns the egg, which makes it visible to all clients
        NetworkObject eggNetworkObject = Eggs[EggCount];
        eggNetworkObject.Spawn(); // This spawns it on all clients
        
        EggCount++;
        
        // Reset progress after laying
        networkProgress.Value = 0;
    }
}