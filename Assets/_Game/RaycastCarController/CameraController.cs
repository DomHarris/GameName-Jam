using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Car
{
    /// <summary>
    /// Chase camera controller for physics-driven arcade racing.
    /// Uses speed-based follow tuning, boost FOV, and manual look support.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("Target References")]
        [SerializeField] private CarController carController;
        [SerializeField] private PlayerInput playerInput;

        [Header("Chase Follow")]
        [SerializeField] private float rigidStiffness = 0.9f;
        [SerializeField] private float rigidRotationSpeed = 6f;
        [SerializeField] private float baseFOV = 60f;
        [SerializeField] private float maxSpeedFOV = 85f;

        [Header("General Camera Settings")]
        [SerializeField] private float cameraDistance = 8f;
        [SerializeField] private float cameraHeight = 3f;
        [SerializeField] private float lookAhead = 5f;
        [SerializeField] private float fovLerpSpeed = 3f;
        
        [Header("Speed Follow")]
        [SerializeField] private float speedDistanceMultiplier = 0.85f;
        [SerializeField] private float speedStiffnessMultiplier = 1.8f;
        
        // Position tracking
        private Vector3 _currentCameraPosition;
        private Quaternion _currentCameraRotation;
        
        // Input
        private float _currentSteerInput;
        private Vector2 _manualLookInput;
        private float _manualLookResetTimer;
        private const float ManualLookResetDelay = 1.5f;

        // Cached values
        private Vector3 _smoothedForwardDirection;

        private void Awake()
        {
            InitializeChaseStateFromCar();
        }
        
        private void InitializeChaseStateFromCar() {
            if (carController == null) {
                return;
            }

            Transform target = GetPresentationTarget();
            _smoothedForwardDirection = target.forward;
            _currentCameraPosition = CalculateIdealPosition(_smoothedForwardDirection);
            _currentCameraRotation = Quaternion.LookRotation(target.position - _currentCameraPosition);
        }
        
        private void LateUpdate()
        {
            if (carController == null) return;
            
            float dt = Time.deltaTime;
            
            // Update cooldowns
            if (_manualLookResetTimer > 0f) _manualLookResetTimer -= dt;
            
            (Vector3 targetPosition, Quaternion targetRotation) = CalculateRigidChaseTarget();
            float currentStiffness = rigidStiffness * Mathf.Lerp(1f, speedStiffnessMultiplier, GetSpeedFollowT());
            
            // Apply spring arm damping (exponential decay)
            float dampingFactor = 1f - Mathf.Exp(-currentStiffness * 10f * dt);
            _currentCameraPosition = Vector3.Lerp(_currentCameraPosition, targetPosition, dampingFactor);
            _currentCameraRotation = Quaternion.Slerp(_currentCameraRotation, targetRotation, dampingFactor);
            
            // Apply to transform
            transform.position = _currentCameraPosition;
            transform.rotation = _currentCameraRotation;
        }
        
        private (Vector3 position, Quaternion rotation) CalculateRigidChaseTarget()
        {
            Transform carTransform = GetPresentationTarget();
            Vector3 carPosition = carTransform.position;
            
            Vector3 targetForward = carTransform.forward;
            
            // Smooth the forward direction (frame-rate independent)
            float forwardSlerpFactor = 1f - Mathf.Exp(-rigidRotationSpeed * Time.deltaTime);
            _smoothedForwardDirection = Vector3.Slerp(_smoothedForwardDirection, targetForward, forwardSlerpFactor).normalized;
            
            Vector3 effectiveDir = _smoothedForwardDirection;
            
            // Apply manual look offset
            if (_manualLookResetTimer > 0f && _manualLookInput.sqrMagnitude > 0.01f)
            {
                Quaternion lookOffset = Quaternion.Euler(-_manualLookInput.y * 30f, _manualLookInput.x * 60f, 0f);
                effectiveDir = lookOffset * effectiveDir;
            }
            
            Vector3 targetPosition = CalculateIdealPosition(effectiveDir);
            
            // Look at car with slight lookahead
            Vector3 lookTarget = carPosition + effectiveDir * lookAhead;
            Quaternion targetRotation = Quaternion.LookRotation(lookTarget - targetPosition);
            
            return (targetPosition, targetRotation);
        }

        private Vector3 CalculateIdealPosition(Vector3 forwardDirection)
        {
            Vector3 carPosition = GetPresentationTarget().position;
            float followDistance = cameraDistance * Mathf.Lerp(1f, speedDistanceMultiplier, GetSpeedFollowT());
            
            // Position behind and above the car
            Vector3 idealPos = carPosition - forwardDirection * followDistance + Vector3.up * cameraHeight;
            
            return idealPos;
        }

        private Transform GetPresentationTarget()
        {
            return carController != null ? carController.transform : transform;
        }

        private float GetSpeedFollowT()
        {
            float normalizedSpeed = carController != null ? carController.Data?.NormalisedSpeed ?? 0f : 0f;
            return normalizedSpeed;
        }
    }
}
