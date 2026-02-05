using System.Collections;
using AI;
using Interfaces;
using Managers;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.AI;
using Utilities;
using Random = UnityEngine.Random;

namespace Characters
{
    public class HumanAI : Human, IDetector
    {
        public enum EHumanState
        {
            Idle,
            Pathing,
            Looking,
            Chasing,
            Rolling
        }

        [Header("Detection")]
        [SerializeField] private MeshRenderer detectionBar;
        private Material _detectionBarMat;

        private float _currentDetection;
        private EHumanState _myState;
        private bool _isDetectionDecaying;
        private float _detectionModifier;
        private float _previousSuggestedDelay;
        private Vector3 _suggestedForward;

        private float _timeSinceLastSpoken;

        private static readonly WaitForSeconds TimerDelay = new WaitForSeconds(0.016f);
        
        private PathHandler _pathHandler;
        private Coroutine _decayTimer;
        private Coroutine _currentRoutine;
        private NavMeshAgent _agent;

        public override float Speed => _agent.velocity.magnitude;

        private void Awake()
        {
            _detectionBarMat = detectionBar.material;
            _pathHandler = GetComponent<PathHandler>();
            _agent = GetComponent<NavMeshAgent>();
        }


        public override void OnControllerEnabled(HumanController controller)
        {
            _controller = controller;
            enabled = true;

            // Initialize AI state
            _myState = EHumanState.Pathing;
            _agent.speed = _controller.Stats.BaseMoveSpeed;
            
            _currentDetection = 0;
            _detectionBarMat.SetFloat(StaticUtilities.FillMatID, 0);

            _detectionModifier = _controller.Stats.IdleStateDetectionModifier;
            
            // Start AI behavior
            _currentRoutine = StartCoroutine(Pathing());
        }

        public override void OnControllerDisabled()
        {
            enabled = false;

            // Clean up AI coroutines
            if (_currentRoutine != null)
            {
                StopCoroutine(_currentRoutine);
                _currentRoutine = null;
            }
            
            if (_decayTimer != null)
            {
                StopCoroutine(_decayTimer);
                _decayTimer = null;
            }

            // Stop agent
            _agent.isStopped = true;
            _agent.ResetPath();
            
            // Reset animations
            Animator.SetBool(StaticUtilities.IsSearchingAnimID, false);
            Animator.SetFloat(StaticUtilities.MoveSpeedAnimID, 0);
            
            _agent.isStopped = false;
            _agent.updateRotation = false; // Player handles rotation
            _agent.speed = _controller.Stats.BaseMoveSpeed;
        }

        private void Update()
        {
            UpdateAnimation();
            
            float dt = Time.deltaTime;
            
            if (_isDetectionDecaying)
            {
                RemoveDetection(_controller.Stats.DetectionDecayRate * dt);
            }

            if (_myState is EHumanState.Idle or EHumanState.Looking or EHumanState.Rolling)
            {
                FaceTarget(_suggestedForward);
            }
            
            if (_myState is EHumanState.Rolling)
            {
                HandleRollingMovement(dt);
            }
        }

        private void HandleRollingMovement(float dt)
        {
            if (!Physics.Raycast(transform.position + Vector3.up, _suggestedForward, 1, StaticUtilities.GroundLayers))
            {
                if (Physics.Raycast(transform.position, Vector3.down, out var ground, 1, StaticUtilities.GroundLayers))
                { 
                    transform.Translate(Vector3.ProjectOnPlane(_suggestedForward, ground.normal) * (_controller.Stats.RollSpeed * dt), Space.World);
                }
            }
        }

        #region AILogic
        
        private IEnumerator Idle()
        {
            _myState = EHumanState.Idle;
            if (_previousSuggestedDelay != 0)
            {
                _suggestedForward = _pathHandler.GetSuggestedForward();
                yield return new WaitForSeconds(Random.Range(_controller.Stats.MinIdleTime + _previousSuggestedDelay,
                    _controller.Stats.MaxIdleTime + _previousSuggestedDelay));
            }

            _currentRoutine = StartCoroutine(Pathing());
        }

        private IEnumerator Pathing()
        {
            _pathHandler.SetNextPatrolPoint();
            _myState = EHumanState.Pathing;
            
            yield return new WaitWhile(() => _agent.pathPending);
            
            while(!_pathHandler.HasReachedDestination(out _previousSuggestedDelay))
            {
                yield return TimerDelay;
            }
            
            _currentRoutine = StartCoroutine(Idle());
        }

        private IEnumerator Looking(Vector3 target)
        {
            _myState = EHumanState.Looking;
            _agent.speed = _controller.Stats.ChaseMoveSpeed;
            _agent.isStopped = true;
            
            float nt = Time.timeSinceLevelLoad;
            if (nt - _controller.Stats.TimeNeededToTalk >= _timeSinceLastSpoken)
            {
                _timeSinceLastSpoken = nt;
                AudioSource.PlayOneShot(_controller.Stats.GetRandomHuh(), SettingsManager.currentSettings.SoundVolume * _controller.Stats.HuhLoudness);
            }
            
            Animator.SetBool(StaticUtilities.IsSearchingAnimID, true);
            _detectionModifier = _controller.Stats.LookingStateDetectionModifier;

            _suggestedForward = target - transform.position;
            
            while(_currentDetection > 0f)
            {
                yield return TimerDelay;
                _suggestedForward = Quaternion.Euler(0, Random.Range(-_controller.Stats.LookRotationAngle, _controller.Stats.LookRotationAngle), 0) * _suggestedForward;
            }
            
            Animator.SetBool(StaticUtilities.IsSearchingAnimID, false);
            _detectionModifier = _controller.Stats.IdleStateDetectionModifier;

            if(_currentRoutine != null) StopCoroutine(_currentRoutine);
            _currentRoutine = StartCoroutine(Pathing());
            _agent.speed = _controller.Stats.BaseMoveSpeed;
            _agent.isStopped = false;
        }
        
        private void Chasing(Vector3 target)
        {
            _agent.isStopped = false;
            _timeSinceLastSpoken = Time.timeSinceLevelLoad;
            _myState = EHumanState.Chasing;
            
            AudioSource.PlayOneShot(_controller.Stats.GetRandomHey(), SettingsManager.currentSettings.SoundVolume * _controller.Stats.HeyLoudness);
            AudioDetection.onSoundPlayed.Invoke(transform.position, _controller.Stats.HeyLoudness, _controller.Stats.HeyLoudness * 10, EAudioLayer.Human);
            
            Animator.SetBool(StaticUtilities.IsSearchingAnimID, false);
            _agent.destination = target;

            AIManager.BeginChasing();
        }

        public override void EndRoll()
        {
            _myState = EHumanState.Chasing;
            _agent.isStopped = false;
            RemoveDetection(40);
        }

        #endregion

        #region Detection

        public void AddDetection(Vector3 location, float detection, EDetectionType detectionType)
        {
            if (_myState == EHumanState.Rolling || 
                _myState == EHumanState.Chasing && (detectionType & _controller.Stats.IgnoreWhileChasing) != 0)
                return;
            
            _currentDetection = Mathf.Min(_currentDetection + detection * _detectionModifier, _controller.Stats.MaxDetection);
            float detectPerc = _currentDetection / _controller.Stats.MaxDetection;
            _detectionBarMat.SetFloat(StaticUtilities.FillMatID, detectPerc);
            
            if (detectPerc >= 0.5f && _myState != EHumanState.Chasing && _myState != EHumanState.Looking)
            {
                if(_currentRoutine != null) StopCoroutine(_currentRoutine);
                _currentRoutine = StartCoroutine(Looking(location));
            }
            else if (detectPerc >= 1f)
            {
                if (_myState == EHumanState.Looking)
                {
                    if(_currentRoutine != null) StopCoroutine(_currentRoutine);
                    Chasing(location);
                }
                
                _agent.SetDestination(location);
                
                Vector3 direction = (location - transform.position);
                float distance = direction.magnitude;
                
                if (distance <= _controller.Stats.DiveDistance && Vector3.Dot(transform.forward, direction) > 0.5)
                {
                    Animator.SetTrigger(StaticUtilities.CaptureAnimID);
                    _myState = EHumanState.Rolling;
                    _suggestedForward = direction / distance;
                    _agent.isStopped = true;
                    StopCoroutine(_currentRoutine);
                }
            }
            
            if(_decayTimer != null) StopCoroutine(_decayTimer);
            _decayTimer = StartCoroutine(BeginDecayCooldown());
        }
        
        private void RemoveDetection(float amount)
        {
            if (_myState == EHumanState.Rolling) return;
            
            _currentDetection = Mathf.Max(_currentDetection - amount, 0);
            float detectPerc = _currentDetection / _controller.Stats.MaxDetection;
            _detectionBarMat.SetFloat(StaticUtilities.FillMatID, detectPerc);
            
            if (detectPerc <= 0.9f && _myState == EHumanState.Chasing)
            {
                StopCoroutine(_currentRoutine);
                _currentRoutine = StartCoroutine(Looking(_agent.destination));
                AIManager.StopChasing();
            }

            if (detectPerc == 0) _isDetectionDecaying = false;
        }
        
        private IEnumerator BeginDecayCooldown()
        {
            _isDetectionDecaying = false;
            yield return new WaitForSeconds(_controller.Stats.BeginDecayCooldown);
            _isDetectionDecaying = true;
        }
        
        #endregion
    }
}