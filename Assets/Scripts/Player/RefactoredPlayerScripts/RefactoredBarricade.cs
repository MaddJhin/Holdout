using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RefactoredBarricade : MonoBehaviour 
{
    public float maxHealth = 100;
    public float sightCheckDelay = .5f;

    [Tooltip("Begins checking for enemies, assigning & retreating X seconds after spawning")]
    public float checkAfter = 5f;
    public float sightRadius = 20f;
    public float assignTargetDelay = .5f;

    [Tooltip("Barricade that units will retreat to")]
    public GameObject retreatBarricade;

    public List<BarricadeWaypoint> frontWaypoints;
    public List<BarricadeWaypoint> backWaypoints;

    public List<PlayerUnitControl> residentList = new List<PlayerUnitControl>();
    UnitStats stats;
    public List<GameObject> targetQueue = new List<GameObject>();
    LayerMask enemyMask;
    PlayerUnitControl unit;

    #region Caches

    RefactoredBarricade retreatBarricadeCache;
    List<BarricadeWaypoint> retreatWaypointsCache;
    PlayerUnitControl unitCache;            // Used to skip searches on subsequent identical units

    #endregion

    // Use this for initialization
    void Start()
    {
        enemyMask = LayerMask.GetMask("Enemy");
        stats = GetComponent<UnitStats>();
        BarricadeWaypoint[] temp = GetComponentsInChildren<BarricadeWaypoint>();
        stats.maxHealth = maxHealth;
        unitCache = null;

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
        InvokeRepeating("AssignTarget", checkAfter, assignTargetDelay);
        InvokeRepeating("FindRetreatPoints", checkAfter, 0.5f);
        InvokeRepeating("CheckForRetreat", checkAfter, 0.35f);
    }

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

            foreach (var target in targetsInRange)
            {
                if (!targetQueue.Contains(target.gameObject))
                    targetQueue.Add(target.gameObject);
            }
        }

        else
            Debug.Log("No residents present - Cancelling vision check");
    }

    void AssignTarget()
    {
        if (residentList.Count > 0 && targetQueue.Count > 0)
        {
            Debug.Log("Assigning Targets");
            GameObject targetToAssign = targetQueue[0];
            targetToAssign.name = targetToAssign.name.Replace("(Clone)", "");

            // Loop to iterate through each level of priority
            for (int i = 0; i < 4; i++)
            {
                Debug.Log("Checking Level " + i + " of priority");
                // Loop through each unit at the barricade, and check it at level N of priority
                for (int j = 0; i < residentList.Count; i++)
                {
                    unit = residentList[j];

                    Debug.Log("CHECKING PRIORITY OF " + targetQueue[0].name + " FOR " + unit);

                    // If the unit has a target at the top level of priority,
                    // Or if the unit is the same as the previous unit which failed: skip it
                    if (unit.unitType == UnitTypes.Medic ||
                        unit.unitType == UnitTypes.Mechanic ||
                        (unit.actionTarget != null && unit.actionTarget.name == unit.priorityList[0]) ||
                        unit.actionTarget == targetToAssign ||
                        unit == unitCache ||
                        (unit.actionTarget != null && unit.priorityList.IndexOf(targetToAssign.name) > unit.priorityList.IndexOf(unit.actionTarget.name)))
                    {
                        Debug.Log(unit + " cannot be assigned a target");
                        continue;
                    }

                    // Otherwise, if the target matches the unit's current level of priority, assign it
                    else if (targetToAssign.name == unit.priorityList[i])
                    {
                        Debug.Log("Assigning target to " + unit);
                        unit.actionTarget = targetToAssign;
                        targetQueue.Remove(targetToAssign);
                        break;
                    }

                    // If nothing is assigned, cache the current unit and move current target to back of the queue
                    else
                    {
                        Debug.Log("Nothing Assigned");
                        targetQueue.Remove(targetToAssign);
                        targetQueue.Add(targetToAssign);
                        unitCache = unit;
                    }
                }

                unitCache = null;
            }
        }

        else
            Debug.Log("No targets to assign");
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
