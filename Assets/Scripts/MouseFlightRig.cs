using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseFlightRig : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] [Tooltip("Transform of the object the mouse rotates to generate MouseAim position")]
    private Transform mouseAim = null;
    [SerializeField] [Tooltip("Transform of the object on the rig which the camera is attached to")]
    private Transform cameraRig = null;
    [SerializeField] [Tooltip("Transform of the camera itself")]
    private Transform cam = null;

    public Transform MouseAim { get => mouseAim; set => mouseAim = value; }
    public Transform CameraRig { get => cameraRig; set => cameraRig = value; }
    public Transform Cam { get => cam; set => cam = value; }
}
