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

    public void Shoot(UnitStats targetHealth)
    {
        gameObject.transform.LookAt(targetHealth.gameObject.transform);

        if (targetHealth != null)
        {
            // ... the enemy should take damage.
            targetHealth.TakeDamage(damagePerHit);
        }
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
            healTarget.Heal(healAmount);
        }
    }
}
