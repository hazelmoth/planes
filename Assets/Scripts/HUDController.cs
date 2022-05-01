using System;
using System.Collections;
using System.Collections.Generic;
using MFlight;
using UnityEngine;
using TMPro;

public class HUDController : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI speedText;
	[SerializeField] private TextMeshProUGUI vertSpeedText;
	[SerializeField] private TextMeshProUGUI throttleText;
	[SerializeField] private TextMeshProUGUI thrustText;
	[SerializeField] private TextMeshProUGUI altitudeText;
	[SerializeField] private TextMeshProUGUI dragAmountText;
	[SerializeField] private TextMeshProUGUI inducedDragText;
	[SerializeField] private TextMeshProUGUI parasiticDragText;
	[SerializeField] private TextMeshProUGUI angleOfAttackText;
	[SerializeField] private TextMeshProUGUI pitchText;
	[SerializeField] private TextMeshProUGUI rollText;
	[SerializeField] private TextMeshProUGUI headingText;
	[SerializeField] private MouseFlightController input;

    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
		if (input.ActivePlane != null)
		{
			speedText.text = Math.Round(input.ActivePlane.CurrentSpeed,                                  1) + " m/s";
			vertSpeedText.text = "vert: " + Math.Round(input.ActivePlane.VerticalSpeed,                  1) + " m/s";
			thrustText.text = "thrust: " + Math.Round(input.ActivePlane.ThrustMagnitude,                 1) + " N";
			altitudeText.text = "altitude: " + Math.Round(input.ActivePlane.Altitude,                    0) + " m";
			dragAmountText.text = "drag:" + Math.Round(input.ActivePlane.DragMagnitude,                  0) + " N";
			inducedDragText.text = "induced:" + Math.Round(input.ActivePlane.InducedDragMagnitude,       0) + " N";
			parasiticDragText.text = "parasitic:" + Math.Round(input.ActivePlane.ParasiticDragMagnitude, 0) + " N";
			angleOfAttackText.text = Math.Round(input.ActivePlane.AngleOfAttack, 1)
				+ "° AoA, "
				+ Math.Round(input.ActivePlane.SideslipAngle, 1)
				+ "° sideslip";
			pitchText.text = "pitch: " + Math.Round(input.ActivePlane.Pitch,       1) + "°";
			rollText.text = "roll: " + Math.Round(input.ActivePlane.Roll,          1) + "°";
			headingText.text = "heading: " + Math.Round(input.ActivePlane.Heading, 1) + "°";
		}
		throttleText.text = Mathf.Round(input.CurrentThrottle * 100) + "% throttle";
    }
}
