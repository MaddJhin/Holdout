﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum UnitTypes
{
    Medic,
    Mechanic,
    Trooper,
    Marksman
}

public class PlayerUnitControl : MonoBehaviour
{
    #region Unit Attributes & Stats
    [Header("Unit Attributes")]
    public float maxHealth = 100f;
    public float sightRange;
    public bool stunImmunity = false;
    public UnitTypes unitType;

    //[Tooltip("How far the unit can go before returning to it's waypoint")]
    //public float barricadeMaxThether;

    [Tooltip("List of units to target. Most important at the start of the list")]
    public List<string> priorityList = new List<string>();	// Stores action target priorty (highest first).

    [Header("Attack Attributes")]
    public float damagePerHit = 20f;
    public float attackRange = 2f;
    public float timeBetweenAttacks = 0.15f;

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

    #endregion

    #region Object & Component References

    [Header("Barricade & Target Data")]
    public RefactoredBarricade currentBarricade;                     // The current Barricade that the unit is stationed at
    public BarricadeWaypoint currentWaypoint;              // The current Waypoint that the unit is occupying
    public GameObject actionTarget;						// Target to shoot

    UnitStats stats;								// Unit stat scripts for health assignment
    float timer;                                    // A timer between actions.
    public NavMeshAgent agent;								// Nav Agent for moving character
    PlayerMovement playerControl;					// Sets attack target based on priority
    RefactoredPlayerAction playerAction;						// Script containg player attacks
    bool targetInRange;								// Tracks when target enters and leaves range
    float originalStoppingDistance;					// Used to store preset agent stopping distance
    NavMeshObstacle obstacle;						// Used to indicate other units to avoid this one
    Animator m_Animator;
    ParticleSystem[] m_ParticleSystem;
    Rigidbody m_RigidBody;
    bool performingAction;
    string selectedAction;

    #endregion

    #region Designer Readability Attributes

    Color healIndicator = Color.green;
    Color sightIndicator = Color.yellow;
    Color attackRangeIndicator = Color.red;

    #endregion

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        playerControl = GetComponent<PlayerMovement>();
        playerAction = GetComponent<RefactoredPlayerAction>();
        stats = GetComponent<UnitStats>();
        obstacle = GetComponent<NavMeshObstacle>();
        m_Animator = GetComponentInChildren<Animator>();
        m_RigidBody = GetComponent<Rigidbody>();

        // Determine which action should be repeated
        switch(unitType)
        {
            case UnitTypes.Medic:
                m_ParticleSystem = GetComponentsInChildren<ParticleSystem>();
                InvokeRepeating("CheckForHeal", 2, 0.5f);
                selectedAction = "Heal";
                break;

            case UnitTypes.Mechanic:
                m_ParticleSystem = GetComponentsInChildren<ParticleSystem>();
                InvokeRepeating("CheckForRepair", 2, 0.5f);
                selectedAction = "Heal";
                break;
            
            case UnitTypes.Trooper:
                InvokeRepeating("TetherCheck", 1, 0.5f);
                selectedAction = "Slash";
                break;

            case UnitTypes.Marksman:
                selectedAction = "Shoot";
                break;

            default:
                selectedAction = null;
                break;
        }
    }

    // Use this for initialization
	void Start () 
    {
        actionTarget = null;
        performingAction = false;
        playerAction.actionTarget = actionTarget;
        playerAction.attackRange = attackRange;
        playerAction.healRange = healRange;
        playerAction.damagePerHit = damagePerHit;
        playerAction.healPerHit = healPerTick;
        stats.maxHealth = maxHealth;
        stats.currentHealth = maxHealth;
        
	}
	
	// Update is called once per frame
	void Update () 
    {

        // If the unit has a target, select the appropriate action
        if (actionTarget != null && actionTarget.activeInHierarchy && !performingAction && selectedAction != null)
        {
            performingAction = true;
            Debug.Log("Unit is acting");
            StartCoroutine(selectedAction);
        }

        else if (actionTarget != null && !actionTarget.activeInHierarchy)
        {
            Debug.Log("Target inactive - Setting to null");
            actionTarget = null;
        }
	}

    #region Unit Actions

    /* All of the following Coroutines contain the logic for that action
     * E.G: Distance checks etc
     */

    IEnumerator Shoot()
    {
        //Shoot at target if in range of Barricade
        Debug.Log("Beginning Shoot Coroutine");
        playerAction.Shoot(actionTarget.GetComponent<UnitStats>());
        yield return new WaitForSeconds(timeBetweenAttacks);
        performingAction = false;
    }

    IEnumerator Slash()
    {
        // If in range of the target, attack it
        // Else, move into range of the target
        Debug.Log("Checking Sight");

        if (Vector3.Distance(actionTarget.transform.position, transform.position) <= attackRange)
        {
            Debug.Log("Beginning Slash Coroutine");
            Stop();
            playerAction.Attack(actionTarget.GetComponent<UnitStats>());
            yield return new WaitForSeconds(timeBetweenAttacks);
            performingAction = false;
        }

        else
        {
            Debug.Log("Out of range, approaching");
            Move();
            performingAction = false;
        }        
    }

    IEnumerator Heal()
    {
        int tickCounter = 0;
        UnitStats targetStatCache = actionTarget.GetComponent<UnitStats>();

        // Enable particle effects on target
        foreach (var pfx in m_ParticleSystem)
        {
            pfx.transform.position = actionTarget.transform.position;
            pfx.enableEmission = true;
        }

        // Perform each tick of healing
        while (tickCounter < numberOfTicks)
        {
            tickCounter++;
            playerAction.Heal(healPerTick, targetStatCache);

            yield return new WaitForSeconds(timeBetweenTicks);
        }

        // Disable particle effects
        foreach (var pfx in m_ParticleSystem)
        {
            pfx.enableEmission = false;
        }

        // Once finished healing the target, null your current target
        actionTarget = null;
        yield return new WaitForSeconds(timeBetweenHeals);
        performingAction = false;
    }
    #endregion

    #region Unit Targeting

    public IEnumerator CheckForTarget(Collider[]targets)
    {
        for (int i = 0; i < priorityList.Count; i++)
        {
            Debug.Log("Checking priority at level " + i);
            for (int j = 0; j < targets.Length; j++)
            {
                if (targets[j].tag == priorityList[i])
                {
                    Debug.Log("Assigning target" + targets[j] + " to " + priorityList[i]);
                    actionTarget = targets[j].gameObject;
                }
            }
        }

        yield return null;
    }

    #endregion

    #region Movement Functionality

    void Move()
    {
        targetInRange = false;

        obstacle.enabled = false;
        agent.enabled = true;
        agent.SetDestination(actionTarget.transform.position);
        agent.Resume();
    }

    void Stop()
    {
        if (agent.enabled)
        {
            agent.Stop();
        }
    }

    #endregion

    #region Barricade Tethering

    /* Function contains logic for performing a tether check
     * This helps to ensure that the player units do not move too far from the barricade
     */
    private void TetherCheck()
    {
        if (Vector3.Distance(gameObject.transform.position, currentBarricade.transform.position) >= currentBarricade.sightRadius &&
            currentBarricade != null)
            agent.SetDestination(currentWaypoint.transform.position);

        else
            return;
    }
    #endregion

    #region Designer Readability Methods

    void OnDrawGizmosSelected()
    {    
        // Attack Range Indicator
        Gizmos.color = attackRangeIndicator;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Heal Range Indicator
        Gizmos.color = healIndicator;
        Gizmos.DrawWireSphere(transform.position, healRange);
    }

    #endregion

    #region Heal & Repair Checks

    void CheckForHeal()
    {
        Debug.Log("Checking for units to heal");

        if (currentBarricade != null)
        {
            PlayerUnitControl tempTarget = currentBarricade.residentList[0];        // Used as a baseline for determining min

            foreach (var unit in currentBarricade.residentList)
            {
                // If unit at max health, a medic, or the temporary target then skip to the next unit
                if (unit.stats.currentHealth == unit.maxHealth || unit.unitType == UnitTypes.Medic || unit == tempTarget)
                    continue;

                // If the unit has a lower % of health than the temp target, make it the new temp
                else if ((unit.stats.currentHealth / unit.maxHealth) < (tempTarget.stats.currentHealth / tempTarget.maxHealth))
                {
                    tempTarget = unit;
                }
            }

            // If the temp target has less health than it's max, set it as the target
            // Used to prevent the target from defaulting to a full health initial temp target
            if (tempTarget.stats.currentHealth < tempTarget.maxHealth)
                actionTarget = tempTarget.gameObject;
        }
    }

    void CheckForRepair()
    {
        if (currentBarricade.GetComponent<UnitStats>().currentHealth < currentBarricade.maxHealth)
        {
            actionTarget = currentBarricade.gameObject;
            StartCoroutine(Heal());
        }
    }

    #endregion

    #region Animation

    void UpdateAnimator()
    {

    }

    #endregion
}