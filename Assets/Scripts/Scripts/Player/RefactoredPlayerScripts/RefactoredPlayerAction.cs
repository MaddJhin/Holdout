using UnityEngine;
using System.Collections;

public class RefactoredPlayerAction : MonoBehaviour 
{

    public Transform shootPoint;

    #region Component References
    Ray shootRay;                                   // A ray from the gun end forwards.
    RaycastHit shootHit;                            // A raycast hit to get information about what was hit.
    int shootableMask;                              // A layer mask so the raycast only hits things on the shootable layer.
    ParticleSystem gunParticles;                    // Reference to the particle system.
    LineRenderer gunLine;                           // Reference to the line renderer.
    AudioSource gunAudio;                           // Reference to the audio source.
    Light gunLight;                                 // Reference to the light component.
    #endregion

    #region Caches & Object References

    [HideInInspector]
    public UnitStats enemyHealthCache;

    [HideInInspector]
    public GameObject actionTarget;

    [HideInInspector]
    public float damagePerHit;

    [HideInInspector]
    public float healPerHit;

    [HideInInspector]
    public float attackRange;

    [HideInInspector]
    public float healRange;

    #endregion

    void Awake()
    {
        // Create a layer mask for the Shootable layer.
        shootableMask = LayerMask.GetMask("Enemy");

        // Set up the references.
        gunParticles = GetComponent<ParticleSystem>();
        gunLine = GetComponentInChildren<LineRenderer>();
        gunAudio = GetComponent<AudioSource>();
        gunLight = GetComponentInChildren<Light>();
    }

	// Use this for initialization
	void Start () 
    {
	    
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}

    public void Shoot(UnitStats targetHealth)
    {
        Debug.Log("Shooting at target");
        gameObject.transform.LookAt(targetHealth.gameObject.transform);

        if (targetHealth != null)
        {
            // ... the enemy should take damage.
            Debug.Log(targetHealth.currentHealth);
            targetHealth.TakeDamage(damagePerHit);
            Debug.Log(targetHealth.currentHealth);
        }
    }

    public void OldShoot(UnitStats targetHealth)
    {
        Debug.Log("Shot at");
        // Set the shootRay so that it starts at the end of the shoot point and points forward from the barrel.
        gameObject.transform.LookAt(targetHealth.gameObject.transform);
        shootRay.origin = shootPoint.position;
        shootRay.direction = transform.forward;

        // Perform the raycast against gameobjects on the shootable layer and if it hits something...
        if (Physics.Raycast(shootRay, out shootHit, attackRange, shootableMask))
        {
            Debug.Log("Raycast successful");

            // Play the gun shot audioclip.
            gunAudio.Play();

            // Enable the light.
            gunLight.enabled = true;

            // Stop the particles from playing if they were, then start the particles.
            //gunParticles.Stop ();
            //gunParticles.Play ();

            // Enable the line renderer and set it's first position to be the end of the gun.
            gunLine.enabled = true;
            gunLine.SetPosition(0, shootPoint.position);

            if (targetHealth != null)
            {
                // ... the enemy should take damage.
                Debug.Log(targetHealth.currentHealth);
                targetHealth.TakeDamage(damagePerHit);
                Debug.Log(targetHealth.currentHealth);
            }

            else
            {
                Debug.Log("Could not deal damage");
            }

            // Set the second position of the line renderer to the point the raycast hit.
            gunLine.SetPosition(1, shootHit.point);
        }

        else
        {
            Debug.Log("Raycast Failed");
        }
        // If the raycast didn't hit anything on the shootable layer...
        // Nothing Happens

    }

    public void Attack(UnitStats targetHealth)
    {
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(damagePerHit);
        }
    }

    public void Heal(float healAmount, UnitStats healTarget)
    {
        if (healTarget != null)
        {
            Debug.Log("Applying Heal");
            healTarget.Heal(healAmount);
        }
    }
}
