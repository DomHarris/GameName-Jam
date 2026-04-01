namespace Car
{
    public interface IVehicleInput
    {
        float SteeringInput { get; }
        
        float ThrottleInput { get; }
        
        bool BrakeInput { get; }
        
        bool HandbrakeInput { get; }
    }
}



