using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

public class ChickenLightUp : MonoBehaviour
{
 [SerializeField] Image chickenLightUpImaged;
[SerializeField] Color chickenLightUpColor;
[SerializeField] private Color chickenLonelyColor;
[SerializeField] private float chickenLightUpAnimationSpeed;
[SerializeField] private Vector3 chickenStartSize;
[SerializeField] private Vector3 chickenEndSize;
[SerializeField] private AnimationCurve growAnimationCurve;
[SerializeField] private AudioClip chickenLightUpSound;
[SerializeField] public AudioSource chickenLightUpAudioSource;
private Coroutine animation;

private void OnEnable()
{
 chickenLightUpAudioSource.PlayOneShot(chickenLightUpSound);
 animation = StartCoroutine((transform.AnimateLocalScale(chickenStartSize, chickenEndSize, chickenLightUpAnimationSpeed, growAnimationCurve)));
 chickenLightUpImaged.color = chickenLightUpColor;
 
}


private void OnDisable()
{
 StopCoroutine((animation));
 chickenLightUpImaged.color = chickenLonelyColor;
 transform.localScale = chickenStartSize;
}



}
