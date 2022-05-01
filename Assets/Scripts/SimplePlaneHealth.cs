using UnityEngine;

public class SimplePlaneHealth : PlaneHealth, IProjectileReceiver
{
    private float aileronHealth = 3f;
    private float rudderHealth = 3f;
    private float elevatorHealth = 3f;
    private float throttleHealth = 3f;

    private float stuckAileronValue;
    private float stuckRudderValue;
    private float stuckElevatorValue;
    private float stuckThrottleValue;

    public void TakeProjectile(Projectile bullet)
    {
        // Subtract 1 from a random subsystem
        switch (Random.Range(0, 4))
        {
            case 0:
                aileronHealth = Mathf.Max(0, aileronHealth - 1);
                if (aileronHealth == 0 && stuckAileronValue == 0)
                {
                    stuckAileronValue = Random.value;
                    Debug.Log("Aileron disabled");
                }
                break;
            case 1:
                rudderHealth = Mathf.Max(0, rudderHealth - 1);
                if (rudderHealth == 0 && stuckRudderValue == 0)
                {
                    stuckRudderValue = Random.value;
                    Debug.Log("Rudder disabled");
                }
                break;
            case 2:
                elevatorHealth = Mathf.Max(0, elevatorHealth - 1);
                if (elevatorHealth == 0 && stuckElevatorValue == 0)
                {
                    stuckElevatorValue = Random.value;
                    Debug.Log("Elevator disabled");
                }
                break;
            case 3:
                throttleHealth = Mathf.Max(0, throttleHealth - 1);
                if (throttleHealth == 0 && stuckThrottleValue == 0)
                {
                    // throttle always gets stuck at low power
                    stuckThrottleValue = Random.Range(0, 0.2f);
                    Debug.Log("Throttle disabled");
                }
                break;
        }
    }

    public override float ModifyAileron(float aileron)
    {
        return aileronHealth > 0 ? aileron : stuckAileronValue;
    }

    public override float ModifyRudder(float rudder)
    {
        return rudderHealth > 0 ? rudder : stuckRudderValue;
    }

    public override float ModifyElevator(float elevator)
    {
        return elevatorHealth > 0 ? elevator : stuckElevatorValue;
    }

    public override float ModifyThrottle(float throttle)
    {
        return throttleHealth > 0 ? throttle : stuckThrottleValue;
    }
}
