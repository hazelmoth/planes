using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneController : MonoBehaviour
{
	[SerializeField] PlanePropellor propellor;
	[SerializeField] ControlSurfaceAnimator surfaces;
	[SerializeField] LandingGearController gear;
	[SerializeField] PlaneWeapons weapons;
	[SerializeField] PlaneHealth health;
	[SerializeField] Vector3 centerOfDrag;
	[SerializeField] AnimationCurve liftCurve;

	Rigidbody rigidbody;


	[SerializeField] private float maxThrust = 150000;
	[SerializeField] private float liftConstant = 20f;
	[SerializeField] private float optimalDragConstant = 10f;
	[SerializeField] private float backwardsDragConstant = 40f;
	[SerializeField] private float inducedDragConstant = 10f;


	// in N
	[SerializeField] private float yawForce = 2000f;
	[SerializeField] private float rollForce = 7000f;
	[SerializeField] private float pitchForce = 3500f;

	float currentRudderAmount;
	float currentAileronAmount;
	float currentElevatorAmount;

	public PlaneWeapons Weapons => weapons;

	// Status
	public float CurrentSpeed => rigidbody.velocity.magnitude;
	public float VerticalSpeed => rigidbody.velocity.y;
	public float Altitude => transform.position.y;
	public float Pitch { get
		{
			return 90 - Vector3.Angle(transform.forward, Vector3.up);
		} 
	}
	public float Roll { get
		{
			return 90 - Vector3.Angle(transform.right, Vector3.up);
		} }
	public float Heading { get
		{
			return Vector3.Angle(Vector3.ProjectOnPlane(transform.forward, Vector3.up), Vector3.forward);
		} }
	public float AngleOfAttack
	{
		get
		{
			Vector3 velocity = transform.InverseTransformVector(rigidbody.velocity);
			Vector3 trajectory = velocity.normalized;
			float angle = Vector3.SignedAngle(Vector3.forward, trajectory, Vector3.right);
			return angle;
		}
	}
	public float DragMagnitude { get; private set; }
	public float CurrentThrottle { get; private set; }
	public bool GearDeployed =>
		gear.State == LandingGearController.GearState.Deployed
		|| gear.State == LandingGearController.GearState.Deploying;

	// Controls
	public void SetThrottle(float power) => CurrentThrottle = health?.ModifyThrottle(power) ?? power;
	public void SetRudderAmount(float yaw) => currentRudderAmount = health?.ModifyRudder(yaw) ?? yaw;
	public void SetElevatorAmount(float pitch) => currentElevatorAmount = health?.ModifyElevator(pitch) ?? pitch;
	public void SetAileronAmount(float roll) => currentAileronAmount = health?.ModifyAileron(roll) ?? roll;
	public void SetGearDeployed (bool deployed)
	{
		deployed = health?.ModifyGearDeployed(deployed) ?? deployed;
		if (gear) gear.SetDeployed(deployed);
	}
	public void ToggleLandingGear ()
	{
		SetGearDeployed(!GearDeployed);
	}

	// Start is called before the first frame update
	void Start()
    {
		rigidbody = GetComponent<Rigidbody>();
    }
	private void Update()
	{
		if (surfaces != null)
		{
			surfaces.SetAileronAmount(currentAileronAmount);
			surfaces.SetRudderAmount(currentRudderAmount);
			surfaces.SetElevatorAmount(currentElevatorAmount);
		}
	}

	// Update is called once per frame
	void FixedUpdate()
    {
		if (CurrentThrottle < 0) CurrentThrottle = 0;

		float thrust = maxThrust * CurrentThrottle;
		Vector3 thrustVector = Vector3.forward * thrust;

		Vector3 liftVector = Vector3.up * CalculateLift();

		Vector3 yawTorque = Vector3.up * currentRudderAmount * yawForce;
		Vector3 rollTorque = Vector3.back * currentAileronAmount * rollForce;
		Vector3 pitchTorque = Vector3.left * currentElevatorAmount * pitchForce;

		Vector3 netTorque = yawTorque + rollTorque + pitchTorque;

		Vector3 netForce = thrustVector + liftVector;

		rigidbody.AddRelativeForce(netForce);

		Vector3 dragWorldPoint = transform.TransformPoint(centerOfDrag);
		Vector3 dragWorldVector = transform.TransformVector(CalculateDragVector());
		Debug.DrawRay(dragWorldPoint, dragWorldVector, Color.red, Time.fixedDeltaTime);
		rigidbody.AddForceAtPosition(dragWorldVector, dragWorldPoint);

		rigidbody.AddRelativeTorque(netTorque);

		propellor?.SetPower(CurrentThrottle);
    }



	float CalculateLift () {
		// in N
		float pitchFactor = liftCurve.Evaluate(AngleOfAttack / 180);
		float liftForce = (liftConstant * Mathf.Pow(CurrentSpeed, 2)) * pitchFactor;
		return liftForce;
	}
	Vector3 CalculateDragVector ()
	{
		Vector3 velocity = transform.InverseTransformVector(rigidbody.velocity);
		Vector3 trajectory = velocity.normalized;
		Vector3 dragTrajectory = -trajectory;

		float rotationalDistance = Vector3.Angle(Vector3.back, dragTrajectory);
		float dragAngleFactor = rotationalDistance / 180;

		float dragFactor = (dragAngleFactor * (backwardsDragConstant - optimalDragConstant) + optimalDragConstant);

		float parasiticDragMagnitude = dragFactor * Mathf.Pow(velocity.magnitude, 2);
		float inducedDragMagnitude = AngleOfAttack * inducedDragConstant;

		if (velocity.magnitude == 0f)
			inducedDragMagnitude = 0;

		DragMagnitude = inducedDragMagnitude + parasiticDragMagnitude;

		return dragTrajectory * DragMagnitude;
	}
}
