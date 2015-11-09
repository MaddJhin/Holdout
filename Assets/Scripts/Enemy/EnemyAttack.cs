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
	public float damage = 5f;

    [HideInInspector]
	public float stunDuration = 1f;

    [HideInInspector]
	public float attackRadius = 5f;

	private ParticleSystem explosionFX;                 // Stores the instance of the explosion Particle System

	public void Punch(GameObject target)
	{
		UnitStats targetHealth = target.GetComponent<UnitStats>();
		targetHealth.TakeDamage(damage);
		PunchEffects();
	}

	public void Slam(GameObject target)
	{		
		AreaOfEffect aoe = new AreaOfEffect();
		aoe.AreaStun(target.transform.position, attackRadius, damage, stunDuration, gameObject);	
		SlamEffects();
	}
	
	public void Explode(GameObject target)
	{		
		AreaOfEffect aoe = new AreaOfEffect();
		aoe.AreaExplode(target.transform.position, attackRadius, damage, gameObject);
		
		DestructEffects();
		Debug.Log("BOOM!");
	}
	
	// Audio and Visual effects for selfDestruct
	void DestructEffects()
	{
		explosionFX.transform.position = gameObject.transform.position;
		explosionFX.Play();
	}

	// Audio and Visual effects for punching
	void SlamEffects()
	{
		
	}

	// Audio and Visual effects for punching
	void PunchEffects()
	{
		
	}
}
