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
    public LayerMask visionLayer;

    [Header("Attack Attributes")]
    public float damagePerHit;
    public float attackRange;
    public float timeBetweenAttacks;
    public float projectileSpeed;
    public LayerMask validTargets;

    [Header("Audio Attributes")]
    [Tooltip("Spawn audio must always be first in array")]
    public AudioClip[] unitAudio;

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
    AudioSource m_AudioSource;

    // Object References
    GameObject actionTarget;
    Collider targetCollider;

    [Header("Projectile Attributes")]
    public Projectile projectile;

    #endregion

    #region Object Caches & Internal Variables

    string selectedAction;
    bool performingAction;
    
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
        m_AudioSource = GetComponent<AudioSource>();
        baseAttackSpeedCache = timeBetweenAttacks;

        //InvokeRepeating("VisionCheck", 2f, 0.5f);
        //InvokeRepeating("EvaluateSituation", 5, 0.5f);

        switch (unitType)
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

        if (projectile != null)
        {
            projectile.gameObject.SetActive(false);
        }
	}

    void OnEnable()
    {
        if (unitAudio[0] != null && m_AudioSource != null)
        {
            m_AudioSource.clip = unitAudio[0];
            m_AudioSource.Play();
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

        if (actionTarget == null && targetLocation != null)
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
            yield return new WaitForSeconds(timeBetweenAttacks);
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
            m_Animator.SetTrigger("Action");
            enemyAttack.Slam(actionTarget, validTargets);
            yield return new WaitForSeconds(timeBetweenAttacks);
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
            m_ParticleSystem.Play(true);
            m_ParticleSystem.transform.parent = null;
            AudioSource.PlayClipAtPoint(unitAudio[1], transform.position);
            enemyAttack.Explode(actionTarget, validTargets);
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
        /*
        projectile.gameObject.SetActive(true);
        m_AudioSource.clip = unitAudio[1];
        m_AudioSource.Play();
        projectile.FireProjectile(actionTarget.transform.position, projectileSpeed, transform.position); */
        enemyAttack.Shoot(actionTarget, damagePerHit);
        yield return new WaitForSeconds(timeBetweenAttacks);
        performingAction = false;
         
    }

    void EvaluateSituation()
    {
        
    }

    public void LaunchProjectile()
    {
        projectile.FireProjectile(actionTarget.transform.position, projectileSpeed, transform.position);
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
            Collider[] targetsInRange = Physics.OverlapSphere(transform.position, sightRange, visionLayer);

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
