﻿using UnityEngine;
using System.Collections;

public enum EnemyTypes
{
    Minion,
    Brute,
    Evoker,
    Bob
}

public class EnemyUnitControl : MonoBehaviour
{
    #region Unit Attributes & Stats

    [Header("Unit Attributes")]
    public float maxHealth = 100f;
    public float sightRange;
    public bool stunImmunity = false;
    public EnemyTypes unitType;
    public GameObject targetLocation;
    public bool slowed = false;
    public float moveSpeed = 1f;

    [Header("Attack Attributes")]
    public float damagePerHit = 20f;
    public float attackRange = 2f;
    public float timeBetweenAttacks = 0.15f;

    #endregion

    #region Object & Component References

    // Component References
    NavMeshAgent agent;
    NavMeshObstacle obstacle;
    Animator m_Animator;
    EnemyAttack enemyAttack;
    UnitStats stats;
    ParticleSystem[] m_ParticleSystem;

    // Object References
    GameObject actionTarget;
    

    #endregion

    #region Object Caches & Internal Variables

    string selectedAction;
    bool performingAction;
    LayerMask playerLayer;
    float baseAttackSpeedCache;
    int animSelector;

    #endregion

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        obstacle = GetComponent<NavMeshObstacle>();
        m_Animator = GetComponentInChildren<Animator>();
        m_ParticleSystem = GetComponentsInChildren<ParticleSystem>();
        enemyAttack = GetComponent<EnemyAttack>();
        stats = GetComponent<UnitStats>();
        baseAttackSpeedCache = timeBetweenAttacks;

        InvokeRepeating("VisionCheck", 2f, 0.5f);

        switch(unitType)
        {
            case EnemyTypes.Minion:
                selectedAction = "Punch";
                animSelector = Random.Range(0, 2);
                m_Animator.SetInteger("AnimSelector", animSelector);
                break;

            case EnemyTypes.Brute:
                selectedAction = "Slam";
                break;

            case EnemyTypes.Evoker:
                selectedAction = "Shoot";
                break;

            case EnemyTypes.Bob:
                selectedAction = "Explode";
                break;

            default:
                break;
        }
    }

    // Use this for initialization
	void Start () 
    {
        performingAction = false;
        actionTarget = null;
        stats.maxHealth = maxHealth;
        stats.currentHealth = maxHealth;
        m_Animator.speed = moveSpeed;
        playerLayer = LayerMask.GetMask("Player");
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (agent.velocity.magnitude > 0.5)
            m_Animator.SetBool("Moving", true);

        else
            m_Animator.SetBool("Moving", false);

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

        else if (actionTarget == null)
            Move(targetLocation.transform.position);
	}

    #region Unit Actions

    IEnumerator Punch()
    {
        Debug.Log("Beginning Punch Coroutine");
        if (Vector3.Distance(transform.position, actionTarget.transform.position) <= attackRange)
        {
            Debug.Log("Attacking");
            Stop();
            m_Animator.SetTrigger("Action");
            enemyAttack.Punch(actionTarget);
            yield return new WaitForSeconds(stats.attackSpeed);
            performingAction = false;
        }

        else
        {
            Debug.Log("Out of range, approaching");
            Move(actionTarget.transform.position);
            performingAction = false;
        } 
    }

    IEnumerator Slam()
    {
        Debug.Log("Beginning Slam Coroutine");
        if (Vector3.Distance(actionTarget.transform.position, transform.position) <= attackRange)
        {
            Debug.Log("Beginning Slash Coroutine");
            Stop();
            m_Animator.SetTrigger("Action");
            enemyAttack.Slam(actionTarget);
            yield return new WaitForSeconds(stats.attackSpeed);
            performingAction = false;
        }

        else
        {
            Debug.Log("Out of range, approaching");
            Move(actionTarget.transform.position);
        }     
    }

    IEnumerator Explode()
    {
        Debug.Log("Beginng Explode Coroutine");
        if (Vector3.Distance(actionTarget.transform.position, transform.position) <= attackRange)
        {
            Debug.Log("Beginning Slash Coroutine");
            Stop();
            enemyAttack.Explode(actionTarget);
            yield return new WaitForSeconds(timeBetweenAttacks);
            performingAction = false;
        }

        else
        {
            Debug.Log("Out of range, approaching");
            Move(actionTarget.transform.position);
        } 
    }

    IEnumerator Shoot()
    {
        Debug.Log("Beginning Explode Coroutine");
        if (Vector3.Distance(actionTarget.transform.position, transform.position) <= attackRange)
        {
            Debug.Log("Beginning Slash Coroutine");
            Stop();
            m_Animator.SetTrigger("Action");
            enemyAttack.Shoot(actionTarget);
            yield return new WaitForSeconds(stats.attackSpeed);
            performingAction = false;
        }

        else
        {
            Debug.Log("Out of range, approaching");
            Move(actionTarget.transform.position);
        } 
    }

    public IEnumerator SetSlow(float slowAmount, float slowDuration)
    {
        float oldAttackSpeed = timeBetweenAttacks;                  // Cache old attack speed
        timeBetweenAttacks = timeBetweenAttacks - ((timeBetweenAttacks * slowAmount) / 100);
        m_Animator.speed = moveSpeed - ((moveSpeed * slowAmount) / 100);
        yield return new WaitForSeconds(slowDuration);
        timeBetweenAttacks = oldAttackSpeed;
        m_Animator.speed = moveSpeed;
    }

    #endregion

    #region Movement Functionality

    void Move(Vector3 destination)
    {
        obstacle.enabled = false;
        agent.enabled = true;
        agent.SetDestination(destination);
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

    #region Unit Targeting

    public void VisionCheck()
    {
        if (actionTarget == null)
        {
            Debug.Log("Performing Sight Check for " + unitType);

            Collider[] targetsInRange = Physics.OverlapSphere(transform.position, sightRange, playerLayer);

            if (targetsInRange.Length > 0)
                actionTarget = targetsInRange[0].gameObject;
        }

        else
            Debug.Log("Ignoring Sight Check for " + unitType);
    }

    #endregion

    #region Designer Readability Methods

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }

    #endregion
}