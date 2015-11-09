using UnityEngine;
using System.Collections;


/* USAGE:
 * ===========================
 * Used to define shared attributes & actions of in-game units
 * Allows for easy number tweaking for the design team
 * Used by specific unit scripts to enable stats for that unit
 * ===========================
 * 
 * Date Created: 21 May 2015
 * Last Modified: 25 May 2015
 * Authors: Andrew Tully
 */


public class UnitStats : MonoBehaviour 
{
    // Unit attributes
    [HideInInspector]
    public float maxHealth;
    public float currentHealth;

    [HideInInspector]
    public float attackSpeed;

    [HideInInspector]
    public float attackRange;

    [HideInInspector]
    public bool stunImmunity;

    public enum statusEffects { stun };

    private GameManager gm;
    private PlayerControlMedic[] availableMedics;

    void Awake()
    {
        currentHealth = maxHealth;
        gm = GameObject.FindObjectOfType<GameManager>();
        
    }

    void Start()
    {
        availableMedics = FindObjectsOfType<PlayerControlMedic>();
    }

	void Update () 
    {
        if (currentHealth <= 0)
        {
            Debug.Log("Killing Unit");
            //KillUnit();
        }

        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
        
        if (currentHealth < maxHealth)
        {
            Debug.Log("Healing Required");

            foreach (var medic in availableMedics)
            {
                Debug.Log("Checking for Medics");
                Debug.Log(availableMedics);
                if (Vector3.Distance(transform.position, medic.transform.position) < medic.healRange && medic.m_Healing == false)
                {
                    medic.healTarget = this;
                    medic.m_Healing = true;
                    medic.StartCoroutine("Heal");
                    Debug.Log("Polling Medic");
                    break;
                }
            }
        }
	}

    public void TakeDamage(float damageTaken)
    {
        currentHealth -= damageTaken;
    }

	public void Heal(float healAmount)
	{
        if (currentHealth < maxHealth)
        {
            currentHealth += healAmount;
        }
	}

    public void KillUnit()
    {
        // Deactivates the unit
        Debug.Log("Removing Unit");
        gameObject.SetActive(false);
		gm.RemoveObjective();
    }

    public void ApplyStatus(statusEffects effect, float duration)
    {
        if (effect == statusEffects.stun)
        {
            StartCoroutine(ActivateStun(duration));
        }
    }

    IEnumerator ActivateStun(float duration)
    {
        yield return null;
        yield return new WaitForSeconds(duration);
    }
}
