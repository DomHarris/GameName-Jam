using UnityEngine;

namespace Car
{
    public class CarData
    {
        public float Speed { get; set; }
        public float NormalisedSpeed { get; set; }
        
        public Vector3 MovementDirection { get; set; }
    }
    
    [CreateAssetMenu(fileName = "NewCarConfig", menuName = "Racing/Car Config")]
    public class CarConfig : ScriptableObject
    {
        [Header("Suspension")] public float suspensionRestDist = 0.5f;
        public float springStrength = 1000f;
        public float springDamper = 100f;
        
        [Header("Movement")] public float carTopSpeed = 20f;
        public float accelForce = 500f;
        public float steerAngle = 30f;
        public AnimationCurve powerCurve;

        [Header("Grip & Friction")] public AnimationCurve gripCurveFront;
        public AnimationCurve gripCurveBack;
        public float tireMass = 10f;
        
        public void ValidateCurves()
        {
            if (powerCurve == null || powerCurve.length == 0)
                powerCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 0.5f));

            if (gripCurveFront == null || gripCurveFront.length == 0)
                gripCurveFront = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 0.5f));
            
            if (gripCurveBack == null || gripCurveBack.length == 0)
                gripCurveBack = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 0.5f));
        }
    }
}