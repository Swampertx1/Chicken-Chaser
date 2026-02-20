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
    [SerializeField] private GameObject[] Eggs; 
    [SerializeField] private DecalProjector Progression;
    
    public NetworkVariable<uint> EggCount = new();
    private Material ProgressMaterial;
    private Coroutine EggRoutine;
   
  

    
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
            
            EggLimit = Mathf.Min(EggLimit, Eggs.Length);
        }

        // All clients subscribe to progress updates
        networkProgress.OnValueChanged += OnProgressChanged;
        EggCount.OnValueChanged += updateEggs;
        updateEggs(0, EggCount.Value);
        
        // Set initial progress on clients that join later
        ProgressMaterial.SetFloat(T, networkProgress.Value);
    }

    private void updateEggs(uint previousValue, uint newValue)
    {
        for (int i = 0; i < Eggs.Length; i++)
        {
            Eggs[i].SetActive(i < newValue);
        }
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
        while (EggCount.Value < EggLimit)
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
        
        
        EggCount.Value++;
        
        // Reset progress after laying
        networkProgress.Value = 0;
    }
}