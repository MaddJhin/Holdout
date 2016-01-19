using UnityEngine;
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
    public float healthRegenRate = 0f;
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
    public List<PlayerUnitControl> residentListCache;

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

        InvokeRepeating("SelfHeal", 10, 1);

        // Determine which action should be repeated
        switch(unitType)
        {
            case UnitTypes.Medic:
                m_ParticleSystem = GetComponentsInChildren<ParticleSystem>();
                actionTarget = this.gameObject;
                selectedAction = "ActivateHeal";
                break;

            case UnitTypes.Mechanic:
                m_ParticleSystem = GetComponentsInChildren<ParticleSystem>();
                actionTarget = this.gameObject;
                selectedAction = "BeginFortify";
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
        playerAction.damagePerHit = damagePerHit;
        playerAction.healPerHit = healPerTick;
        stats.maxHealth = maxHealth;
        stats.currentHealth = maxHealth;
        residentListCache = new List<PlayerUnitControl>();
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (stats.currentHealth <= 0 || currentBarricade == null)
        {
            if (unitType == UnitTypes.Medic)
                StartCoroutine(DeactivateHeal());

            else if (unitType == UnitTypes.Mechanic)
                StartCoroutine(EndFortify());
        }

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

    IEnumerator ActivateHeal()
    {
        Debug.Log("Activating Heal");
        // Check if the resident list has changed
        if (currentBarricade != null &&
            agent.hasPath == false)
        {
            Debug.Log("Modifying Heal Values");
            // Cache the new resident list
            residentListCache = currentBarricade.residentList;

            for (int i = 0; i < residentListCache.Count; i++)
            {
                residentListCache[i].healthRegenRate = healPerTick;
            }

            yield return new WaitForSeconds(timeBetweenAttacks);
            performingAction = false;
        }
    }

    public IEnumerator DeactivateHeal()
    {
        Debug.Log("Deactivating Heal");
        if (currentBarricade.residentList.Count > 0)
        {
            for (int i = 0; i < currentBarricade.residentList.Count; i++)
            {
                currentBarricade.residentList[i].healthRegenRate = 0;
            }
        }

        yield return null;
    }

    IEnumerator BeginFortify()
    {
        Debug.Log("Beginning Fortification");

        if (currentBarricade != null && agent.hasPath == false)
        {
            Debug.Log("Adjusting Self Heal Values");
            currentBarricade.fortified = true;
            currentBarricade.selfHealAmount = repairPerTick;
        }
        
        performingAction = false;
        yield return null;
    }

    public IEnumerator EndFortify()
    {
        Debug.Log("Ending Fortification");

        if (currentBarricade != null)
        {
            currentBarricade.fortified = false;
        }

        yield return null;
    }

    public IEnumerator Slow(Collider[] targets)
    {
        Debug.Log("Slowing enemy units"); 
        for (int i = 0; i < targets.Length; i++)
        {
            Debug.Log("Slowing " + targets[i]);
            targets[i].GetComponent<EnemyUnitControl>().SetSlow(slowPercentage, slowDuration);
        }

        yield return null;
    }

    void SelfHeal()
    {
        stats.Heal(healthRegenRate);
    }

    #endregion

    #region Unit Targeting

    public IEnumerator CheckForTarget(Collider[] targets)
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
    }

    #endregion

    #region Animation

    void UpdateAnimator()
    {

    }

    #endregion
}
