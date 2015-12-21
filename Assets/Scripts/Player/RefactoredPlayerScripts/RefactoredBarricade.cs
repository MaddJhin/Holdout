using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RefactoredBarricade : MonoBehaviour 
{
    public float maxHealth = 100;
    public float sightCheckDelay = .5f;
    [Header("Begins checking for enemies X seconds after spawning")]
    public float checkAfter = 5f;
    public float sightRadius = 20f;
    public float assignTargetDelay = .1f;

    public List<BarricadeWaypoint> frontWaypoints;
    public List<BarricadeWaypoint> backWaypoints;

    public List<PlayerUnitControl> residentList = new List<PlayerUnitControl>();
    UnitStats stats;
    public List<GameObject> targetQueue = new List<GameObject>();
    LayerMask enemyMask;

    PlayerUnitControl unitCache;            // Used to skip searches on subsequent identical units

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
    }

    void Update()
    {
        if (residentList.Count > 0 && targetQueue.Count > 0)
        {
            Debug.Log("Assigning Targets");
            AssignTarget();
        }

        else
        {
            Debug.Log("Cannot assign target");
            return;
        }
    }

    #region Assign Waypoints

    public void AssignFrontWaypoint(GameObject unit)
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
                }
            }
        }
    }

    public void AssignRearWaypoint(GameObject unit)
    {
        PlayerMovement barricadeCache;

        if (barricadeCache = unit.GetComponent<PlayerMovement>())
        {
            foreach (var waypoint in backWaypoints)
            {
                if (waypoint.occupied == false)
                {
                    if (barricadeCache.targetWaypoint != null)
                    {
                        barricadeCache.targetWaypoint.occupied = false;
                        barricadeCache.targetWaypoint.resident = null;
                    }

                    barricadeCache.targetWaypoint = waypoint;
                    waypoint.occupied = true;
                    waypoint.resident = unit;
                    residentList.Add(unit.GetComponent<PlayerUnitControl>());
                }
            }
        }
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
        GameObject targetToAssign = targetQueue[0];
        targetToAssign.name = targetToAssign.name.Replace("(Clone)", "");

        // Loop to iterate through each level of priority
        for (int i = 0; i < 4; i++)
        {
            Debug.Log("Checking Level " + i + " of priority");
            // Loop through each unit at the barricade, and check it at level N of priority
            foreach (var unit in residentList)
            {
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

    #endregion

    #region Designer Readability Methods

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRadius);
    }

    #endregion
}
