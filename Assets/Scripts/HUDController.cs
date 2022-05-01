using System.Collections;
using System.Collections.Generic;
using MFlight;
using UnityEngine;
using TMPro;

public class HUDController : MonoBehaviour
{
	[SerializeField] TextMeshProUGUI speedText;
	[SerializeField] TextMeshProUGUI vertSpeedText;
	[SerializeField] TextMeshProUGUI throttleText;
	[SerializeField] TextMeshProUGUI altitudeText;
	[SerializeField] TextMeshProUGUI dragAmountText;
	[SerializeField] TextMeshProUGUI angleOfAttackText;
	[SerializeField] TextMeshProUGUI pitchText;
	[SerializeField] TextMeshProUGUI rollText;
	[SerializeField] TextMeshProUGUI headingText;
	[SerializeField] MouseFlightController input;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
		if (input.ActivePlane != null)
		{
			speedText.text = System.Math.Round(input.ActivePlane.CurrentSpeed, 1) + " m/s";
			altitudeText.text = "altitude: " + System.Math.Round(input.ActivePlane.Altitude, 0) + " m";
			vertSpeedText.text = "vert: " + System.Math.Round(input.ActivePlane.VerticalSpeed, 1) + " m/s";
			dragAmountText.text = "drag:" + System.Math.Round(input.ActivePlane.DragMagnitude, 0) + " N";
			angleOfAttackText.text = System.Math.Round(input.ActivePlane.AngleOfAttack, 1)
				+ "° AoA, "
				+ System.Math.Round(input.ActivePlane.SideslipAngle, 1)
				+ "° sideslip";
			pitchText.text = "pitch: " + System.Math.Round(input.ActivePlane.Pitch,       1) + "°";
			rollText.text = "roll: " + System.Math.Round(input.ActivePlane.Roll,          1) + "°";
			headingText.text = "heading: " + System.Math.Round(input.ActivePlane.Heading, 1) + "°";
		}
		throttleText.text = Mathf.Round(input.CurrentThrottle * 100) + "% throttle";
    }
}
