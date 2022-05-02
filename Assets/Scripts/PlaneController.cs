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

	private Rigidbody rigidbody;


	[SerializeField] private float enginePowerWatts = 800000;
	[SerializeField] private float staticThrust = 15000;

	[SerializeField] private float liftConstant = 20f;
	[SerializeField] private float optimalDragConstant = 10f;
	[SerializeField] private float backwardsDragConstant = 40f;
	[SerializeField] private float inducedDragConstant = 10f;
	[SerializeField] private float sidewaysLiftMultiplier = 0.25f;

	[SerializeField] private float rollTorqueConstant = 4f;
	[SerializeField] private float pitchTorqueConstant = 3f;
	[SerializeField] private float yawTorqueConstant = 1f;

	[SerializeField] private float controlSurfaceMoveSpeed = 3f;

	private float targetRudderAmount;
	private float targetAileronAmount;
	private float targetElevatorAmount;

	private float currentRudderAmount;
	private float currentAileronAmount;
	private float currentElevatorAmount;

	public PlaneWeapons Weapons => weapons;

	// Status
	public float CurrentSpeed => rigidbody.velocity.magnitude;

	public float VerticalSpeed => rigidbody.velocity.y;

	public float Altitude => transform.position.y;

	public float Pitch => 90 - Vector3.Angle(transform.forward, Vector3.up);

	public float Roll => 90 - Vector3.Angle(transform.right, Vector3.up);

	public float Heading =>
		Vector3.Angle(Vector3.ProjectOnPlane(transform.forward, Vector3.up), Vector3.forward);

	/// The angle between the velocity vector and the forward vector as projected on the
	/// aircraft's YZ plane.
	public float AngleOfAttack
	{
		get
		{
			Vector3 localVelocity = transform.InverseTransformVector(rigidbody.velocity);
			Vector3 localTrajectory = localVelocity.normalized;

			Vector3 yzTrajectory = Vector3.ProjectOnPlane(localTrajectory, Vector3.right);
			return Vector3.SignedAngle(Vector3.forward, yzTrajectory, Vector3.right);
		}
	}

	/// The aircraft's velocity vector projected onto its forward vector.
	public Vector3 ForwardVelocity => Vector3.Project(rigidbody.velocity, transform.forward);

	public float DragMagnitude { get; private set; }

	public float InducedDragMagnitude => CalculateInducedDragMagnitude();

	public float ParasiticDragMagnitude => CalculateParasiticDragMagnitude();

	public float ThrustMagnitude => CalculateThrustVector().magnitude;

	public float CurrentThrottle { get; private set; }

	/// The angle between the velocity vector and forward vector as projected on the
	/// aircraft's XZ plane.
	public float SideslipAngle
	{
		get
		{
			Vector3 localVelocity = transform.InverseTransformVector(rigidbody.velocity);
			Vector3 localTrajectory = localVelocity.normalized;

			Vector3 xzTrajectory = Vector3.ProjectOnPlane(localTrajectory, Vector3.up);
			return Vector3.SignedAngle(Vector3.forward, xzTrajectory, Vector3.up);
		}
	}

	public bool GearDeployed =>
		gear.State is LandingGearController.GearState.Deployed
			or LandingGearController.GearState.Deploying;


	// Controls
	public void SetThrottle(float power) =>
		CurrentThrottle = health != null ? health.ModifyThrottle(power) : power;
	public void SetRudderAmount(float yaw) =>
		targetRudderAmount = health != null ? health.ModifyRudder(yaw) : yaw;
	public void SetElevatorAmount(float pitch) =>
		targetElevatorAmount = health != null ? health.ModifyElevator(pitch) : pitch;
	public void SetAileronAmount(float roll) =>
		targetAileronAmount = health != null ? health.ModifyAileron(roll) : roll;

	public void SetGearDeployed (bool deployed)
	{
		deployed = health != null ? health.ModifyGearDeployed(deployed) : deployed;
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

	private void LateUpdate()
	{
		float rudderDelta = Mathf.Min(
			Mathf.Abs(targetRudderAmount - currentRudderAmount),
			Time.deltaTime * controlSurfaceMoveSpeed)
			* Mathf.Sign(targetRudderAmount - currentRudderAmount);
		float elevatorDelta = Mathf.Min(
			Mathf.Abs(targetElevatorAmount - currentElevatorAmount),
			Time.deltaTime * controlSurfaceMoveSpeed)
			* Mathf.Sign(targetElevatorAmount - currentElevatorAmount);
		float aileronDelta = Mathf.Min(
			Mathf.Abs(targetAileronAmount - currentAileronAmount),
			Time.deltaTime * controlSurfaceMoveSpeed)
			* Mathf.Sign(targetAileronAmount - currentAileronAmount);

		currentRudderAmount += rudderDelta;
		currentElevatorAmount += elevatorDelta;
		currentAileronAmount += aileronDelta;

		if (surfaces != null)
		{
			surfaces.SetAileronAmount(currentAileronAmount);
			surfaces.SetRudderAmount(currentRudderAmount);
			surfaces.SetElevatorAmount(currentElevatorAmount);
		}
	}

	// Update is called once per frame
	private void FixedUpdate()
    {
		if (CurrentThrottle < 0) CurrentThrottle = 0;

		Vector3 thrustVector = CalculateThrustVector();

		Vector3 liftVector = Vector3.up * CalculateLift();
		liftVector += Vector3.right * CalculateSidewaysLift();

		Vector3 rollTorque = Vector3.back * CalculateRollTorque();
		Vector3 yawTorque = Vector3.up * CalculateYawTorque();
		Vector3 pitchTorque = Vector3.left * CalculatePitchTorque();
		Vector3 netTorque = yawTorque + rollTorque + pitchTorque;

		Vector3 netForce = thrustVector + liftVector;

		rigidbody.AddRelativeForce(netForce);

		Vector3 dragWorldPoint = transform.TransformPoint(centerOfDrag);
		Vector3 dragWorldVector = transform.TransformVector(CalculateDragVector());
		rigidbody.AddForceAtPosition(dragWorldVector, dragWorldPoint);

		rigidbody.AddRelativeTorque(netTorque);

		propellor?.SetPower(CurrentThrottle);

		Debug.DrawRay(transform.position, transform.TransformVector(liftVector) / 100f, Color.green, Time.fixedDeltaTime);
		Debug.DrawRay(dragWorldPoint, dragWorldVector / 100f, Color.red, Time.fixedDeltaTime);
		Debug.DrawRay(transform.position, rigidbody.velocity / 10f, Color.blue, Time.fixedDeltaTime);

    }

	private Vector3 CalculateThrustVector()
	{
		float thrust = ForwardVelocity.magnitude > 0f
			? CurrentThrottle * enginePowerWatts / (ForwardVelocity.magnitude)
			: float.MaxValue;
		thrust = Mathf.Min(staticThrust, thrust);
		return Vector3.forward * thrust;
	}

	private float CalculateLift () {
		// in N
		float pitchFactor = liftCurve.Evaluate(AngleOfAttack / 180);
		float liftForce = liftConstant * Mathf.Pow(CurrentSpeed, 2) * pitchFactor;
		return liftForce;
	}

	/// Calculates lift on the x-axis from sideways flight, using a scaled down version
	/// of the normal lift curve.
	private float CalculateSidewaysLift () {
		// in N
		float pitchFactor = Mathf.Sign(SideslipAngle) * liftCurve.Evaluate(Mathf.Abs(SideslipAngle) / 180);
		float liftForce = -1f * sidewaysLiftMultiplier * (liftConstant * Mathf.Pow(CurrentSpeed, 2)) * pitchFactor;
		return liftForce;
	}

	private float CalculateRollTorque()
	{
		return Mathf.Pow(ForwardVelocity.magnitude, 2) * rollTorqueConstant * currentAileronAmount;
	}

	private float CalculatePitchTorque()
	{
		return Mathf.Pow(ForwardVelocity.magnitude, 2) * pitchTorqueConstant * currentElevatorAmount;
	}

	private float CalculateYawTorque()
	{
		return Mathf.Pow(ForwardVelocity.magnitude, 2) * yawTorqueConstant * currentRudderAmount;
	}

	private Vector3 CalculateDragVector ()
	{
		float parasiticDragMagnitude = CalculateParasiticDragMagnitude();
		float inducedDragMagnitude = CalculateInducedDragMagnitude();
		DragMagnitude = inducedDragMagnitude + parasiticDragMagnitude;

		Vector3 velocity = transform.InverseTransformVector(rigidbody.velocity);
		Vector3 trajectory = velocity.normalized;
		Vector3 dragTrajectory = -trajectory;

		return dragTrajectory * DragMagnitude;
	}

	private float CalculateParasiticDragMagnitude ()
	{
		Vector3 velocity = transform.InverseTransformVector(rigidbody.velocity);
		Vector3 trajectory = velocity.normalized;
		Vector3 dragTrajectory = -trajectory;

		float rotationalDistance = Vector3.Angle(Vector3.back, dragTrajectory);
		float dragAngleFactor = rotationalDistance / 180;

		float dragFactor = (dragAngleFactor * (backwardsDragConstant - optimalDragConstant) + optimalDragConstant);

		float parasiticDragMagnitude = dragFactor * Mathf.Pow(velocity.magnitude, 2);

		return parasiticDragMagnitude;
	}

	private float CalculateInducedDragMagnitude ()
	{
		float inducedDragMagnitude = CalculateLift() * inducedDragConstant / Mathf.Pow(rigidbody.velocity.magnitude, 4);

		if (ForwardVelocity.magnitude <= 20f)
			inducedDragMagnitude = 0;

		return inducedDragMagnitude;
	}
}
