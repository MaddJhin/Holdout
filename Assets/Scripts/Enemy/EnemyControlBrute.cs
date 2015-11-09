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
 * Acts as the "brain" of the brute unit
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

public class EnemyControlBrute : MonoBehaviour 
{
    [Header ("Unit Attributes")]
	public float maxHealth = 100.0f;
    public float sightDistance = 10f;
    
    [Tooltip ("The target object the unit moves to by default")]
    public string defaultTarget;
    public List<string> priorityList = new List<string>();

    [Header ("Attack Attributes")]
	public float attackSpeed = 1.0f;
	public float attackRange = 1f;   
	public float stunDuration = 1f;
	public float attackRadius = 5f;
    public float damage = 10f;
	

    private NavMeshAgent agent;
    private UnitStats stats;
    private EnemyAttack action;
    private UnitSight vision;
    private float elapsedTime;
    private Vector3 targetLoc;
    private NavMeshObstacle obstacle;
    private GameManager gm;
    private Animator m_Animator;
    private ParticleSystem m_ParticleSystem;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        stats = GetComponent<UnitStats>();
        action = GetComponent<EnemyAttack>();
        vision = GetComponent<UnitSight>();
        obstacle = GetComponent<NavMeshObstacle>();
        gm = GameObject.FindObjectOfType<GameManager>();
        m_Animator = GetComponent<Animator>();
        m_ParticleSystem = GetComponentInChildren<ParticleSystem>();
    }

	void Start (){
		elapsedTime = 0f;

		// Set values for dependant scripts. Only modify values in one script in inspector
		vision.defaultTarget = defaultTarget;
		vision.priorityList = priorityList;
        vision.sightDistance = sightDistance;
		stats.maxHealth = maxHealth;
        stats.currentHealth = maxHealth;
		stats.attackSpeed = attackSpeed;
		stats.attackRange = attackRange;
		action.stunDuration = stunDuration;
		action.attackRadius = attackRadius;
        action.damage = damage;

	}

    void OnEnable()
    {
        gm.AddObjective();
    }

    void Update()
    {
        // Update target location
        targetLoc = vision.actionTarget.transform.position;

        // If unit as at the target, stop moving and block other units
        if (vision.targetDistance <= attackRange)
        {
			if(agent.enabled == true)
			{
				agent.Stop();
			}
            agent.enabled = false;
            obstacle.enabled = true;
            m_Animator.SetBool("Walk Forward", false);

            if (stats.attackSpeed < elapsedTime)
            {
                m_Animator.SetTrigger("PunchTrigger");
                elapsedTime = 0f;
                Attack();
            }
        }

        else
        {
            obstacle.enabled = false;
            agent.enabled = true;
            m_Animator.SetBool("Walk Forward", true);
            Move();
        }

        elapsedTime += Time.deltaTime;
    }

    void Attack()
    {
        //agent.Stop();
        Debug.Log("Current Particles: " + m_ParticleSystem);
        m_ParticleSystem.Stop();
        m_ParticleSystem.Play();
        Debug.Log(vision.actionTarget);
        action.Slam(vision.actionTarget);
    }

    void Move()
    {
        agent.Resume();
        agent.SetDestination(targetLoc);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}
