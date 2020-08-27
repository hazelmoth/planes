using UnityEngine;

[CreateAssetMenu(menuName = "Content/Weapon Data")]
public class WeaponData : ScriptableObject
{
	public new string name;
	public GameObject projectileAsset;
	public bool automatic = true;

	[LabelOverride("Fire Rate (RPM)")]
	public float fireRate;
	public float muzzleVelocity;
}