using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanePropellor : MonoBehaviour
{
	public float maxRpm = 2250f;
	public float idleRpm = 120f;
	float currentRpm = 0;

    // Start is called before the first frame update
    void Start()
    {
		currentRpm = idleRpm;
    }

    // Update is called once per frame
    void Update()
    {
		float degToTurn = currentRpm * 360 / 60 * Time.deltaTime;
		this.transform.Rotate(Vector3.down, degToTurn, Space.Self);
    }
	public void SetPower (float power)
	{
		currentRpm = power * (maxRpm - idleRpm) + idleRpm;
	}
}
