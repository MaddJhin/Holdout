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

public class EnemyAttack : MonoBehaviour
{
    Animation m_Animation;

    void Awake()
    {
        m_Animation = GetComponentInChildren<Animation>();
    }

    public void Punch(GameObject target, float damagePerHit)
	{
		UnitStats targetHealth = target.GetComponent<UnitStats>();
        m_Animation.CrossFade("Attack");
        targetHealth.TakeDamage(damagePerHit);
	}

    public void Shoot(GameObject target, float damagePerHit)
    {
        UnitStats targetHealth = target.GetComponent<UnitStats>();
        m_Animation.CrossFade("Attack");
        targetHealth.TakeDamage(damagePerHit);
    }

	public void Slam(GameObject target, int attackMask, float damagePerHit, float stunDuration, float attackRadius)
	{		
		AreaOfEffect aoe = new AreaOfEffect();
        m_Animation.CrossFade("Attack");
        aoe.AreaStun(target.transform.position, attackRadius, damagePerHit, stunDuration, gameObject, attackMask);	
	}
	
	public void Explode(GameObject target, int attackMask, float damagePerHit, float attackRadius)
	{		
		AreaOfEffect aoe = new AreaOfEffect();
        aoe.AreaExplode(target.transform.position, attackRadius, damagePerHit, gameObject, attackMask);
	}
}
