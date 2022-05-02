

// A discrete-time, general-purpose PID controller.

using UnityEngine;

public class PidController
{
    /// Proportional gain; output proportional to current error
    private readonly float kP;

    /// Integral gain; output proportional to accumulated error
    private readonly float kI;

    /// Derivative gain; output proportional to current error rate
    private readonly float kD;

    /// The cumulative error over time
    private float errorIntegral;

    /// The last error
    private float lastError;

    /// The last time the error was updated
    private float lastTime;

    private float maxIntegral;

    private bool isFirstRun = true;


    public PidController(
        float kP,
        float kI,
        float kD,
        float maxIntegral = 1f)
    {
        this.kP = kP;
        this.kI = kI;
        this.kD = kD;
        this.maxIntegral = maxIntegral;
    }

    public float Update(float currentTime, float error)
    {
        if (isFirstRun)
        {
            errorIntegral = 0;
            lastError = error;
            lastTime = currentTime;
            isFirstRun = false;
        }

        float dt = currentTime - lastTime;
        float errorRate = (error - lastError) / dt;
        errorIntegral += error * dt;
        lastError = error;
        lastTime = currentTime;

        errorIntegral = Mathf.Clamp(errorIntegral, -maxIntegral, maxIntegral);

        return kP * error + kI * errorIntegral + kD * errorRate;
    }
}
