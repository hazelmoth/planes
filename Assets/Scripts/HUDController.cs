using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HUDController : MonoBehaviour
{
	PlaneController planeToDisplay;
	[SerializeField] TextMeshProUGUI speedText;
	[SerializeField] TextMeshProUGUI vertSpeedText;
	[SerializeField] TextMeshProUGUI throttleText;
	[SerializeField] TextMeshProUGUI altitudeText;
	[SerializeField] TextMeshProUGUI dragAmountText;
	[SerializeField] TextMeshProUGUI angleOfAttackText;
	[SerializeField] TextMeshProUGUI pitchText;
	[SerializeField] TextMeshProUGUI rollText;
	[SerializeField] TextMeshProUGUI headingText;
	[SerializeField] PlayerPlaneInput input;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
		if (planeToDisplay != null)
		{
			speedText.text = System.Math.Round(input.ActivePlane.CurrentSpeed, 1) + " m/s";
			altitudeText.text = "altitude: " + System.Math.Round(input.ActivePlane.Altitude, 0) + " m";
			vertSpeedText.text = "vert: " + System.Math.Round(input.ActivePlane.VerticalSpeed, 1) + " m/s";
			dragAmountText.text = "drag:" + System.Math.Round(input.ActivePlane.DragMagnitude, 0);
			angleOfAttackText.text = System.Math.Round(input.ActivePlane.AngleOfAttack, 1) + "° AoA";
			pitchText.text = "pitch: " + System.Math.Round(input.ActivePlane.Pitch, 1) + "°";
			rollText.text = "roll: " + System.Math.Round(input.ActivePlane.Roll, 1) + "°";
			headingText.text = "heading: " + System.Math.Round(input.ActivePlane.Heading, 1) + "°";
		}
		throttleText.text = (Mathf.Round(input.currentThrottle * 100)).ToString() + "% throttle";
    }
}
