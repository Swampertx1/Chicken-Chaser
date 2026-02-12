using System;
using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Utilities;

public class LayEggs : MonoBehaviour
{
    private static readonly int T = Shader.PropertyToID("_T");

    [SerializeField] private float LayingEggDuration;
    [SerializeField] private int EggLimit = 2;
    [SerializeField] private GameObject[] Eggs;
    [SerializeField] private DecalProjector Progression;
    private int EggCount;
    private Material ProgressMaterial;
    private Coroutine EggRoutine;


    private void Awake()
    {
        Eggs.Shuffle();
        EggLimit = Mathf.Min(EggLimit, Eggs.Length);
        ProgressMaterial = Progression.material;
        ProgressMaterial.SetFloat(T, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody && other.attachedRigidbody.TryGetComponent(out Chicken chicken))

        {
            
           EggRoutine = StartCoroutine(StartLayingEggs());
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.attachedRigidbody && other.attachedRigidbody.TryGetComponent(out Chicken chicken))

        {
            StopCoroutine(EggRoutine);
            ProgressMaterial.SetFloat(T, 0);
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
                ProgressMaterial.SetFloat(T, percent);
                yield return null;
            }

            LayEgg();
        }

        Debug.Log("Chicken Done Laying eggs");
    }

   



    private void LayEgg()
    {
        Eggs[EggCount].SetActive(true);
        EggCount++;
    }

}
