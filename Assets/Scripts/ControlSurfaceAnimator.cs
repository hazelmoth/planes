using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlSurfaceAnimator : MonoBehaviour
{
	[SerializeField] GameObject leftElevator;
	[SerializeField] GameObject rightElevator;
	[SerializeField] GameObject leftAileron;
	[SerializeField] GameObject rightAileron;
	[SerializeField] GameObject rudder;

	// how many degrees the surfaces can rotate in either direction
	float elevatorRange = 27f;
	float aileronRange = 48f;
	float rudderRange = 26f;

	Quaternion leftAileronBaseRot;
	Quaternion rightAileronBaseRot;
	Quaternion leftElevatorBaseRot;
	Quaternion rightElevatorBaseRot;
	Quaternion rudderBaseRot;

	private void Start()
	{
		leftAileronBaseRot = leftAileron.transform.localRotation;
		rightAileronBaseRot = rightAileron.transform.localRotation;
		leftElevatorBaseRot = leftElevator.transform.localRotation;
		rightElevatorBaseRot = rightElevator.transform.localRotation;
		rudderBaseRot = rudder.transform.localRotation;
	}
	public void SetElevatorAmount (float amount)
	{
		float degrees = amount * elevatorRange;
		leftElevator.transform.localRotation = leftElevatorBaseRot;
		rightElevator.transform.localRotation = rightElevatorBaseRot;
		leftElevator.transform.Rotate(Vector3.right, degrees, Space.Self);
		rightElevator.transform.Rotate(Vector3.right, degrees, Space.Self);
	}
	public void SetAileronAmount (float amount)
	{
		float degrees = amount * aileronRange;
		leftAileron.transform.localRotation = leftAileronBaseRot;
		rightAileron.transform.localRotation = rightAileronBaseRot;
		leftAileron.transform.Rotate(Vector3.right, -degrees, Space.Self);
		rightAileron.transform.Rotate(Vector3.right, degrees, Space.Self);
	}
	public void SetRudderAmount (float amount)
	{
		float degrees = amount * rudderRange;
		rudder.transform.localRotation = rudderBaseRot;
		// Vector3.forward because blender meshes are rotated by 90 degrees on x
		rudder.transform.Rotate(Vector3.forward, -degrees, Space.Self);
	}
}
