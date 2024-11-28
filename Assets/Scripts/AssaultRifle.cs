using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssaultRifle : MonoBehaviour
{
    // Enum to define firing modes
    public enum FireMode { Single, Automatic }

    [Header("Gun Settings")]
    public GameObject bulletPrefab;         // Bullet prefab to instantiate
    public Transform firePoint;             // Position where the bullet will spawn
    public float bulletSpeed = 20f;         // Speed of the bullet
    public float fireRate = 0.5f;           // Time between shots
    public float range = 100f;               // Max range for raycast
    public float damage = 20f;               // Damage dealt by the bullet
    public Camera playerCamera;              // Reference to the camera
    //public ParticleSystem muzzleFlash;       // Muzzle flash effect
    //public GameObject hitEffect;             // Effect on hit (e.g., explosion)

    [Header("Firing Mode Settings")]
    public FireMode fireMode = FireMode.Single;

    private float nextFireTime = 0f;        // Tracks when the gun can fire next

    void Update()
    {
        // Toggle between single and auto fire modes
        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleFireMode();
        }

        // Handle shooting based on the current fire mode
        if (fireMode == FireMode.Single)
        {
            if (Input.GetButtonDown("Fire1") && Time.time >= nextFireTime)
            {
                nextFireTime = Time.time + fireRate;
                Shoot();
            }
        }
        else if (fireMode == FireMode.Automatic)
        {
            if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
            {
                nextFireTime = Time.time + fireRate;
                Shoot();
            }
        }
    }

    void Shoot()
    {
        // Play muzzle flash effect
        //if (muzzleFlash != null)
        //{
        //    muzzleFlash.Play();
        //}

        // Instantiate the bullet at the firePoint position and rotation
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        // Get the Rigidbody component from the bullet to apply velocity
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        if (bulletRb != null)
        {
            // Set bullet velocity
            bulletRb.velocity = firePoint.forward * bulletSpeed;
        }

        // Perform raycast to detect hit
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, range))
        {
            // Log the name of the object hit
            Debug.Log("Hit: " + hit.collider.name);
            // Instantiate hit effect if it exists
            //if (hitEffect != null)
            //{
            //    Instantiate(hitEffect, hit.point, Quaternion.LookRotation(hit.normal));
            //}
        }
    }

    void ToggleFireMode()
    {
        // Switch between Single and Automatic fire modes
        fireMode = (fireMode == FireMode.Single) ? FireMode.Automatic : FireMode.Single;
        Debug.Log("Fire Mode: " + fireMode);
    }
}