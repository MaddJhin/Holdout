using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RefactoredBarricade : MonoBehaviour
{

    #region Barricade Attributes
    public float selfHealAmount;
    public float healRateSeconds;
    public float sightCheckDelay;

    [Tooltip("Begins checking for enemies, assigning & retreating X seconds after spawning")]
    public float checkAfter;
    public float sightRadius;
    public float assignTargetDelay;

    [Tooltip("Barricade that units will retreat to")]
    public GameObject retreatBarricade;

    public List<BarricadeWaypoint> frontWaypoints;
    public List<BarricadeWaypoint> backWaypoints;

    public List<PlayerUnitControl> residentList = new List<PlayerUnitControl>();
    UnitStats stats;
    public List<GameObject> targetQueue = new List<GameObject>();
    public LayerMask enemyMask;
    PlayerUnitControl unit;
    public bool fortified = false;
    #endregion

    #region Caches

    RefactoredBarricade retreatBarricadeCache;
    List<BarricadeWaypoint> retreatWaypointsCache;
    PlayerUnitControl unitCache;            // Used to skip searches on subsequent identical units
    Collider coll;
    float baseSelfHealCache;

    #endregion

    // Use this for initialization
    void Start()
    {
        stats = GetComponent<UnitStats>();
        coll = GetComponent<Collider>();
        BarricadeWaypoint[] temp = GetComponentsInChildren<BarricadeWaypoint>();
        unitCache = null;
        baseSelfHealCache = selfHealAmount;

        foreach (var curr in temp)
        {
            if (curr.tag == "Front Waypoint")
                frontWaypoints.Add(curr);

            else
                backWaypoints.Add(curr);
        }
    }

    void Awake()
    {
        InvokeRepeating("CheckForEnemies", checkAfter, sightCheckDelay);
        InvokeRepeating("FindRetreatPoints", checkAfter, 0.5f);
        InvokeRepeating("CheckForRetreat", checkAfter, 0.35f);
        InvokeRepeating("HealSelf", checkAfter, healRateSeconds);
    }

    void HealSelf()
    {
        if (fortified)
            stats.Heal(selfHealAmount);

        else
            stats.Heal(baseSelfHealCache);
    }

    #region Retreat Methods
    // Search for open waypoints and along retreat line and send units to them
    void FindRetreatPoints()
    {
        if (retreatBarricadeCache = retreatBarricade.GetComponent<RefactoredBarricade>())
        {
            if (retreatBarricadeCache != null && retreatBarricadeCache.residentList.Count < 5)
            {
                retreatWaypointsCache = new List<BarricadeWaypoint>();

                // Compile list of open waypoints
                // Add open front waypoints
                for (int i = 0; i < retreatBarricadeCache.frontWaypoints.Count; i++)
                {
                    // If the waypoint is not occupied, or marked for retreat, mark it
                    if (!retreatBarricadeCache.frontWaypoints[i].occupied ||
                        retreatWaypointsCache.Contains(retreatBarricadeCache.frontWaypoints[i]))
                    {
                        retreatWaypointsCache.Add(retreatBarricadeCache.frontWaypoints[i]);
                    }
                }

                // Add open back waypoints
                for (int i = 0; i < retreatBarricadeCache.backWaypoints.Count; i++)
                {
                    // If the waypoint is not occupied, or marked for retreat, mark it
                    if (!retreatBarricadeCache.backWaypoints[i].occupied ||
                        retreatWaypointsCache.Contains(retreatBarricadeCache.frontWaypoints[i]))
                    {
                        retreatWaypointsCache.Add(retreatBarricadeCache.backWaypoints[i]);
                    }
                }
            }
        }
    }

    void CheckForRetreat()
    {
        if (stats.currentHealth <= 0 && residentList.Count > 0)
        {
            RetreatFrom(this);
        }
    }

    void RetreatFrom(RefactoredBarricade retreatFromBarricade)
    {
        // If there are places to retreat to from this Barricade, retreat
        for (int i = 0; i < residentList.Count; i++)
        {
            residentList[i].currentBarricade = retreatFromBarricade.retreatBarricadeCache;
            residentList[i].currentWaypoint = retreatFromBarricade.retreatWaypointsCache[0];
            retreatFromBarricade.retreatWaypointsCache.Remove(residentList[i].currentWaypoint);
            residentList[i].agent.SetDestination(residentList[i].currentWaypoint.transform.position);
        }
    }

    #endregion

    #region Assign Waypoints

    public bool AssignFrontWaypoint(GameObject unit)
    {
        PlayerUnitControl barricadeCache;

        if (barricadeCache = unit.GetComponent<PlayerUnitControl>())
        {
            foreach (var waypoint in frontWaypoints)
            {
                if (waypoint.occupied == false)
                {
                    if (barricadeCache.currentWaypoint != null)
                    {
                        barricadeCache.currentWaypoint.occupied = false;
                        barricadeCache.currentWaypoint.resident = null;
                        barricadeCache.currentBarricade.residentList.Remove(barricadeCache);
                    }

                    barricadeCache.currentWaypoint = waypoint;
                    waypoint.occupied = true;
                    waypoint.resident = unit;
                    residentList.Add(unit.GetComponent<PlayerUnitControl>());
                    return true;
                }
            }
        }

        return false;
    }

    public bool AssignRearWaypoint(GameObject unit)
    {
        PlayerUnitControl barricadeCache;

        if (barricadeCache = unit.GetComponent<PlayerUnitControl>())
        {
            foreach (var waypoint in backWaypoints)
            {
                if (waypoint.occupied == false)
                {
                    if (barricadeCache.currentWaypoint != null)
                    {
                        barricadeCache.currentWaypoint.occupied = false;
                        barricadeCache.currentWaypoint.resident = null;
                        barricadeCache.currentBarricade.residentList.Remove(barricadeCache);
                    }

                    barricadeCache.currentWaypoint = waypoint;
                    waypoint.occupied = true;
                    waypoint.resident = unit;
                    residentList.Add(unit.GetComponent<PlayerUnitControl>());
                    return true;
                }
            }
        }

        return false;
    }

    #endregion

    #region Sight Checking Algorithm

    void CheckForEnemies()
    {
        if (residentList.Count > 0)
        {
            Debug.Log("Checking for enemy units");
            Collider[] targetsInRange = Physics.OverlapSphere(transform.position, sightRadius, enemyMask);

            // If there are enemies in range, ping residents to perform a sight check
            if (targetsInRange.Length > 0)
            {
                for (int i = 0; i < residentList.Count; i++)
                {
                    Debug.Log("Pinging residents");
                    if (residentList[i].unitType == UnitTypes.Trooper ||
                        residentList[i].unitType == UnitTypes.Marksman)
                        StartCoroutine(residentList[i].CheckForTarget(targetsInRange));

                    else if (residentList[i].unitType == UnitTypes.Mechanic)
                        StartCoroutine(residentList[i].Slow(targetsInRange));
                }
            }

            else
                Debug.Log("No enemies inrange");
        }

        else
            Debug.Log("No residents present - Cancelling vision check");
    }

    #endregion

    #region Designer Readability Methods

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRadius);
    }

    #endregion
}
