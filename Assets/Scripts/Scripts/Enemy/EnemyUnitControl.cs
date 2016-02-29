using UnityEngine;
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
    public float projectileSpeed = 5f;

    #endregion

    #region Object & Component References

    // Component References
    NavMeshAgent agent;
    NavMeshObstacle obstacle;
    Animator m_Animator;
    EnemyAttack enemyAttack;
    UnitStats stats;
    ParticleSystem m_ParticleSystem;
    Vector3 projectileTargetPos;

    // Object References
    GameObject actionTarget;
    Collider targetCollider;
    public Projectile projectile;

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
                m_ParticleSystem = GetComponentInChildren<ParticleSystem>();
                selectedAction = "Slam";
                break;

            case EnemyTypes.Evoker:
                selectedAction = "Shoot";
                break;

            case EnemyTypes.Bob:
                m_ParticleSystem = GetComponentInChildren<ParticleSystem>();
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
        m_Animator.speed = moveSpeed;
        playerLayer = LayerMask.GetMask("Player");
        targetLocation = GameObject.Find("Evac Shuttle");

        if (projectile != null)
        {
            projectile.gameObject.SetActive(false);
        }
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (agent.velocity.magnitude > 0.5)
        {
            m_Animator.SetBool("Moving", true);
        }

        else
            m_Animator.SetBool("Moving", false);

        // If the unit has a target, select the appropriate action
        if (actionTarget != null && actionTarget.activeInHierarchy && !performingAction && selectedAction != null)
        {
            performingAction = true;
            StartCoroutine(selectedAction);
        }

        else if (actionTarget != null && !actionTarget.activeInHierarchy)
        {
            actionTarget = null;
        }

        else if (actionTarget == null)
            Move(targetLocation.transform.position);
	}

    #region Unit Actions

    IEnumerator Punch()
    {
        if (Vector3.Distance(targetCollider.ClosestPointOnBounds(transform.position), transform.position) <= attackRange)
        {
            Stop();
            m_Animator.SetTrigger("Action");
            enemyAttack.Punch(actionTarget);
            yield return new WaitForSeconds(stats.attackSpeed);
            performingAction = false;
        }

        else
        {
            Move(actionTarget.transform.position);
            performingAction = false;
        } 
    }

    IEnumerator Slam()
    {
        if (Vector3.Distance(targetCollider.ClosestPointOnBounds(transform.position), transform.position) <= attackRange)
        {
            Stop();
            //m_ParticleSystem.Play(true);
            m_Animator.SetTrigger("Action");
            enemyAttack.Slam(actionTarget);
            yield return new WaitForSeconds(stats.attackSpeed);
            performingAction = false;         
        }

        else
        {
            performingAction = false;
            Move(actionTarget.transform.position);
        }     
    }

    IEnumerator Explode()
    {
        if (Vector3.Distance(targetCollider.ClosestPointOnBounds(transform.position), transform.position) <= attackRange)
        {
            Stop();
            enemyAttack.Explode(actionTarget);
            stats.KillUnit();
            m_ParticleSystem.Play(true);
            yield return new WaitForSeconds(timeBetweenAttacks);
            performingAction = false;         
        }

        else
        {
            performingAction = false;
            Move(actionTarget.transform.position);
        } 
    }

    IEnumerator Shoot()
    {
        Stop();
        m_Animator.SetTrigger("Action");
        projectile.gameObject.SetActive(true);
        projectile.FireProjectile(actionTarget.transform.position, projectileSpeed, transform.position);
        enemyAttack.Shoot(actionTarget);
        yield return new WaitForSeconds(stats.attackSpeed);
        performingAction = false;
         
    }

    void resetProjectile()
    {
        if (projectile.transform.position == projectileTargetPos)
        {
            projectile.gameObject.SetActive(false);
            projectile.transform.position = transform.position;
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
            Collider[] targetsInRange = Physics.OverlapSphere(transform.position, sightRange, playerLayer);

            if (targetsInRange.Length > 0)
            {
                actionTarget = targetsInRange[0].gameObject;
                targetCollider = targetsInRange[0];
            }
        }
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
