using UnityEngine;

    // A base MonoBehaviour for IPlaneHealth implementations
    public abstract class PlaneHealth : MonoBehaviour, IPlaneHealth
    {
        public virtual float ModifyThrottle(float thrust)
        {
            return thrust;
        }

        public virtual float ModifyAileron(float aileron)
        {
            return aileron;
        }

        public virtual float ModifyRudder(float rudder)
        {
            return rudder;
        }

        public virtual float ModifyElevator(float elevator)
        {
            return elevator;
        }

        public virtual bool ModifyGearDeployed(bool deployed)
        {
            return deployed;
        }
    }
