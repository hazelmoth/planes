﻿using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BulletSystem : MonoBehaviour
{
    private const float FlyTime = 5f;

    private static BulletSystem instance;

    private List<BulletData> bullets;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        bullets = new List<BulletData>();
    }

    // Update is called once per frame
    void Update()
    {
        int toRemove = 0; // How many bullets have exceeded their lifespan

        foreach (BulletData bullet in bullets)
        {
            if (bullet.bullet == null)
            {
                toRemove++;
                continue;
            }
            if (Time.time - bullet.startTIme > FlyTime)
            {
                Destroy(bullet.bullet);
                toRemove++;
            }
            else
            {
                if (bullet.rb != null) bullet.rb.velocity = bullet.velocity;
                else bullet.bullet.transform.Translate(bullet.velocity * Time.deltaTime, Space.World);
            }
        }
        bullets.RemoveRange(0, toRemove); // Assumes that bullets are ordered by age
    }

    public static void Fire (GameObject bulletPrefab, Transform origin, float muzzleVelocity, Vector3 parentVelocity)
    {
        GameObject bullet = GameObject.Instantiate(bulletPrefab, origin.position, origin.rotation, instance.transform);
        instance.bullets.Add(new BulletData (bullet, parentVelocity + muzzleVelocity * bullet.transform.forward, Time.time));
    }

    private struct BulletData
    {
        public GameObject bullet;
        public Rigidbody rb;
        public Vector3 velocity;
        public float startTIme;

        public BulletData(GameObject bullet, Vector3 velocity, float startTIme)
        {
            this.bullet = bullet;
            this.rb = bullet.GetComponent<Rigidbody>();
            this.velocity = velocity;
            this.startTIme = startTIme;
        }
    }
}
