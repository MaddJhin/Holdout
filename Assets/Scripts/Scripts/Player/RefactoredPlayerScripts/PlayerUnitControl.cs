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
    public float healthRegenRate = 0f;
    public float sightRange;
    public bool stunImmunity = false;
    public UnitTypes unitType;
    public float moveSpeed;

    [HideInInspector]
    public HpBarManager hpBar;

    //[Tooltip("How far the unit can go before returning to it's waypoint")]
    //public float barricadeMaxThether;

    [Tooltip("List of units to target. Most important at the start of the list")]
    public List<string> priorityList = new List<string>();	// Stores action target priorty (highest first).

    [Header("Attack Attributes")]
    public float damagePerHit = 20f;
    public float attackRange = 2f;
    public float timeBetweenAttacks = 0.15f;

    [Header("Shoot Attributes")]
    public Transform shootPoint;

    [Header("Heal Behavior Attributes")]
    [Tooltip("Amount of health healed per second of the Medic's heal ability")]
    public float healPerTick = 5f;

    [Header("Repair Behavior Attributes")]
    [Tooltip("The amount of health healed per second of the Mechanic's repair")]
    public float repairPerTick = 5f;

    [Header("Mechanic Support Attributes")]
    public float slowPercentage = 10f;
    public float slowDuration = 0.5f;

    #endregion

    #region Object & Component References

    [Header("Barricade & Target Data")]
    public RefactoredBarricade currentBarricade;                     // The current Barricade that the unit is stationed at
    public BarricadeWaypoint currentWaypoint;              // The current Waypoint that the unit is occupying
    public GameObject actionTarget;						// Target to shoot
    RefactoredBarricade retreatFromBarricade;

    UnitStats stats;								// Unit stat scripts for health assignment
    float timer;                                    // A timer between actions.
    public NavMeshAgent agent;								// Nav Agent for moving character
    PlayerMovement playerControl;					// Sets attack target based on priority
    RefactoredPlayerAction playerAction;						// Script containg player attacks
    bool targetInRange;								// Tracks when target enters and leaves range
    float originalStoppingDistance;					// Used to store preset agent stopping distance
    NavMeshObstacle obstacle;						// Used to indicate other units to avoid this one
    public Animation m_Animation;
    ParticleSystem[] m_ParticleSystem;
    Rigidbody m_RigidBody;
    bool performingAction;
    string selectedAction;
    public List<PlayerUnitControl> residentListCache;
    LineRenderer line;
    Light light;
    AudioSource gunshot;
    bool moving;
    Renderer[] rendCache;
    Color colorCache;
    AudioSource m_AudioSource;

    #endregion

    #region Designer Readability Attributes

    Color healIndicator = Color.green;
    Color sightIndicator = Color.yellow;
    Color attackRangeIndicator = Color.red;

    #endregion

    void OnDisable()
    {
        if (currentWaypoint != null)
            currentWaypoint.resident = null;
    }

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        playerControl = GetComponent<PlayerMovement>();
        playerAction = GetComponent<RefactoredPlayerAction>();
        stats = GetComponent<UnitStats>();
        obstacle = GetComponent<NavMeshObstacle>();
        m_Animation = GetComponentInChildren<Animation>();
        m_RigidBody = GetComponent<Rigidbody>();
        m_ParticleSystem = GetComponentsInChildren<ParticleSystem>();
        gunshot = GetComponent<AudioSource>();
        rendCache = GetComponentsInChildren<Renderer>();
        m_AudioSource = GetComponent<AudioSource>();

        InvokeRepeating("SelfHeal", 10, 1);
        

        // Determine which action should be repeated
        switch(unitType)
        {
            case UnitTypes.Medic:
                m_ParticleSystem = GetComponentsInChildren<ParticleSystem>();
                selectedAction = "ActivateHeal";
                break;

            case UnitTypes.Mechanic:
                m_ParticleSystem = GetComponentsInChildren<ParticleSystem>();
                selectedAction = "BeginFortify";
                break;
            
            case UnitTypes.Trooper:
                InvokeRepeating("TetherCheck", 1, 0.5f);
                selectedAction = "Slash";
                break;

            case UnitTypes.Marksman:
                selectedAction = "Shoot";
                line = GetComponentInChildren<LineRenderer>();
                light = GetComponentInChildren<Light>();
                line.SetPosition(0, shootPoint.position);
                break;

            default:
                selectedAction = null;
                break;
        }
    }

    // Use this for initialization
	void Start () 
    {
        for (int i = 0; i < rendCache.Length; i++)
        {
            if (rendCache[i].material.HasProperty("_OutlineColor"))
            {
                colorCache = rendCache[i].material.GetColor("_OutlineColor");
                colorCache.a = (10F / 255F);
                rendCache[i].material.SetColor("_OutlineColor", colorCache);
            }
        }

        if (unitType == UnitTypes.Mechanic || unitType == UnitTypes.Medic)
            actionTarget = this.gameObject;

        else
            actionTarget = null;

        performingAction = false;
        playerAction.actionTarget = actionTarget;
        playerAction.attackRange = attackRange;
        playerAction.damagePerHit = damagePerHit;
        playerAction.healPerHit = healPerTick;
        agent.speed = moveSpeed;
        residentListCache = new List<PlayerUnitControl>();
        StartCoroutine(EvaluateSituation());
    }
	
	// Update is called once per frame
	void Update () 
    {
        if (agent.velocity.magnitude > 0.5 && !performingAction)
        {
            m_Animation.CrossFade("Run");
        }

        else if (!performingAction)
        {
            m_Animation.CrossFade("Idle");
        }
	}

    #region Unit Actions

    /* All of the following Coroutines contain the logic for that action
     * E.G: Distance checks etc
     */

    IEnumerator Shoot()
    {
        //Shoot at target if in range of Barricade
        line.enabled = true;
        gunshot.PlayOneShot(gunshot.clip);
        StartCoroutine(ShootFX());
        playerAction.Shoot(actionTarget.GetComponent<UnitStats>());

        yield return new WaitForSeconds(timeBetweenAttacks);
        
        performingAction = false;
    }

    IEnumerator ShootFX()
    {
        light.enabled = true;
        yield return new WaitForSeconds(0.2f);
        m_AudioSource.Play();
        light.enabled = false;
    }

    IEnumerator Slash()
    {
        // If in range of the target, attack it
        // Else, move into range of the target

        if (Vector3.Distance(actionTarget.transform.position, transform.position) <= attackRange)
        {
            Stop();
            m_Animation.CrossFade("Attack");
            m_AudioSource.Play();
            playerAction.Attack(actionTarget.GetComponent<UnitStats>());
            yield return new WaitForSeconds(timeBetweenAttacks);
            performingAction = false;
        }

        else
        {
            Move(actionTarget.transform.position);
            performingAction = false;
        }        
    }

    IEnumerator ActivateHeal()
    {
        // Check if the resident list has changed
        if (currentBarricade != null &&
            agent.hasPath == false)
        {
            for (int i = 0; i < m_ParticleSystem.Length; i++)
            {
                m_ParticleSystem[i].Play();
            }

            // Cache the new resident list
            m_Animation.CrossFade("Attack");

            if (!m_AudioSource.isPlaying)
                m_AudioSource.Play();

            for (int i = 0; i < currentBarricade.residentList.Count; i++)
            {
                currentBarricade.residentList[i].healthRegenRate = healPerTick;
            }

            yield return new WaitForSeconds(timeBetweenAttacks);
        }

        else
            performingAction = false;
    }

    public IEnumerator DeactivateHeal()
    {
        if (currentBarricade != null && currentBarricade.residentList.Count > 0)
        {
            for (int i = 0; i < m_ParticleSystem.Length; i++)
            {
                m_ParticleSystem[i].Stop();
            }

            m_Animation.CrossFade("Idle");

            for (int i = 0; i < currentBarricade.residentList.Count; i++)
            {
                currentBarricade.residentList[i].healthRegenRate = 0;
            }

            performingAction = false;
        }

        else
            yield return null;
    }

    IEnumerator BeginFortify()
    {
        if (currentBarricade != null && agent.hasPath == false && agent.velocity.magnitude < 0.5)
        {
            for (int i = 0; i < m_ParticleSystem.Length; i++)
            {
                m_ParticleSystem[i].Play();
            }

            m_Animation.CrossFade("Attack");

            if (!m_AudioSource.isPlaying)
                m_AudioSource.Play();

            currentBarricade.fortified = true;
            currentBarricade.selfHealAmount = repairPerTick;
        }
        
        else
            performingAction = false;

        yield return null;
    }

    public IEnumerator EndFortify()
    {
        if (currentBarricade != null)
        {
            for (int i = 0; i < m_ParticleSystem.Length; i++)
            {
                m_ParticleSystem[i].Stop();
            }

            m_Animation.CrossFade("Idle");
            m_AudioSource.Stop();
            currentBarricade.fortified = false;
        }

        performingAction = false;

        yield return null;
    }

    public IEnumerator Slow(Collider[] targets)
    {
        for (int i = 0; i < targets.Length; i++)
        {
            targets[i].GetComponent<EnemyUnitControl>().SetSlow(slowPercentage, slowDuration);
        }

        yield return null;
    }

    void SelfHeal()
    {
        stats.Heal(healthRegenRate);
    }

    IEnumerator EvaluateSituation()
    {
        while (true)
        {
            if (actionTarget != null && actionTarget.activeInHierarchy && !performingAction && selectedAction != null)
            {
                if (unitType == UnitTypes.Marksman)
                {
                    line.SetPosition(0, shootPoint.position);
                    line.SetPosition(1, actionTarget.transform.position);
                    line.enabled = true;
                }

                performingAction = true;
                StartCoroutine(selectedAction);
            }

            else if (actionTarget != null && !actionTarget.activeInHierarchy)
            {
                actionTarget = null;

                if (unitType == UnitTypes.Marksman)
                {
                    line.enabled = false;
                }
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    #endregion

    #region Unit Targeting

    public IEnumerator CheckForTarget(Collider[] targets)
    {
        if (actionTarget == null)
        {
            for (int i = 0; i < priorityList.Count; i++)
            {
                for (int j = 0; j < targets.Length; j++)
                {
                    if (targets[j].tag == priorityList[i] && targets[j].gameObject.activeInHierarchy)
                    {
                        actionTarget = targets[j].gameObject;
                    }
                }
            }
        }

        yield return null;
    }

    #endregion

    #region Movement Functionality

    public void Move(Vector3 targetPos)
    {
        targetInRange = false;
        m_Animation.CrossFade("Run");
        obstacle.enabled = false;
        agent.enabled = true;
        agent.SetDestination(targetPos);
        agent.Resume();        
    }

    void Stop()
    {
        if (agent.enabled)
        {
            agent.Stop();
            m_Animation.CrossFade("Idle");
        }
    }

    #endregion

    #region Barricade Tethering

    /* Function contains logic for performing a tether check
     * This helps to ensure that the player units do not move too far from the barricade
     */
    private void TetherCheck()
    {
        if (currentBarricade)
        {
            if (Vector3.Distance(gameObject.transform.position, currentBarricade.transform.position) >= currentBarricade.sightRadius &&
                currentBarricade != null)
            {
                agent.SetDestination(currentWaypoint.transform.position);
            }

            else
                return;
        }

        else
            return;
    }

    public void BeginRetreat()
    {
        retreatFromBarricade = currentBarricade;
        StartCoroutine(RetreatFrom());
    }

    IEnumerator RetreatFrom()
    {
        int i = 0;
        // As long as there are retreatpoints, check for occupied
        while (i < retreatFromBarricade.retreatPoints.Count)
        {
            if (!retreatFromBarricade.retreatPoints[i].occupied)
            {
                agent.SetDestination(retreatFromBarricade.retreatPoints[i].transform.position);
                currentBarricade = retreatFromBarricade.retreatPoints[i].belongsTo;
                currentBarricade.retreatPoints[i].resident = gameObject;
                break;
            }

            i++;
        }


        yield return null;
    }

    #endregion

    #region Designer Readability Methods

    void OnDrawGizmosSelected()
    {    
        // Attack Range Indicator
        Gizmos.color = attackRangeIndicator;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    #endregion
}
