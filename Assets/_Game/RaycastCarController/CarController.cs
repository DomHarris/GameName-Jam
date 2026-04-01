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
            _engine.ProcessPhysics(_input.ThrottleInput, _input.SteeringInput, _input.BrakeInput);
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