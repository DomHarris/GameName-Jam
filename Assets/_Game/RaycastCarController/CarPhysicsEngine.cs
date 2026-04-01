using System;
using UnityEngine;


namespace Car
{
    public class CarPhysicsEngine
    {
        private readonly CarConfig _config;
        private readonly Rigidbody _rb;
        private readonly Transform _carTransform;
        private readonly TireData[] _tires;
        
        private CarData _data;

        private struct TireData
        {
            public Transform Transform;
            public bool IsFrontWheel;
        }
        
        private bool _wasAirborne;

        public CarPhysicsEngine(CarConfig config, Rigidbody rb, Transform carTransform, Transform[] tireTransforms, CarData data)
        {
            _data = data;
            _config = config;
            _rb = rb;
            _carTransform = carTransform;
            _config.ValidateCurves();

            _tires = new TireData[tireTransforms.Length];
            for (int i = 0; i < tireTransforms.Length; i++)
            {
                _tires[i] = new TireData
                {
                    Transform = tireTransforms[i],
                    IsFrontWheel = i < 2
                };
            }
        }

        public void ProcessPhysics(float throttleInput, float steerInput, bool isBraking)
        {
            var carUp = _carTransform.up;
            var dt = Time.fixedDeltaTime;
            
            var velocity = _rb.linearVelocity;
            _data.Speed = velocity.magnitude;
            _data.NormalisedSpeed = Mathf.Clamp01(_data.Speed / _config.carTopSpeed);
            
            if (_data.Speed > 0.0001f) 
                _data.MovementDirection = velocity / _data.Speed;
            
            for (var i = 0; i < _tires.Length; i++)
            {
                var tire = _tires[i];
                var tireTrans = tire.Transform;
                var tirePos = tireTrans.position;
                var gripCurve = tire.IsFrontWheel ? _config.gripCurveFront : _config.gripCurveBack;

                if (tire.IsFrontWheel)
                    tireTrans.localRotation = Quaternion.Euler(0, steerInput * _config.steerAngle, 0);
                
                if (Physics.Raycast(tirePos, -carUp, out var hit, _config.suspensionRestDist))
                {
                    var offset = _config.suspensionRestDist - hit.distance;
                    
                    var tireWorldVel = _rb.GetPointVelocity(tirePos);
                    var vel = Vector3.Dot(carUp, tireWorldVel);
                    var force = (offset * _config.springStrength) - (vel * _config.springDamper);
                    _rb.AddForceAtPosition(carUp * force, tirePos);

                    var steeringDir = tireTrans.right;
                    var steeringVel = Vector3.Dot(steeringDir, tireWorldVel);
                    var tireVelSpeed = tireWorldVel.magnitude;
                    var slipRatio = tireVelSpeed > 0 ? Mathf.Abs(steeringVel) / tireVelSpeed : 0f;

                    var gripFactor = gripCurve.Evaluate(slipRatio);
                    
                    var desiredVelChange = -steeringVel * gripFactor;
                    var desiredAccel = desiredVelChange / dt;
                    _rb.AddForceAtPosition(steeringDir * (_config.tireMass * desiredAccel), tirePos);

                    
                    if (throttleInput > 0)
                    {
                        var accelDir = tireTrans.forward;
                        
                        var torque = _config.powerCurve.Evaluate(_data.NormalisedSpeed) * throttleInput * _config.accelForce;
                        _rb.AddForceAtPosition(accelDir * torque, tirePos);
                    }
                }
            }
        }
    }
}