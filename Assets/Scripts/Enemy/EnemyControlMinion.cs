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
 * Acts as the "brain" of the minion unit
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

public class EnemyControlMinion : MonoBehaviour 
{
    [Header ("Unit Attributes")]
	public float maxHealth = 100.0f;
    public float sightDistance = 10f;

    [Tooltip("The target object the unit moves to by default")]
    public string defaultTarget;
    public List<string> priorityList = new List<string>();

    [Header ("Attack Attributes")]
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
    private LineRenderer atkLine;               // Temporary

    // Animation attributes
    private Animator m_Animator;
    private float m_TurnAmount;
    private float m_ForwardAmount;
    private bool attacking;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        stats = GetComponent<UnitStats>();
        action = GetComponent<EnemyAttack>();
        vision = GetComponent<UnitSight>();
        obstacle = GetComponent<NavMeshObstacle>();
        m_Animator = GetComponentInChildren<Animator>();
        gm = GameObject.FindObjectOfType<GameManager>();
    }

	void Start(){
		obstacle.enabled = false;
		agent.enabled = true;
        attacking = false;
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
        UpdateMovementAnimator(agent.desiredVelocity);

        // Update the target location
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

            if (stats.attackSpeed < elapsedTime)
            {
                elapsedTime = 0f;
                attacking = true;
                Attack();
            }
        }

        else
        {
            obstacle.enabled = false;
            agent.enabled = true;
            attacking = false;
            if (atkLine != null)
                atkLine.enabled = false;
            Move();
        }

        elapsedTime += Time.deltaTime;
    }

    void Attack()
    {
        // Use of line renderer is temporary until animations are hooked up for Evoker
         
        //agent.Stop();
        if (gameObject.tag == "Evoker")
        {
            atkLine = GetComponent<LineRenderer>();
            atkLine.SetPosition(0, transform.position);
            atkLine.SetPosition(1, vision.actionTarget.transform.position);
            atkLine.enabled = true;
        }

        Debug.Log(vision.actionTarget);
        m_Animator.SetTrigger("AttackTrigger");
        action.Punch(vision.actionTarget);
    }

    void Move()
    {
        agent.Resume();
        agent.SetDestination(targetLoc);
    }

    void UpdateMovementAnimator(Vector3 move)
    {
        //Set float values based on nav agent velocity
        if (move.magnitude > 1f) move.Normalize();
        move = transform.InverseTransformDirection(move);
        m_TurnAmount = Mathf.Atan2(move.x, move.z);
        m_ForwardAmount = move.z;

        // Update animator float values 
        m_Animator.SetFloat("Forward", m_ForwardAmount, 0.1f, Time.deltaTime);
        m_Animator.SetFloat("Turn", m_TurnAmount, 0.1f, Time.deltaTime);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
