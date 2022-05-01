/// A class that modifies control inputs based on the plane's condition
public interface IPlaneHealth
{
    float ModifyThrottle(float thrust);
    float ModifyAileron(float aileron);
    float ModifyRudder(float rudder);
    float ModifyElevator(float elevator);
    bool ModifyGearDeployed(bool deployed);
}
