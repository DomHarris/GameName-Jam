using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Car
{
    [RequireComponent(typeof(CarController))]
    [RequireComponent(typeof(PlayerInput))]
    public class CarInputAdapter : MonoBehaviour, IVehicleInput
    {
        [Header("Cameras")]
        [SerializeField] private CinemachineCamera followCamera;
        [SerializeField] private CinemachineCamera rearCamera;

        public float SteeringInput { get; private set; }
        public float ThrottleInput { get; private set; }
        public bool BrakeInput { get; private set; }
        public bool HandbrakeInput { get; private set; }

        private bool _isLookingBehind;

        private void OnEnable()
        {
            UpdateCameraPriorities(false);
        }

        private void LateUpdate()
        {
            var shouldLookBehind = Keyboard.current?.rightMetaKey.isPressed ?? false;
            if (shouldLookBehind == _isLookingBehind)
                return;

            UpdateCameraPriorities(shouldLookBehind);
        }

        public void OnSteer(InputValue value)
        {
            SteeringInput = value.Get<float>();
        }

        public void OnBrake(InputValue value)
        {
            BrakeInput = value.isPressed;
        }

        public void OnHandbrake(InputValue value)
        {
            HandbrakeInput = value.isPressed;
        }

        public void OnAccelerate(InputValue value)
        {
            ThrottleInput = value.Get<float>();
        }

        private void UpdateCameraPriorities(bool lookBehind)
        {
            _isLookingBehind = lookBehind;

            if (followCamera != null)
                followCamera.Priority = lookBehind ? 10 : 20;

            if (rearCamera != null)
                rearCamera.Priority = lookBehind ? 20 : 10;
        }
    }
}
