using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandingGearController : MonoBehaviour
{
	[SerializeField] GameObject rightGear;
	[SerializeField] GameObject leftGear;
	[SerializeField] GameObject rightGearCover;
	[SerializeField] GameObject leftGearCover;
	[SerializeField] bool retractsOutward = false;
	[SerializeField] float deployedGearXAngle = 90f;
	[SerializeField] float deployedGearYAngle = 25f;
	[SerializeField] float deployedGearCoverAngle = 100f;
	[SerializeField] float gearDeployTime = 1f;

	Vector3 leftGearStartRot;
	Vector3 rightGearStartRot;
	Vector3 leftCoverStartRot;
	Vector3 rightCoverStartRot;

	Vector3 leftGearTargetRot;
	Vector3 rightGearTargetRot;
	Vector3 leftCoverTargetRot;
	Vector3 rightCoverTargetRot;

	float currentAnimStartTime;
	float currentAnimStartProgress;

	public GearState State { get; private set; }

	private void Start()
	{
		//State = GearState.Deployed;
		//SetDeployed(true);
	}
	public enum GearState
	{
		Deployed,
		Retracted,
		Deploying,
		Retracting
	}

	public void SetDeployed (bool doDeploy)
	{
		leftGearStartRot = leftGear.transform.localRotation.eulerAngles;
		rightGearStartRot = rightGear.transform.localRotation.eulerAngles;
		if (leftGearCover)
		{
			leftCoverStartRot = leftGearCover.transform.localRotation.eulerAngles;
		}
		if (rightGearCover)
		{
			rightCoverStartRot = rightGearCover.transform.localRotation.eulerAngles;
		}

		//If we are currently moving in the opposite direction of the direction we wanna move
		if (State == GearState.Retracting && doDeploy || State == GearState.Deploying && !doDeploy)
		{
			currentAnimStartProgress = (gearDeployTime - (Time.time - currentAnimStartTime)) / gearDeployTime;
		}
		else
		{
			currentAnimStartProgress = 0f;
		}

		if (doDeploy)
		{
			if (State == GearState.Deploying || State == GearState.Deployed)
				return;
			State = GearState.Deploying;

			float xAngle = retractsOutward ? -deployedGearXAngle : deployedGearXAngle;

			leftGearTargetRot = new Vector3(deployedGearYAngle, 0, -xAngle);
			rightGearTargetRot = new Vector3(deployedGearYAngle, 0, xAngle);
			leftCoverTargetRot = new Vector3(0, 0, -deployedGearCoverAngle);
			rightCoverTargetRot = new Vector3(0, 0, deployedGearCoverAngle);
		}
		else
		{
			if (State == GearState.Retracting || State == GearState.Retracted)
				return;
			State = GearState.Retracting;
			leftGearTargetRot = Vector3.zero;
			rightGearTargetRot = Vector3.zero;
			leftCoverTargetRot = Vector3.zero;
			rightCoverTargetRot = Vector3.zero;
		}
		currentAnimStartTime = Time.time;
	}

    // Update is called once per frame
    void Update()
    {
		// check if we should currently be animating
		if (Time.time - currentAnimStartTime <= gearDeployTime && currentAnimStartTime > 0)
		{
			float animProgress = (Time.time - currentAnimStartTime) / (gearDeployTime * (1 - currentAnimStartProgress));
			leftGear.transform.localRotation = Quaternion.Lerp(Quaternion.Euler(leftGearStartRot), Quaternion.Euler(leftGearTargetRot), animProgress);
			rightGear.transform.localRotation = Quaternion.Lerp(Quaternion.Euler(rightGearStartRot), Quaternion.Euler(rightGearTargetRot), animProgress);
			if (leftGearCover)
				leftGearCover.transform.localRotation = Quaternion.Lerp(Quaternion.Euler(leftCoverStartRot), Quaternion.Euler(leftCoverTargetRot), animProgress);
			if (rightGearCover)
				rightGearCover.transform.localRotation = Quaternion.Lerp(Quaternion.Euler(rightCoverStartRot), Quaternion.Euler(rightCoverTargetRot), animProgress);
		}
		else if (State == GearState.Deploying)
			State = GearState.Deployed;
		else if (State == GearState.Retracting)
			State = GearState.Retracted;
    }
}
