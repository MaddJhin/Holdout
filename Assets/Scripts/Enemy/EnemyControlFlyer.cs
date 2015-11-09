using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/* USES:
 * ==============
 * UnitSight.cs
 * UnitStats.cs
 * EnemyAttack.cs
 * ==============
 * 
 * USAGE:
 * ======================================
 * Acts as the "brain" of the Flyer unit
 * Uses other scripts to perform actions, E.G: seeing the player, attacking
 * Makes calls to other scripts to perform attacks, or to utilise stats
 * Enables modularity
 * ======================================
 * 
 * Date Created: 27 May 2015
 * Last Modified: 8 Aug 2015
 * Authors: Andrew Tully, Francisco Carrera
 */

[RequireComponent(typeof (NavMeshAgent))]
[RequireComponent(typeof (UnitSight))]
[RequireComponent(typeof (UnitStats))]
[RequireComponent(typeof (EnemyAttack))]

public class EnemyControlFlyer : MonoBehaviour 
{
	public float height = 30f;
	public float maxHealth = 100.0f;
	public float armor = 0.0f;
	public float attackSpeed = 1.0f;
	public float attackRange = 1f;
	public float damage = 5f;
	public string defaultTarget;
	public List<string> priorityList = new List<string>();

    private NavMeshAgent agent;
    private UnitStats stats;
    private UnitSight vision;
    private Vector3 targetLoc;
	private EnemyAttack action;
    private GameManager gm;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        stats = GetComponent<UnitStats>();
        vision = GetComponent<UnitSight>();
		action = GetComponent<EnemyAttack>();
        gm = GameObject.FindObjectOfType<GameManager>();
    }

	void Start (){		
		// Set values for dependant scripts. Only modify values in one script in inspector
		vision.defaultTarget = defaultTarget;
		vision.priorityList = priorityList;
		stats.maxHealth = maxHealth;
		stats.attackSpeed = attackSpeed;
		stats.attackRange = attackRange;
		action.damage = damage;
	}

    void OnEnable()
    {
        gm.AddObjective();
    }

    void Update()
    {
        // Ensures that the flyer remains at a specified height
        transform.position = new Vector3(transform.position.x, height, transform.position.z);

        // Update the target location
        targetLoc = vision.actionTarget.transform.position;
        Move();
    }

    void Move()
    {
        agent.Resume();
        agent.SetDestination(targetLoc);
    }
}
