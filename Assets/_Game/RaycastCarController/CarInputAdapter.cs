using UnityEngine;
using UnityEngine.InputSystem;

namespace Car
{
    [RequireComponent(typeof(CarController))]
    [RequireComponent(typeof(PlayerInput))]
    public class CarInputAdapter : MonoBehaviour, IVehicleInput
    {
        public float SteeringInput { get; private set; }
        public float ThrottleInput { get; private set; }
        public bool BrakeInput { get; private set; }

        public void OnSteer(InputValue value)
        {
            SteeringInput = value.Get<float>();
        }

        public void OnBrake(InputValue value)
        {
            BrakeInput = value.isPressed;
        }

        public void OnAccelerate(InputValue value)
        {
            ThrottleInput = value.Get<float>();
        }
    }
}
