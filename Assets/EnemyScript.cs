using UnityEngine;
using System.Collections;

public enum EnemyTypes
{
    Minion,
    Brute,
    Evoker,
    Bob
}

public class EnemyScript : MonoBehaviour {

    #region Unit Stats                      
    [Header("Unit Stats")]

    /* Contains numerical information that describes the unit */ 

    public float movespeed = 3f;                                // Unit's movement speed
    public float health = 100f;                                 // Unit's health value
    public float attackDamage = 10f;                            // Damage dealt when attacking
    public float attackRange = 5f;                              // Range of attacks
    public float attackCooldown = 2f;

    [Tooltip("Duration of attack effects. E.G: Brute Stun")]
    public float attackEffectDuration = 1f;                     // Duration of any effects resulting from the attack

    [Tooltip("Area of Effect for abilities like the Brute Slam/Bob Explosion")]
    public float attackAreaOfEffect = 3f;                       // Area that the attack affects
    
    [Tooltip("Layer that the enemy unit attempts to attack")]
    public LayerMask attackTargetLayer;                         // The layer which the enemy unit attempts to attack
    public EnemyTypes unitType;                                 // What type of unit this is

    #endregion

    #region Navigation

    /*  Contains data which the unit uses to traverse levels
        and make decisions regarding movement               */

    GameObject navPath;                                         // The navigation path for the level
    PathNode[] pathNodeCollection;                              // Collection of all path nodes
    PathNode targetPathNode;                                    // The next node on the path
    Transform targetLocation;                                   // The next location the unit is going to
    int pathNodeIndex = 0;                                      // Index for traversing navigation nodes

    #endregion

    #region Caches

    /*  Caches of object and component references */

    RefactoredBarricade targetBarricadeCache;                   // Caches Barricade the unit is interacting with
    GameObject targetPlayerCache;                        // Caches target player the unit is attacking
    UnitStats targetHealthCache;

    #endregion

    #region Internal Variables

    /*  Miscellaneous variables used by the script when
        performing a variety of logical actions */

    Vector3 dir;                                        // Vector the unit moves along
    float distThisFrame;                                // How far the unit moves in a frame
    Quaternion targetRotation;                          // How much the unit wants to rotate

    // Flags
    bool beginAttacking;                                // Indicate that an attack state should begin
    bool attacking;                                     // Indicate that the unit is currently in it's attack state
    bool beginPathing;                                  // Indicate the pathing state should begin
    bool moving;

    #endregion

    #region Component References

    EnemyAttack m_EnemyAttack;

    #endregion

    void Awake()
    {
        m_EnemyAttack = GetComponent<EnemyAttack>();
        navPath = GameObject.Find("Path");                                      // Get the level's navigation path
        pathNodeCollection = navPath.GetComponentsInChildren<PathNode>();
    }

    void Start ()
    {
        GetNextPathNode();
        StartCoroutine("MoveToTarget");
        beginPathing = true;
	}
	
	void Update ()
    {
        if (targetLocation != null)
            dir = targetLocation.position - this.transform.position;                // The vector to move along to reach target location

        distThisFrame = movespeed * Time.deltaTime;                             // How far the unit moves in a frame
        
        if (targetPathNode != null && targetPathNode.barricade != null && !attacking)
        {
            attacking = true;
            beginAttacking = true;
            beginPathing = false;
            targetBarricadeCache = targetPathNode.barricade;                    // Cache the Barricade the unit is
            StartCoroutine("Combat");
        }

        else if (beginPathing)
        {
            if (!moving)
                StartCoroutine("MoveToTarget");

            Pathfinding();
        }
    }

    #region Navigation & Target Selection Methods

    void GetNextPathNode()
    {
        Debug.Log("Next node is: " + pathNodeIndex);
        targetPathNode = pathNodeCollection[pathNodeIndex];
        targetLocation = targetPathNode.transform;
        pathNodeIndex++;
    }

    /*  Function: Finds a random player located in front of a Barricade
        Parameters: 1) The Barricade to search
        Returns: The player found
    */
    GameObject FindPlayerTarget(RefactoredBarricade barricadeInput)
    {           
        // Tracks how many players are in front of the Barricade
        // Starts at -1 to account for base 0 counting
        int frontPlayerCount = -1;

        // Count the number of players at the front waypoints
        for (int i = 0; i < barricadeInput.frontWaypoints.Count; i++)
        {
            // If a player is found, increment the playerCount
            if (barricadeInput.frontWaypoints[i].occupied)
            {
                frontPlayerCount++;
            }
        }

        if (frontPlayerCount >= 0)
            return barricadeInput.frontWaypoints[Random.Range(0, frontPlayerCount)].resident;

        else
            return null;   
    }

    /*  Function: Move unit towards selected location
        Parameters: None
        Returns: None
    */
    IEnumerator MoveToTarget()
    {
        moving = true;

        while (true)
        {
            transform.Translate(dir.normalized * distThisFrame, Space.World);                                       // Move along the vector
            targetRotation = Quaternion.LookRotation(dir);                                                          // Where to look at
            this.transform.rotation = Quaternion.Lerp(this.transform.rotation, targetRotation, Time.deltaTime);     // Turn to face location

            yield return null;
        }
    }

    /*  Function: Find new path nodes as required
        Paramters: None
        Returns: None
    */
    void Pathfinding()
    {
        Debug.Log("Pathing");

        // Check if we need to get a new node
        if (targetPathNode == null)
        {
            GetNextPathNode();
        }

        // Check if the unit is at the next node
        if (dir.magnitude <= 1)
        {
            targetPathNode = null;
        }
        
    }

    /*  Function: Select targets, and perform the appropriate attack
        Parameters: None
        Returns: None
    */
    IEnumerator Combat()
    {
        while (true)
        {
            // If the current target has no HP, find a new one
            if ((targetHealthCache != null && targetHealthCache.currentHealth <= 0) || targetPlayerCache == null)
            {
                // If a player is found at the Barricade, cache it
                // Otherwise, cache the Barricade
                if (targetPlayerCache = FindPlayerTarget(targetBarricadeCache))
                {
                    targetHealthCache = targetPlayerCache.GetComponent<UnitStats>();
                    targetLocation = targetPlayerCache.transform;                       // Set the target location to the player unit
                }

                else
                {
                    targetPlayerCache = targetBarricadeCache.gameObject;
                    targetHealthCache = targetBarricadeCache.GetComponent<UnitStats>();
                    targetLocation = targetBarricadeCache.transform;

                    // If the Barricade has no HP, stop combat
                    if (targetHealthCache.currentHealth <= 0)
                    {
                        attacking = false;
                        targetPlayerCache = null;
                        targetLocation = null;
                        targetBarricadeCache = null;
                        targetPathNode = null;
                        beginPathing = true;

                        yield break;
                    }
                }
            }
            
            if (dir.magnitude < attackRange)
            {
                // Skips attack cooldown on first attack
                if (!beginAttacking)
                    yield return new WaitForSeconds(attackCooldown);

                else
                    beginAttacking = false;

                StopCoroutine("MoveToTarget");
                moving = false;

                switch (unitType)
                {
                    case EnemyTypes.Minion:
                        m_EnemyAttack.Punch(targetPlayerCache, attackDamage);
                        break;

                    case EnemyTypes.Brute:
                        m_EnemyAttack.Slam(targetPlayerCache, attackTargetLayer, attackDamage, attackEffectDuration, attackAreaOfEffect);
                        break;

                    case EnemyTypes.Evoker:
                        m_EnemyAttack.Shoot(targetPlayerCache, attackDamage);
                        break;

                    case EnemyTypes.Bob:
                        m_EnemyAttack.Explode(targetPlayerCache, attackTargetLayer, attackDamage, attackAreaOfEffect);
                        break;

                    default:
                        break;
                }
            }

            else if (!moving)
            {
                //StartCoroutine("MoveToTarget");
            }

            yield return null;
        }
    }

    #endregion

    #region Designer Readability Methods

    void OnDrawGizmosSelected()
    {
        //Gizmos.color = Color.red;
        //Gizmos.DrawWireSphere(transform.position, distThisFrame);
    }

    #endregion
}
