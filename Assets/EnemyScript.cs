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
    public EnemyTypes unitType;                                  // What type of unit this is

    #endregion

    #region Navigation

    /*  Contains data which the unit uses to traverse levels
        and make decisions regarding movement               */

    GameObject navPath;                                         // The navigation path for the level
    PathNode targetPathNode;                                    // The next node on the path
    Transform targetLocation;                                   // The next location the unit is going to
    int pathNodeIndex = 0;                                      // Index for traversing navigation nodes

    #endregion

    #region Caches

    /*  Caches of object and component references */

    RefactoredBarricade targetBarricadeCache;                   // Caches Barricade the unit is interacting with
    GameObject targetPlayerCache;                        // Caches target player the unit is attacking

    #endregion

    #region Internal Variables

    /*  Miscellaneous variables used by the script when
        performing a variety of logical actions */

    Vector3 dir;                                        // Vector the unit moves along
    float distThisFrame;                                // How far the unit moves in a frame
    Quaternion targetRotation;                          // How much the unit wants to rotate
    bool beginAttacking;
    bool beginPathing;
    string selectedAction;

    #endregion

    void Awake()
    {
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
        navPath = GameObject.Find("Path");                      // Get the level's navigation path
        GetNextPathNode();
        StartCoroutine(MoveToTarget());
        beginPathing = true;
	}
	
	void Update ()
    {       
        /** State Switching Checks **/
        // If we have a node, check if it belongs to a Barricade       
        if (targetPathNode != null && targetPathNode.barricade != null)
        {
            beginAttacking = true;
            beginPathing = false;
        }

        /*
        // If there is nothing to attack, go back to pathing
        if (targetPlayerCache = null)
        {
            ("Beginning Move");
            beginPathing = true;
            beginAttacking = false;
            GetNextPathNode();
        }

        /**  Move unit towards it's target location  **/
        dir = targetLocation.position - this.transform.position;                // The vector to move along to reach target location
        distThisFrame = movespeed * Time.deltaTime;                             // How far the unit moves in a frame        

        /** Perform the appropriate state action **/
        if (beginPathing)
        {
            Pathfinding();
        }

        else if (beginAttacking)
        {
            StartCoroutine(Combat());
        }
    }

    #region Navigation & Target Selection Methods

    void GetNextPathNode()
    {
        targetPathNode = navPath.gameObject.transform.GetChild(pathNodeIndex).GetComponent<PathNode>();
        targetLocation = targetPathNode.transform;
        pathNodeIndex++;
    }

    /*  Function: Finds a random player located in front of a Barricade
        Parameters: 1) The Barricade to search
        Returns: The player found
    */
    GameObject FindPlayerTarget(RefactoredBarricade barricadeInput)
    {
        // If there are players in front of the Barricade, attack them
        if (barricadeInput.frontWaypoints.Count > 0)
        {             
            // Tracks how many players are in front of the Barricade
            // Starts at -1 to account for base 0 counting
            int frontPlayerCount = -1;

            // Count the number of players at the front waypoints
            for (int i = 0; i < barricadeInput.frontWaypoints.Count; i++)
            {
                // If a player is found, increment the playerCount
                if (barricadeInput.frontWaypoints[i])
                {
                    frontPlayerCount++;
                }
            }

            return barricadeInput.frontWaypoints[Random.Range(0, frontPlayerCount)].resident;
        }

        // Otherwise, attack the Barricade
        else
            return barricadeInput.gameObject;
    }

    /*  Function: Move unit towards selected location
        Parameters: None
        Returns: None
    */
    IEnumerator MoveToTarget()
    {
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
        // Check if we need to get a new node
        if (targetPathNode == null)
        {
            GetNextPathNode();
        }

        // Check if the unit is at the next node
        if (dir.magnitude <= distThisFrame)
        {
            targetPathNode = null;
        }
    }

    /*  Function: Attack targets

    */
    IEnumerator Combat()
    {
        if (targetPlayerCache == null)
        {
            targetPlayerCache = FindPlayerTarget(targetBarricadeCache);
            targetBarricadeCache = targetPathNode.barricade;                    // Cache the Barricade the unit is 
            targetLocation = targetPlayerCache.transform;                       // Set the target location to the player unit
        }

        if (dir.magnitude < attackRange)
        {
            StopCoroutine(MoveToTarget());
        }

        yield return null;
    }

    #endregion

    #region Enemy Actions

    #endregion

    #region Designer Readability Methods

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    #endregion
}
