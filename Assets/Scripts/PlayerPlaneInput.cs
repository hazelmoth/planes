using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerPlaneInput : MonoBehaviour
{
	[SerializeField] PlaneController activePlane = null;
	[SerializeField] SmoothFollow followCamera;
	[SerializeField] Camera cockpitCam;
	const float throttleSpeed = .55f;
	public float currentThrottle = 0f;

	public PlaneController ActivePlane
	{
		get { return activePlane; }
		set
		{
			activePlane = value;
			followCamera.Target = value.transform;
		}
	}

	private void Start()
	{
		followCamera.Target = activePlane.transform;
	}

	// Update is called once per frame
	void Update()
    {
		if (activePlane == null)
			return;

		currentThrottle += Input.GetAxis("Throttle") * throttleSpeed * Time.deltaTime;
		currentThrottle = Mathf.Clamp01(currentThrottle);
		activePlane.SetThrottle(currentThrottle);
		activePlane.SetElevatorAmount(Input.GetAxis("Vertical"));
		activePlane.SetAileronAmount(Input.GetAxis("Roll"));
		activePlane.SetRudderAmount(Input.GetAxis("Horizontal"));

		if (Input.GetKeyDown(KeyCode.G))
			activePlane.ToggleLandingGear();
		if (Input.GetKeyDown(KeyCode.Escape))
			SceneManager.LoadScene(0);
		if (Input.GetKeyDown(KeyCode.C))
		{
			cockpitCam.gameObject.SetActive(!cockpitCam.gameObject.activeInHierarchy);
		}
    }
}
