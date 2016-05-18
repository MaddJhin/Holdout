using UnityEngine;
using System.Collections;


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
    Animation m_Animation;
    EnemyAttack enemyAttack;
    UnitStats stats;
    public ParticleSystem m_ParticleSystem;
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

    Collider[] targetBuffer;
    int targetBufferIndex;
    Collider[] tempBuffer;

    #endregion

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        obstacle = GetComponent<NavMeshObstacle>();
        m_Animation = GetComponentInChildren<Animation>();
        enemyAttack = GetComponent<EnemyAttack>();
        stats = GetComponent<UnitStats>();
        m_AudioSource = GetComponent<AudioSource>();
        baseAttackSpeedCache = timeBetweenAttacks;

        

        switch (unitType)
        {
            case EnemyTypes.Minion:
                selectedAction = "Punch";
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
    
	void Start () 
    {
        performingAction = false;
        actionTarget = null;
        StartCoroutine(VisionCheck());
        StartCoroutine(EvaluateSituation());

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
        if (agent.velocity.magnitude > 0.5 && unitType != EnemyTypes.Evoker && !performingAction)
        {
            m_Animation.CrossFade("Run");
        }

        else if (!performingAction)
        {
            m_Animation.CrossFade("Idle");
        }      
    }

    IEnumerator EvaluateSituation()
    {
        while (true)
        {
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
            {
                Move(targetLocation.transform.position);
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    #region Unit Actions

    IEnumerator Punch()
    {
        if (Vector3.Distance(targetCollider.ClosestPointOnBounds(transform.position), transform.position) <= attackRange)
        {
            Stop();
            m_Animation.CrossFade("Attack");
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
            m_Animation.CrossFade("Attack");
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

        m_Animation.CrossFade("Attack");
        /*
        projectile.gameObject.SetActive(true);
        m_AudioSource.clip = unitAudio[1];
        m_AudioSource.Play();
        projectile.FireProjectile(actionTarget.transform.position, projectileSpeed, transform.position); */
        enemyAttack.Shoot(actionTarget, damagePerHit);
        yield return new WaitForSeconds(timeBetweenAttacks);
        performingAction = false;
         
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
        //m_Animation.speed = moveSpeed - ((moveSpeed * slowAmount) / 100);
        agent.speed = moveSpeed - ((moveSpeed * slowAmount) / 100);
        yield return new WaitForSeconds(slowDuration);
        timeBetweenAttacks = oldAttackSpeed;
        //m_Animation.speed = moveSpeed;
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
    
    IEnumerator VisionCheck()
    {
        while (true)
        {
            if (actionTarget == null && (targetBuffer == null || targetBuffer.Length <= 0))
            {
                // Find new targets and save them to buffer
                targetBuffer = Physics.OverlapSphere(transform.position, sightRange, visionLayer);
                targetBufferIndex = targetBuffer.Length - 1;


                // If targets were found, set them
                if (targetBuffer != null && targetBuffer.Length > 0 && (targetBufferIndex != targetBuffer.Length -1))
                {
                    SetActionTarget();
                }

            }

            else if (actionTarget == null && (targetBuffer != null && targetBuffer.Length > 0))
            {
                SetActionTarget();
            }

            yield return new WaitForSeconds(1f);
        }
    }

    void SetActionTarget()
    {
        actionTarget = targetBuffer[targetBufferIndex].gameObject;
        targetCollider = targetBuffer[targetBufferIndex];

        // If the shrink target is less than 0 don't shrink
        if (targetBufferIndex - 1 >= 0)
            System.Array.Resize(ref targetBuffer, targetBufferIndex - 1);
        
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
