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
 * Acts as the control for the medic player Character
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

public class PlayerControlMedic : MonoBehaviour 
{
    [Header("Unit Attributes")]
	public float maxHealth = 100f;
	public float sightRange = 10;
    public float barricadeMaxThether = 10f;
	public List<string> priorityList = new List<string>();	// Stores action target priorty (highest first).

    [Header("Heal Behavior Attributes")]
    [Tooltip("Amount of health healed per tick of the Medic's heal ability")]
	public float healPerTick = 10f;

    [Tooltip("How many ticks of healing will occur")]
    public int numberOfTicks = 3;

    [Tooltip("Time between each tick of healing occurring")]
    public float timeBetweenTicks = 1;

    [Tooltip("Number of seconds between heals")]
	public float timeBetweenHeals = 3f;
    public float healRange = 100f;
	
	UnitStats stats;								// Unit stat scripts for health assignment
	Transform actionTarget;							// Current Action target
	NavMeshAgent agent;								// Nav Agent for moving character
	PlayerMovement playerControl;					// Sets attack target based on priority
	PlayerAction playerAction;						// Script containg player attacks
	bool targetInRange;								// Tracks when target enters and leaves range
	float originalStoppingDistance;					// Used to store preset agent stopping distance
	NavMeshObstacle obstacle;						// Used to indicate other units to avoid this one
    public UnitStats healTarget;                          // Current target to heal
    LayerMask healMask;                             // Layer Mask of heal targets
    ParticleSystem[] m_HealParticleSystem;            // Particles for healing
    LineRenderer healBeam;
    int tickCounter;

    Animator m_Animator;
    float m_ForwardAmount;
    float m_TurnAmount;
    public bool m_Healing;
	
	void Awake(){
		agent = GetComponent<NavMeshAgent>();
		playerControl = GetComponent<PlayerMovement>();
		playerAction = GetComponent<PlayerAction>();
		stats = GetComponent<UnitStats>();
		obstacle = GetComponent<NavMeshObstacle>();
        m_Animator = GetComponent<Animator>();
        healMask = LayerMask.GetMask("Player");
        m_HealParticleSystem = GetComponentsInChildren<ParticleSystem>();
        healBeam = GetComponent<LineRenderer>();
	}
	
	void Start (){
		playerAction.range = healRange;
		playerControl.priorityList = priorityList;
		playerAction.timeBetweenAttacks = timeBetweenHeals;
		originalStoppingDistance = agent.stoppingDistance;
		stats.maxHealth = maxHealth;
        stats.currentHealth = maxHealth;
		playerControl.maxBarricadeDistance = barricadeMaxThether;
		playerControl.sightDistance = sightRange;
        m_Healing = false;
        healBeam.enabled = false;

        foreach (var pfx in m_HealParticleSystem)
            pfx.enableEmission = false;
	}
	
	// Update is called once per frame
	void Update () {

        UpdateAnimator(agent.desiredVelocity);

        Debug.Log("Healing: " + m_Healing);
        Debug.Log("Current Heal Target: " + healTarget);

        /*
        if (m_Healing == false && healTarget == null)
        {
            // ** TEST EFFICIENCY WITH: **
            // - Tracking health of all active units
            Debug.Log("Retrieving Heal Target...");
            healTarget = playerControl.SetHealTarget(gameObject.transform.position, healRange, healMask, "Player");
            Debug.Log("Heal Target found: " + healTarget);
        }*/

        if (stats.currentHealth <= 0)
        {
            m_Animator.Play("Death");
            StartCoroutine("KillUnit");
        }

		// If there is nothing to attack, script does nothing.
		if (healTarget == null) 
		{
			agent.stoppingDistance = originalStoppingDistance;
			return;
		}
		
		// Set if the target is in range
		if (Vector3.Distance (healTarget.transform.position, transform.position) <= healRange)
		{
			Stop();
		}
		else
		{
			Move();
		}
        
		// If the target is in range and enough time has passed between attacks, Attack.
        if (m_Healing == false && targetInRange && healTarget != null)
        {
            Debug.Log("Beginning Heal");
            StartCoroutine("Heal");
        }
	}

	void Stop()
    {
		targetInRange = true;
	}

	void Move ()
    {
		targetInRange = false;
	}
	
	IEnumerator Heal()
    {		
        tickCounter = 0;                                                  // Iterator for counting heal ticks

        if (healTarget.tag != "Player")
            yield return null;

        // Enabled relevant particle systems
        StartCoroutine("BeamModulator");
        foreach (var pfx in m_HealParticleSystem)
        {
            pfx.transform.position = healTarget.transform.position;
            pfx.enableEmission = true;
        }

        while (m_Healing)
        {
            tickCounter++;

            Debug.Log("Heal Tick " + tickCounter + "on: " + healTarget);
            playerAction.Heal(healPerTick, healTarget);

            // Delay between each tick of healing
            yield return new WaitForSeconds(timeBetweenTicks);

            if (tickCounter >= numberOfTicks)
            {
                Debug.Log("Finishing Heal");
                m_Healing = false;
                
            }
            
        }

        // Disable relevant particle systems
        foreach (var pfx in m_HealParticleSystem)
        {
            pfx.enableEmission = false;
        }

        healTarget = null;
        Debug.Log("Ending Heal Loop.");

        // Cooldown on beginng a heal
        yield return new WaitForSeconds(timeBetweenHeals);
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
        m_Animator.SetBool("Healing", m_Healing);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, healRange);
    }

    IEnumerator BeamModulator()
    {
        m_Healing = true;
        healBeam.enabled = true;
        transform.LookAt(healTarget.transform.position);

        // Position healing beam
        healBeam.SetPosition(1, healTarget.transform.position);

        while(m_Healing)
        {
            if (healBeam.enabled)
            {
                healBeam.material.mainTextureOffset = new Vector2(healBeam.material.mainTextureOffset.x + (.0007f * Time.time),
                                                                    healBeam.material.mainTextureOffset.y + (.0007f * Time.time));

                yield return null;
            }

            else
                yield return null;
        }

        healBeam.enabled = false;

        yield return null;
    }

    IEnumerator KillUnit()
    {
        yield return new WaitForSeconds(2);
        gameObject.SetActive(false);
    }
}
