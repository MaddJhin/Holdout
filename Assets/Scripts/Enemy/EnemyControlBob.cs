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
 * Acts as the "brain" of the bob unit
 * Uses other scripts to perform actions, E.G: seeing the player, attacking
 * Makes calls to other scripts to perform attacks, or to utilise stats
 * Enables modularity
 * ======================================
 * 
 * Date Created: 27 May 2015
 * Last Modified: 8 Aug 2015
 * Authors: Andrew Tully
 */

[RequireComponent(typeof (NavMeshAgent))]
[RequireComponent(typeof (NavMeshObstacle))]
[RequireComponent(typeof (UnitSight))]
[RequireComponent(typeof (EnemyAttack))]
[RequireComponent(typeof (UnitStats))]

public class EnemyControlBob : MonoBehaviour
{
    [Header("Unit Attributes")]
    public float maxHealth = 100.0f;
    public float sightDistance = 10f;

    [Tooltip("The target object the unit moves to by default")]
    public string defaultTarget;
    public List<string> priorityList = new List<string>();

    [Header("Attack Attributes")]
    public float attackSpeed = 1.0f;
    public float attackRange = 1f;
    public float damage = 5;

    private NavMeshAgent agent;
    private UnitStats stats;
    private EnemyAttack action;
    private UnitSight vision;
    private float elapsedTime;
    private Vector3 targetLoc;
	private NavMeshObstacle obstacle;
    private GameManager gm;


    void Awake()
    {
		agent = GetComponent<NavMeshAgent>();
		stats = GetComponent<UnitStats>();
		action = GetComponent<EnemyAttack>();
		vision = GetComponent<UnitSight>();
		obstacle = GetComponent<NavMeshObstacle>();
        gm = GameObject.FindObjectOfType<GameManager>();
	}
	
	void Start(){
		obstacle.enabled = false;
		agent.enabled = true;
		elapsedTime = 0f;
		
		// Set values for dependant scripts. Only modify values in one script in inspector
		vision.defaultTarget = defaultTarget;
		vision.priorityList = priorityList;
        vision.sightDistance = sightDistance;
		stats.maxHealth = maxHealth;
        stats.currentHealth = maxHealth;
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
		if (vision.actionTarget != null)
		{
			targetLoc = vision.actionTarget.transform.position;
		}
        // Update the target location

        if (stats.attackSpeed < elapsedTime && vision.targetDistance < agent.stoppingDistance)
        {
            Debug.Log("Attacking");
            elapsedTime = 0f;
            Attack();
        }

        else
            Move();

        elapsedTime += Time.deltaTime;
    }

    void Attack()
    {        
        agent.Stop();
        Debug.Log(vision.actionTarget);
        action.Explode(vision.actionTarget);

        gameObject.SetActive(false);
    }

    void Move()
    {
        agent.Resume();
        agent.SetDestination(targetLoc);
    }
}
