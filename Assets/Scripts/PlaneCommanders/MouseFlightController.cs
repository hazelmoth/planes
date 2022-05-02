//
// Copyright (c) Brian Hernandez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using MFlight.Demo;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MFlight
{
    public class MouseFlightController : MonoBehaviour
    {
        const float ThrottleSpeed = .55f;

        [SerializeField] private PlaneController aircraft;
        [SerializeField] private GameObject flightRigPrefab;
        private MouseFlightRig flightRig;
        [SerializeField] private GameObject hudPrefab;
        private Hud hud;

        [Header("Autopilot")]
        [Tooltip("Sensitivity for autopilot flight.")] public float sensitivity = 4f;
        [Tooltip("Sensitivity multiplier for roll specifically.")] public float rollSensitivity = 0.5f;
        [Tooltip("Angle at which airplane banks fully into target.")] public float aggressiveTurnAngle = 20f;

        [Header("Options")]
        [SerializeField] [Tooltip("Follow aircraft.transform using fixed update loop")]
        private bool useFixed = true;

        [SerializeField] [Tooltip("How quickly the camera tracks the mouse aim point.")]
        private float camSmoothSpeed = 5f;

        [SerializeField] [Tooltip("Mouse sensitivity for the mouse flight target")]
        private float mouseSensitivity = 3f;

        [SerializeField] [Tooltip("How far the boresight and mouse flight are from the aircraft.transform")]
        private float aimDistance = 500f;

        [Space]
        [SerializeField] [Tooltip("How far the boresight and mouse flight are from the aircraft.transform")]
        private bool showDebugInfo = false;

        private Vector3 frozenDirection = Vector3.forward;
        private bool isMouseAimFrozen = false;
        private float currentThrottle = 0f;

        private PidController yawController;
        private PidController pitchController;
        private PidController rollController;

        public PlaneController ActivePlane => aircraft;
        public float CurrentThrottle => currentThrottle;

        /// <summary>
        /// Get a point along the aircraft.transform's boresight projected out to aimDistance meters.
        /// Useful for drawing a crosshair to aim fixed forward guns with, or to indicate what
        /// direction the aircraft.transform is pointed.
        /// </summary>
        public Vector3 BoresightPos
        {
            get
            {
                return aircraft.transform == null
                     ? flightRig.transform.forward * aimDistance
                     : (aircraft.transform.transform.forward * aimDistance) + aircraft.transform.transform.position;
            }
        }

        /// <summary>
        /// Get the position that the mouse is indicating the aircraft.transform should fly, projected
        /// out to aimDistance meters. Also meant to be used to draw a mouse cursor.
        /// </summary>
        public Vector3 MouseAimPos
        {
            get
            {
                if (flightRig.MouseAim != null)
                {
                    return isMouseAimFrozen
                        ? GetFrozenMouseAimPos()
                        : flightRig.MouseAim.position + (flightRig.MouseAim.forward * aimDistance);
                }
                else
                {
                    return flightRig.transform.forward * aimDistance;
                }
            }
        }

        private void Awake()
        {
            if (flightRig == null)
                flightRig = Instantiate(flightRigPrefab, transform, true).GetComponent<MouseFlightRig>();

            if (hud == null)
                hud = Instantiate(hudPrefab, transform, true).GetComponent<Hud>();
            hud.SetReferenceMouseFlight(this);


            if (flightRig.MouseAim == null)
                Debug.LogError(name + "MouseFlightController - No mouse aim transform assigned!");
            if (flightRig.CameraRig == null)
                Debug.LogError(name + "MouseFlightController - No camera rig transform assigned!");
            if (flightRig.Cam == null)
                Debug.LogError(name + "MouseFlightController - No camera transform assigned!");

            yawController = new PidController(0.45f,   0.04f, 0.20f);
            pitchController = new PidController(0.45f, 0.04f, 0.20f);
            rollController = new PidController(0.45f,  0.04f, 0.20f);

        }

        private void LateUpdate()
        {
            Cursor.visible = aircraft == null;
            Cursor.lockState = aircraft == null ? CursorLockMode.None : CursorLockMode.Locked;
            if (aircraft == null) return;
            if (!useFixed) UpdateCameraPos();

            RotateRig();

            // Throttle input
            currentThrottle += Input.GetAxis("Throttle") * ThrottleSpeed * Time.deltaTime;
            currentThrottle = Mathf.Clamp01(currentThrottle);
            aircraft.SetThrottle(currentThrottle);

            // Mouse follow input
            CalculateControls(MouseAimPos, out float yaw, out float pitch, out float roll);
            aircraft.SetRudderAmount(yaw);
            aircraft.SetElevatorAmount(pitch);
            aircraft.SetAileronAmount(roll);

            // Other input
            if (aircraft.Weapons != null) aircraft.Weapons.SetTrigger(Input.GetButton("Fire"));

            if (Input.GetKeyDown(KeyCode.G))
                aircraft.ToggleLandingGear();
            if (Input.GetKeyDown(KeyCode.Tab))
                SceneManager.LoadScene(0);
        }

        private void FixedUpdate()
        {
            if (aircraft == null) return;
            if (useFixed) UpdateCameraPos();
        }

        private void RotateRig()
        {
            if (flightRig.MouseAim == null || flightRig.CameraRig == null || flightRig.Cam == null)
                return;

            // Freeze the mouse aim direction when the free look key is pressed.
            if (Input.GetKeyDown(KeyCode.C))
            {
                isMouseAimFrozen = true;
                frozenDirection = flightRig.MouseAim.forward;
            }
            else if  (Input.GetKeyUp(KeyCode.C))
            {
                isMouseAimFrozen = false;
                flightRig.MouseAim.forward = frozenDirection;
            }

            // Mouse input.
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = -Input.GetAxis("Mouse Y") * mouseSensitivity;

            // Rotate the aim target that the plane is meant to fly towards.
            // Use the camera's axes in world space so that mouse motion is intuitive.
            flightRig.MouseAim.Rotate(flightRig.Cam.right, mouseY, Space.World);
            flightRig.MouseAim.Rotate(flightRig.Cam.up, mouseX, Space.World);

            // The up vector of the camera normally is aligned to the horizon. However, when
            // looking straight up/down this can feel a bit weird. At those extremes, the camera
            // stops aligning to the horizon and instead aligns to itself.
            Vector3 upVec = (Mathf.Abs(flightRig.MouseAim.forward.y) > 0.9f) ? flightRig.CameraRig.up : Vector3.up;

            // Smoothly rotate the camera to face the mouse aim.
            flightRig.CameraRig.rotation = Damp(flightRig.CameraRig.rotation,
                                      Quaternion.LookRotation(flightRig.MouseAim.forward, upVec),
                                      camSmoothSpeed,
                                      Time.deltaTime);
        }

        private Vector3 GetFrozenMouseAimPos()
        {
            if (flightRig.MouseAim != null)
                return flightRig.MouseAim.position + (frozenDirection * aimDistance);
            else
                return flightRig.transform.forward * aimDistance;
        }

        private void UpdateCameraPos()
        {
            if (aircraft.transform != null)
            {
                // Move the whole rig to follow the aircraft
                flightRig.transform.position = aircraft.transform.position;
            }
        }

        private void CalculateControls(Vector3 flyTarget, out float yaw, out float pitch, out float roll)
        {
            // This is my usual trick of converting the fly to position to local space.
            // You can derive a lot of information from where the target is relative to self.
            Vector3 localFlyTarget = aircraft.transform.InverseTransformPoint(flyTarget).normalized * sensitivity;
            float angleOffTarget = Vector3.Angle(aircraft.transform.forward, flyTarget - aircraft.transform.position);


            // ====================
            // ROLL
            // ====================

            // Roll is a little special because there are two different roll commands depending
            // on the situation. When the target is off axis, then the plane should roll into it.
            // When the target is directly in front, the plane should fly wings level.

            // An "aggressive roll" is input such that the aircraft rolls into the target so
            // that pitching up (handled above) will put the nose onto the target. This is
            // done by rolling such that the X component of the target's position is zeroed.
            float aggressiveRoll = Mathf.Clamp(localFlyTarget.x * rollSensitivity, -1f, 1f);

            // A "wings level roll" is a roll commanding the aircraft to fly wings level.
            // This can be done by zeroing out the Y component of the aircraft's right.
            float wingsLevelRoll = aircraft.transform.right.y * rollSensitivity;

            // Blend between auto level and banking into the target.
            float wingsLevelInfluence = Mathf.InverseLerp(0f, aggressiveTurnAngle, angleOffTarget);
            // roll = Mathf.Lerp(wingsLevelRoll, aggressiveRoll, wingsLevelInfluence);
            roll = Mathf.Clamp(
                rollController.Update(Time.time, Mathf.Lerp(wingsLevelRoll, aggressiveRoll, wingsLevelInfluence)),
                -1f,
                1f);

            // ====================
            // PITCH AND YAW
            // ====================

            // Yaw/Pitch into the target so as to put it directly in front of the aircraft.
            // A target is directly in front the aircraft if the relative X and Y are both
            // zero. Note this does not handle for the case where the target is directly behind.
            //yaw = Mathf.Clamp(localFlyTarget.x, -1f, 1f);
            //pitch = Mathf.Clamp(localFlyTarget.y, -1f, 1f);
            yaw = Mathf.Clamp(yawController.Update(Time.time, localFlyTarget.x),     -1f, 1f);

            pitch = Mathf.Clamp(localFlyTarget.y, -1f, 1f);
            // Reduce pitch change when performing an aggressive roll.
            pitch -= wingsLevelInfluence * Mathf.Abs(aggressiveRoll) * Mathf.Sign(pitch);
            pitch = Mathf.Clamp(pitch, -1f, 1f);
            pitch = Mathf.Clamp(pitchController.Update(Time.time, pitch), -1f, 1f);
        }

        // Thanks to Rory Driscoll
        // http://www.rorydriscoll.com/2016/03/07/frame-rate-independent-damping-using-lerp/
        /// <summary>
        /// Creates dampened motion between a and b that is framerate independent.
        /// </summary>
        /// <param name="a">Initial parameter</param>
        /// <param name="b">Target parameter</param>
        /// <param name="lambda">Smoothing factor</param>
        /// <param name="dt">Time since last damp call</param>
        /// <returns></returns>
        private Quaternion Damp(Quaternion a, Quaternion b, float lambda, float dt)
        {
            return Quaternion.Slerp(a, b, 1 - Mathf.Exp(-lambda * dt));
        }

        private void OnDrawGizmos()
        {
            if (showDebugInfo == true)
            {
                Color oldColor = Gizmos.color;

                // Draw the boresight position.
                if (aircraft.transform != null)
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawWireSphere(BoresightPos, 10f);
                }

                if (flightRig.MouseAim != null)
                {
                    // Draw the position of the mouse aim position.
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(MouseAimPos, 10f);

                    // Draw axes for the mouse aim transform.
                    Gizmos.color = Color.blue;
                    Gizmos.DrawRay(flightRig.MouseAim.position, flightRig.MouseAim.forward * 50f);
                    Gizmos.color = Color.green;
                    Gizmos.DrawRay(flightRig.MouseAim.position, flightRig.MouseAim.up * 50f);
                    Gizmos.color = Color.red;
                    Gizmos.DrawRay(flightRig.MouseAim.position, flightRig.MouseAim.right * 50f);
                }

                Gizmos.color = oldColor;
            }
        }
    }
}
