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
 * Acts as the control for the Mechanic player Character
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

public class PlayerControlMechanic : MonoBehaviour {

    [Header ("Unit Attributes")]
	public float maxHealth = 100f;
	public float sightRange = 10;
    public float barricadeMaxThether = 10f;
    public List<string> priorityList = new List<string>();	// Stores action target priorty (highest first).


	public float healPerHit = 20f;
	public float healRange = 100f;
	public float timeBetweenHeals = 0.15f;
	
	
	
	UnitStats stats;								// Unit stat scripts for maxHealth assignment
	Transform actionTarget;							// Current Action target
	float timer;                                    // A timer between actions.
	NavMeshAgent agent;								// Nav Agent for moving character
	PlayerMovement playerControl;					// Sets attack target based on priority
	PlayerAction playerAction;						// Script containg player attacks
	bool targetInRange;								// Tracks when target enters and leaves range
	float originalStoppingDistance;					// Used to store preset agent stopping distance
	NavMeshObstacle obstacle;						// Used to indicate other units to avoid this one
    UnitStats repairTarget;                        // Target to be repaired
    LayerMask repairLayer;                          // Mask to find barricades
    bool m_Repairing;
    ParticleSystem[] m_RepairParticleSystem;

    Animator m_Animator;
    float m_ForwardAmount;
    float m_TurnAmount;
    bool m_Healing;

	void Awake(){
		agent = GetComponent<NavMeshAgent>();
		playerControl = GetComponent<PlayerMovement>();
		playerAction = GetComponent<PlayerAction>();
		stats = GetComponent<UnitStats>();
		obstacle = GetComponent<NavMeshObstacle>();
        m_RepairParticleSystem = GetComponentsInChildren<ParticleSystem>();
        repairLayer = LayerMask.GetMask("Player");
        m_Animator = GetComponent<Animator>();
	}
	
	void Start (){
		playerAction.range = healRange;
		playerControl.priorityList = priorityList;
		playerAction.timeBetweenAttacks = timeBetweenHeals;
		originalStoppingDistance = agent.stoppingDistance;
		stats.maxHealth = maxHealth;
		playerControl.maxBarricadeDistance = barricadeMaxThether;
		playerControl.sightDistance = sightRange;
        m_Repairing = false;
	}
	
	// Update is called once per frame
	void Update () 
    {
        UpdateAnimator(agent.desiredVelocity);
		
		// If there is nothing to attack, script does nothing.
		if (actionTarget == null) 
		{
			agent.stoppingDistance = originalStoppingDistance;
			return;
		}
		
		if (Vector3.Distance (actionTarget.position, transform.position) <= healRange)
		{
			Stop();
		}
		else
		{
			Move();
		}

        if (m_Repairing == false && repairTarget == null)
        {
            // ** TEST EFFICIENCY WITH: **
            // - Setting repair target based on current barricade
            Debug.Log("Retrieving Heal Target...");
            repairTarget = playerControl.SetHealTarget(gameObject.transform.position, healRange, repairLayer, "Barricade");
            Debug.Log("Heal Target found: " + repairTarget);
        }

		// If there is nothing to attack, script does nothing.
        if (repairTarget == null) 
		{
			agent.stoppingDistance = originalStoppingDistance;
			return;
		}
		
		// Set if the target is in range
        if (Vector3.Distance(repairTarget.transform.position, transform.position) <= healRange)
		{
			Stop();
		}
		else
		{
			Move();
		}

		// If the target is in range and enough time has passed between attacks, Attack.
        if (m_Repairing == false && targetInRange && repairTarget != null)
        {
            
            StartCoroutine("Repair");
        }
	}
	
	void Stop(){
		targetInRange = true;
		
//		if(agent.enabled)
//		{
//			agent.Stop();
//		}
//		agent.enabled = false;
//		obstacle.enabled = true;
	}
	
	IEnumerator Repair()
    {
        m_Repairing = true;
        transform.LookAt(repairTarget.transform.position);

        foreach (var pfx in m_RepairParticleSystem)
        {
            pfx.transform.position = repairTarget.transform.position;
            pfx.enableEmission = true;
        }

        Debug.Log("Healing: " + repairTarget);
        playerAction.Heal(healPerHit, repairTarget);

        yield return new WaitForSeconds(timeBetweenHeals);

        m_Repairing = false;
        repairTarget = null;
        Debug.Log("Post Healing Target: " + repairTarget);

        foreach (var pfx in m_RepairParticleSystem)
        {
            pfx.enableEmission = false;
        }
	}
	
	void Move (){
		targetInRange = false;
		
//		obstacle.enabled = false;
//		agent.enabled = true;
//		agent.Resume();
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
        m_Animator.SetBool("Healing", m_Repairing);
    }
}
