using UnityEngine;
using System.Collections;

/* USED BY:
 * ==============
 * EnemyControlBob.cs
 * EnemyControlFlyer.cs
 * EnemyControlMinion.cs
 * EnemyControlBrute.cs
 * ==============
 * 
 * USAGE:
 * ======================================
 * Contains the attack methods used by the different
 * enemy control scripts. 
 * Enables modularity
 * ======================================
 * 
 * Date Created: 6 Aug 2015
 * Last Modified: 8 Aug 2015
 * Authors: Francisco Carrera, Andrew Tully
 */

public class EnemyAttack : MonoBehaviour {

    [HideInInspector]
	public float damage;

    [HideInInspector]
	public float stunDuration = 1f;

    [HideInInspector]
	public float attackRadius;

	public void Punch(GameObject target)
	{
		UnitStats targetHealth = target.GetComponent<UnitStats>();
		targetHealth.TakeDamage(damage);
	}

    public void Shoot(GameObject target, float damagePerHit)
    {
        UnitStats targetHealth = target.GetComponent<UnitStats>();
        targetHealth.TakeDamage(damagePerHit);
    }

	public void Slam(GameObject target, int attackMask)
	{		
		AreaOfEffect aoe = new AreaOfEffect();
		aoe.AreaStun(target.transform.position, attackRadius, damage, stunDuration, gameObject, attackMask);	
	}
	
	public void Explode(GameObject target, int attackMask)
	{		
		AreaOfEffect aoe = new AreaOfEffect();
		aoe.AreaExplode(target.transform.position, attackRadius, damage, gameObject, attackMask);
	}
}
