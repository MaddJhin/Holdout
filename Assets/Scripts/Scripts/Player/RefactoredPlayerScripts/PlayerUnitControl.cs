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
    public float healthRegenRate = 0f;
    public float sightRange;
    public bool stunImmunity = false;
    public UnitTypes unitType;
    public float moveSpeed = 1f;

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

    UnitStats stats;								// Unit stat scripts for health assignment
    float timer;                                    // A timer between actions.
    public NavMeshAgent agent;								// Nav Agent for moving character
    PlayerMovement playerControl;					// Sets attack target based on priority
    RefactoredPlayerAction playerAction;						// Script containg player attacks
    bool targetInRange;								// Tracks when target enters and leaves range
    float originalStoppingDistance;					// Used to store preset agent stopping distance
    NavMeshObstacle obstacle;						// Used to indicate other units to avoid this one
    public Animator m_Animator;
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

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        playerControl = GetComponent<PlayerMovement>();
        playerAction = GetComponent<RefactoredPlayerAction>();
        stats = GetComponent<UnitStats>();
        obstacle = GetComponent<NavMeshObstacle>();
        m_Animator = GetComponentInChildren<Animator>();
        m_RigidBody = GetComponent<Rigidbody>();
        m_ParticleSystem = GetComponentsInChildren<ParticleSystem>();
        gunshot = GetComponent<AudioSource>();
        rendCache = GetComponentsInChildren<Renderer>();
        m_AudioSource = GetComponent<AudioSource>();

        InvokeRepeating("SelfHeal", 10, 1);
        InvokeRepeating("EvaluateSituation", 5, 0.5f);

        // Determine which action should be repeated
        switch(unitType)
        {
            case UnitTypes.Medic:
                m_ParticleSystem = GetComponentsInChildren<ParticleSystem>();
                selectedAction = "ActivateHeal";
                InvokeRepeating("DeactivateSupportAbilities", 5, 1);
                break;

            case UnitTypes.Mechanic:
                m_ParticleSystem = GetComponentsInChildren<ParticleSystem>();
                selectedAction = "BeginFortify";
                InvokeRepeating("DeactivateSupportAbilities", 5, 1);
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
        m_Animator.speed = moveSpeed;
        residentListCache = new List<PlayerUnitControl>();
	}
	
	// Update is called once per frame
	void Update () 
    {
        
        

        if (agent.velocity.magnitude > 0.5)
            m_Animator.SetBool("Moving", true);

        else
            m_Animator.SetBool("Moving", false);

        // If the unit has a target, select the appropriate action
        
	}

    #region Unit Actions

    /* All of the following Coroutines contain the logic for that action
     * E.G: Distance checks etc
     */

    IEnumerator Shoot()
    {
        //Shoot at target if in range of Barricade
        m_Animator.SetTrigger("Acting");

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
        light.enabled = false;
    }

    IEnumerator Slash()
    {
        // If in range of the target, attack it
        // Else, move into range of the target

        if (Vector3.Distance(actionTarget.transform.position, transform.position) <= attackRange)
        {
            m_Animator.SetTrigger("Action");
            Stop();
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
            m_Animator.SetBool("Acting", true);

            if (!m_AudioSource.isPlaying)
                m_AudioSource.Play();

            for (int i = 0; i < currentBarricade.residentList.Count; i++)
            {
                currentBarricade.residentList[i].healthRegenRate = healPerTick;
            }

            yield return new WaitForSeconds(timeBetweenAttacks);
            performingAction = false;
        }

        else
            performingAction = false;
    }

    public IEnumerator DeactivateHeal()
    {
        if (currentBarricade.residentList.Count > 0)
        {
            for (int i = 0; i < m_ParticleSystem.Length; i++)
            {
                m_ParticleSystem[i].Pause();
            }

            m_Animator.SetBool("Acting", false);
            m_AudioSource.Stop();

            for (int i = 0; i < currentBarricade.residentList.Count; i++)
            {
                currentBarricade.residentList[i].healthRegenRate = 0;
            }
        }

        else
            yield return null;
    }

    IEnumerator BeginFortify()
    {
        moving = m_Animator.GetBool("Moving");

        if (currentBarricade != null && agent.hasPath == false && agent.velocity.magnitude < 0.5)
        {
            for (int i = 0; i < m_ParticleSystem.Length; i++)
            {
                m_ParticleSystem[i].Play();
            }

            m_Animator.SetBool("Acting", true);

            if (!m_AudioSource.isPlaying)
                m_AudioSource.Play();
            currentBarricade.fortified = true;
            currentBarricade.selfHealAmount = repairPerTick;
        }
        
        performingAction = false;
        yield return null;
    }

    public IEnumerator EndFortify()
    {
        if (currentBarricade != null)
        {
            for (int i = 0; i < m_ParticleSystem.Length; i++)
            {
                m_ParticleSystem[i].Play();
            }

            m_Animator.SetBool("Acting", false);
            m_AudioSource.Stop();
            currentBarricade.fortified = false;
        }

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

    void EvaluateSituation()
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
    }

    void DeactivateSupportAbilities()
    {
        if (stats.currentHealth <= 0 || currentBarricade == null)
        {
            if (unitType == UnitTypes.Medic)
                StartCoroutine(DeactivateHeal());

            else if (unitType == UnitTypes.Mechanic)
                StartCoroutine(EndFortify());
        }
    }

    #endregion

    #region Unit Targeting

    public IEnumerator CheckForTarget(Collider[] targets)
    {
        for (int i = 0; i < priorityList.Count; i++)
        {
            for (int j = 0; j < targets.Length; j++)
            {
                if (targets[j].tag == priorityList[i])
                {
                    actionTarget = targets[j].gameObject;
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
        m_Animator.SetBool("Moving", true);
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
            m_Animator.SetBool("Moving", false);
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
