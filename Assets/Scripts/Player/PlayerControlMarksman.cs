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
 * Acts as the control for the marksman player Character
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

public class PlayerControlMarksman : MonoBehaviour {
    
    [Header ("Unit Attributes")]
	public float maxHealth = 100f;
	public float sightRange = 10;

    [Tooltip("How far the unit can go before returning to it's waypoint")]
    public float barricadeMaxThether = 10f;

    [Tooltip("List of units to target. Most important at the start of the list")]
    public List<string> priorityList = new List<string>();	// Stores action target priorty (highest first).

    [Header("Action Attributes")]
	public float damagePerHit = 20f;
	public float attackRange = 100f;
	public float timeBetweenAttacks = 0.15f;
	
	

	UnitStats stats;								// Unit stat scripts for health assignment
	Transform actionTarget;							// Current Action target
	float timer;                                    // A timer between actions.
	NavMeshAgent agent;								// Nav Agent for moving character
	PlayerMovement playerControl;					// Sets attack target based on priority
	PlayerAction playerAction;						// Script containg player attacks
	bool targetInRange;								// Tracks when target enters and leaves range
	float effectsDisplayTime = 0.1f;                // The proportion of the timeBetweenBullets that the effects will display for.
	float originalStoppingDistance;					// Used to store preset agent stopping distance
	NavMeshObstacle obstacle;						// Used to indicate other units to avoid this one


	void Awake(){
		agent = GetComponent<NavMeshAgent>();
		playerControl = GetComponent<PlayerMovement>();
		playerAction = GetComponent<PlayerAction>();
		stats = GetComponent<UnitStats>();
		obstacle = GetComponent<NavMeshObstacle>();
	}

	void Start (){
		playerAction.range = attackRange;
		playerControl.priorityList = priorityList;
		playerAction.timeBetweenAttacks = timeBetweenAttacks;
		originalStoppingDistance = agent.stoppingDistance;
		stats.maxHealth = maxHealth;
        stats.currentHealth = maxHealth;
        stats.attackRange = attackRange;
		playerControl.maxBarricadeDistance = barricadeMaxThether;
		playerControl.sightDistance = sightRange;
	}
	
	// Update is called once per frame
	void Update () {
		// Add the time since Update was last called to the timer.
		timer += Time.deltaTime;
		actionTarget = playerControl.actionTarget;

		// If the timer has exceeded the proportion of timeBetweenBullets that the effects should be displayed for...
		if(timer >= timeBetweenAttacks * effectsDisplayTime)
		{
			// ... disable the effects.
			playerAction.DisableEffects ();
		}

		// If there is nothing to attack, script does nothing.
		if (actionTarget == null) 
		{
			agent.stoppingDistance = originalStoppingDistance;
			return;
		}
		
		// Otherwise, if there is a valid target...
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
			Shoot();
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
	
	void Shoot(){
		timer = 0f;
		playerAction.Shoot(damagePerHit);
	}
	
	void Move (){
		targetInRange = false;
		
		obstacle.enabled = false;
		agent.enabled = true;
		agent.Resume();
	}
}
