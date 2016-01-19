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

    [HideInInspector]
    public float healthPercentage;

    public enum statusEffects { stun, slow, healed};

    void Awake()
    {
        currentHealth = maxHealth;
    }

	void Update () 
    {
        if (currentHealth <= 0)
        {
            KillUnit();
        }

        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
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
        gameObject.SetActive(false);
    }

    public void ApplyStatus(statusEffects effect, float duration)
    {
        if (effect == statusEffects.stun)
        {
            StartCoroutine(ActivateStun(duration));
        }
    }

    public IEnumerator ActivateStun(float duration)
    {
        yield return null;
        yield return new WaitForSeconds(duration);
    }
}
