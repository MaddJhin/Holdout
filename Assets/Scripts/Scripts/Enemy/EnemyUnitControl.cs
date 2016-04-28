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
    Animation m_Animation;
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

        //InvokeRepeating("EvaluateSituation", 5, 0.5f);

        switch (unitType)
        {
            case EnemyTypes.Minion:
                selectedAction = "Punch";
                animSelector = Random.Range(0, 2);
                //m_Animation.SetInteger("AnimSelector", animSelector);
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
    
	void Start () 
    {
        performingAction = false;
        actionTarget = null;
        //m_Animation.speed = moveSpeed;
        StartCoroutine(VisionCheck());

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
            //m_Animation.SetBool("Moving", true);
            m_Animation.CrossFade("Run");
        }

        else
        {
            //m_Animation.SetBool("Moving", false);
            m_Animation.CrossFade("Idle");
        }

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
            //m_Animation.SetTrigger("Action");
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
    /*
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
    }*/
    
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
                if (targetBuffer.Length > 0)
                {
                    SetActionTarget();
                }

            }

            else if (actionTarget == null && targetBuffer.Length > 0)
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

        // Remove newly selected actionTarget from buffer
        tempBuffer = new Collider[targetBufferIndex];           // Temp array to represent buffer minus the newly selected target
        targetBuffer.CopyTo(tempBuffer, 0);                     // Copy contents of buffer to temp buffer
        targetBuffer = tempBuffer;                              // Set buffer to new contents
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
