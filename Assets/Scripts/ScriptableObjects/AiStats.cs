using Interfaces;
using UnityEngine;

namespace ScriptableObjects
{
  [CreateAssetMenu(fileName = "AIStats", menuName = "ChickenChaser/AIStats", order = 1)]
  public class AiStats : ScriptableObject
  {
  
    [Header("AI|Decay")]
    [SerializeField] private float maxDetection = 100;
    [SerializeField] private float detectionDecayRate = 5;
    [SerializeField] private float beginDecayCooldown = 3;
    [SerializeField, Min(0)] private float lookingStateDetectionModifier = 2;
    [SerializeField, Min(0)] private float idleStateDetectionModifier = 1;
    [SerializeField, Min(0)] private float baseMoveSpeed = 2.5f;
    [SerializeField, Min(0)] private float chaseMoveSpeed = 4;
    [SerializeField, Min(0)] private float rollSpeed = 12;
    [SerializeField, Min(0)] private float rollDuration = 0.5f;
    [SerializeField, Range(0,180)] private float lookRotationAngle = 15;
    [SerializeField, Min(0)] private float minIdleTime = 1;
    [SerializeField, Min(0)] private float maxIdleTime = 3;
    [SerializeField, Min(0)] private float diveDistance = 1;
    [SerializeField] private EDetectionType ignoreWhileChasing;

    [Header("Audio")]
    [SerializeField] private AudioClip[] huhs;
    [SerializeField] private AudioClip[] heys;
    [SerializeField] private float huhLoudness;
    [SerializeField] private float heyLoudness;
    [SerializeField] private float timeNeededToTalk = 1;

    public AudioClip GetRandomHuh() => huhs[Random.Range(0, huhs.Length)];
    public AudioClip GetRandomHey() => heys[Random.Range(0, heys.Length)];

    public float HuhLoudness => huhLoudness;
    public float HeyLoudness => heyLoudness;
    public float TimeNeededToTalk => timeNeededToTalk;

  
    public float MaxDetection => maxDetection;
    public float DetectionDecayRate => detectionDecayRate;
    public float BeginDecayCooldown => beginDecayCooldown;
    public float LookingStateDetectionModifier => lookingStateDetectionModifier;
    public float IdleStateDetectionModifier => idleStateDetectionModifier;
    public float BaseMoveSpeed => baseMoveSpeed;
    public float ChaseMoveSpeed => chaseMoveSpeed;
    public float RollSpeed => rollSpeed;
    public float RollDuration => rollDuration;
    public float LookRotationAngle => lookRotationAngle;

    public EDetectionType IgnoreWhileChasing => ignoreWhileChasing;
    
  

  
    public float MinIdleTime => minIdleTime;
    public float MaxIdleTime => maxIdleTime;
    public float DiveDistance => diveDistance;
  }
}
