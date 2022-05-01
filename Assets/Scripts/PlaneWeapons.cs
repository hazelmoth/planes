using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneWeapons : MonoBehaviour
{
    [SerializeField] private List<Gun> weapons;

    private int activeWeapon;
    private bool triggerHeld;
    private float lastShotTime;
    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponentInParent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (triggerHeld)
        {
            float secPerShot = 60 / weapons[activeWeapon].data.fireRate;
            if (Time.time - lastShotTime > secPerShot)
            {
                for(int i = 0; i < weapons[activeWeapon].launchPoints.Count; i++)
                {
                    BulletSystem.Fire(
                        weapons[activeWeapon].data.projectileAsset,
                        weapons[activeWeapon].launchPoints[i],
                        weapons[activeWeapon].data.muzzleVelocity,
                        rb != null ? rb.velocity : Vector3.zero);
                }
                lastShotTime = Time.time;
            }
        }
    }

    public void SetTrigger (bool trigger)
    {
        triggerHeld = trigger;
    }

    [System.Serializable]
    private struct Gun
    {
        public WeaponData data;
        public List<Transform> launchPoints;
    }
}
