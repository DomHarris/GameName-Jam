using UnityEngine;

namespace Car
{
    [RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(IVehicleInput))]
    public class CarController : MonoBehaviour
    {
        public CarData Data { get; private set; }
        
        [SerializeField] private CarConfig carConfig;
        [SerializeField] private Transform[] tireTransforms;
        
        private Rigidbody _rb;
        private CarPhysicsEngine _engine;

        private IVehicleInput _input;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _input = GetComponent<IVehicleInput>();
            ConfigureRigidbody();
            
            if (carConfig != null && tireTransforms is { Length: > 0 })
            {
                Data = new CarData();
                _engine = new CarPhysicsEngine(carConfig, _rb, transform, tireTransforms, Data);
            }
            else
            {
                Debug.LogError("Car Controller is missing Config or Tires!");
                enabled = false;
            }
        }

        private void FixedUpdate()
        {
            _engine.ProcessPhysics(_input.ThrottleInput, _input.SteeringInput, _input.BrakeInput, _input.HandbrakeInput);
        }

        private void ConfigureRigidbody()
        {
            if (tireTransforms == null || tireTransforms.Length == 0)
                return;

            var wheelHeightTotal = 0f;
            var wheelCount = 0;

            foreach (var tire in tireTransforms)
            {
                if (tire == null)
                    continue;

                wheelHeightTotal += transform.InverseTransformPoint(tire.position).y;
                wheelCount++;
            }

            if (wheelCount == 0)
                return;

            var averageWheelHeight = wheelHeightTotal / wheelCount;
            var minCenterOfMassY = averageWheelHeight * 0.35f;
            var centerOfMass = _rb.centerOfMass;

            // A center of mass below the tire contact patch can invert pitch and roll response.
            if (centerOfMass.y < minCenterOfMassY)
            {
                centerOfMass.y = minCenterOfMassY;
                _rb.centerOfMass = centerOfMass;
            }
        }
        
        private void OnDrawGizmos()
        {
            if (tireTransforms == null || carConfig == null) return;
            Gizmos.color = Color.yellow;
            foreach (var t in tireTransforms)
            {
                if(t != null)
                    Gizmos.DrawLine(t.position, t.position - transform.up * carConfig.suspensionRestDist);
            }
        }
    }
}