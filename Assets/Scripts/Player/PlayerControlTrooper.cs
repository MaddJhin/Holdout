using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/* USES:
 * ==============
 * PlayerCharacterMovement
 * PlayerAttack.cs
 * ==============
 * 
 * USAGE:
 * ======================================
 * Acts as the control for the trooper player Character
 * Uses other scripts to attack and move 
 * Enables modularity
 * ======================================
 * 
 * Date Created: 27 Jul 2015
 * Last Modified: 	8 Aug 2015
 * Authors: Francisco Carrera
 */

[RequireComponent(typeof (NavMeshAgent))]
[RequireComponent(typeof (PlayerMovement))]
[RequireComponent(typeof (PlayerAction))]
[RequireComponent(typeof (UnitStats))]
[RequireComponent(typeof (NavMeshObstacle))]

public class PlayerControlTrooper : MonoBehaviour {

    [Header ("Unit Attributes")]
	public float maxHealth = 100f;
	public float sightRange = 10;
    public bool stunImmunity = false;

    [Tooltip ("How far the unit can go before returning to it's waypoint")]
    public float barricadeMaxThether = 10f;

    [Tooltip ("List of units to target. Most important at the start of the list")]
    public List<string> priorityList = new List<string>();	// Stores action target priorty (highest first).

    [Header("Action Attributes")]
	public float damagePerHit = 20f;
	public float attackRange = 2f;
	public float timeBetweenAttacks = 0.15f;

	Transform actionTarget;							// Target to shoot
	UnitStats stats;								// Unit stat scripts for health assignment
	float timer;                                    // A timer between actions.
	NavMeshAgent agent;								// Nav Agent for moving character
	PlayerMovement playerControl;					// Sets attack target based on priority
	PlayerAction playerAction;						// Script containg player attacks
	bool targetInRange;								// Tracks when target enters and leaves range
	float originalStoppingDistance;					// Used to store preset agent stopping distance
	NavMeshObstacle obstacle;						// Used to indicate other units to avoid this one

    Animator m_Animator;
    float m_TurnAmount;
    float m_ForwardAmount;
    bool m_Attacking;
	
	void Awake(){
		agent = GetComponent<NavMeshAgent>();
		playerControl = GetComponent<PlayerMovement>();
		playerAction = GetComponent<PlayerAction>();
		stats = GetComponent<UnitStats>();
		obstacle = GetComponent<NavMeshObstacle>();
        m_Animator = GetComponentInChildren<Animator>();
	}
	
	void Start (){
		playerAction.range = attackRange;
		playerControl.priorityList = priorityList;
		playerAction.timeBetweenAttacks = timeBetweenAttacks;
		originalStoppingDistance = agent.stoppingDistance;
		stats.maxHealth = maxHealth;
        stats.currentHealth = maxHealth;
        stats.attackRange = attackRange;
        stats.stunImmunity = stunImmunity;
		playerControl.maxBarricadeDistance = barricadeMaxThether;
		playerControl.sightDistance = sightRange;
	}
	
	// Update is called once per frame
	void Update () {

        UpdateAnimator(agent.desiredVelocity);

		// Add the time since Update was last called to the timer.
		timer += Time.deltaTime;
		actionTarget = playerControl.actionTarget;

		// If there is nothing to attack, script does nothing.
        if (actionTarget == null)
        {
            agent.stoppingDistance = originalStoppingDistance;
            return;
        }

        // Set if the target is in range
		if (Vector3.Distance (actionTarget.position, transform.position) <= attackRange)
		{
			Stop();
		}
		else
		{
			Move();
		}
		
		// If the target is in range and enough time has passed between attacks, Attack.
		if (timer >= timeBetweenAttacks && targetInRange)
		{
            Debug.Log("Tropper Attacking");
            
			Attack();
		}
	}

	void Stop(){
		targetInRange = true;

		if(agent.enabled)
		{
			agent.Stop();
		}
		agent.enabled = false;
		obstacle.enabled = true;
	}

	void Attack(){
		timer = 0f;
        m_Animator.SetTrigger("AttackTrigger");
		playerAction.Attack(damagePerHit);
	}

	void Move (){
		targetInRange = false;

		obstacle.enabled = false;
		agent.enabled = true;
		agent.Resume();
	}

    void UpdateAnimator(Vector3 move)
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
}
