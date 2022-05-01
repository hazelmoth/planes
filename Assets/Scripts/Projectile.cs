using UnityEngine;

/// Detects collisions and triggers the explosion effect.
public class Projectile : MonoBehaviour
{
    [SerializeField] private GameObject visualEffect;

    private void OnTriggerEnter(Collider other)
    {
        // Instantiate the explosion effect.
        GameObject explosion = Instantiate(visualEffect, transform.position, transform.rotation);
        Destroy(explosion, 4f);
        Destroy(gameObject);

        IProjectileReceiver receiver = other.GetComponentInParent<IProjectileReceiver>();
        receiver?.TakeProjectile(this);
    }
}
