﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RefactoredBarricade : MonoBehaviour
{

    #region Barricade Attributes
    [Header("Barricade Attributes")]
    public float selfHealAmount;
    public float healRateSeconds;
    public float sightCheckDelay;

    [Tooltip("Begins checking for enemies, assigning & retreating X seconds after spawning")]
    public float checkAfter;
    public float sightRadius;
    public float assignTargetDelay;

    [Header("Retreat Info")]
    [Tooltip("Barricade that units will retreat to")]
    public RefactoredBarricade retreatBarricade;    
    public List<BarricadeWaypoint> frontWaypoints;
    public List<BarricadeWaypoint> backWaypoints;

    public List<BarricadeWaypoint> retreatPoints;

    [Header("Resident Info")]
    public List<PlayerUnitControl> residentList = new List<PlayerUnitControl>();
    UnitStats stats;
    public List<GameObject> targetQueue = new List<GameObject>();
    public LayerMask enemyMask;
    PlayerUnitControl unit;
    public bool fortified = false;

    [HideInInspector]
    public DoorOpen door;
    #endregion

    #region Caches
    BarricadeWaypoint[] waypointCache;
    RefactoredBarricade retreatBarricadeCache;
    List<BarricadeWaypoint> retreatWaypointsCache;
    PlayerUnitControl unitCache;            // Used to skip searches on subsequent identical units
    Collider coll;
    float baseSelfHealCache;
    public int retreatPointIndexCache;              // Tracks the currently available retreatPoint

    Collider[] enemyBuffer;
    #endregion

    // Use this for initialization
    void Start()
    {
        stats = GetComponent<UnitStats>();
        coll = GetComponent<Collider>();
        door = GetComponent<DoorOpen>();
        waypointCache = GetComponentsInChildren<BarricadeWaypoint>();
        unitCache = null;
        baseSelfHealCache = selfHealAmount;
    }

    void Awake()
    {
        StartCoroutine(CheckForEnemies());
        StartCoroutine(CheckForRetreat());
        InvokeRepeating("HealSelf", checkAfter, healRateSeconds);
    }

    void HealSelf()
    {
        if (stats.currentHealth > 0)
        {
            if (fortified)
                stats.Heal(selfHealAmount);

            else
                stats.Heal(baseSelfHealCache);
        }
    }

    #region Retreat Methods

    // See if conditions for a retreat are satisfied
    IEnumerator CheckForRetreat()
    {
        Debug.Log("Beginning Retreat Checks");
        while (true)
        {
            Debug.Log("Checking Retreat");
            // If destroyed, and units are present; retreat & deactivate
            if (stats != null && stats.currentHealth < 1)
            {
                Debug.Log("Retreating");
                for (int i = 0; i < residentList.Count; i++)
                {
                    StartCoroutine(residentList[i].RetreatFrom(this));
                }

                gameObject.SetActive(false);
            }

            // Otherwise, if destroyed deactivate barricade 
            else if ((stats != null && stats.currentHealth < 1) && gameObject.activeInHierarchy)
            {
                Debug.Log("Deactivating");
                gameObject.SetActive(false);
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    public void GetRetreatPoints(RefactoredBarricade retreatTarget)
    {
        for (int i = 0; i < retreatTarget.backWaypoints.Count; i++)
        {
            retreatPoints.Add(retreatTarget.backWaypoints[i]);
        }

        for (int i = 0; i < retreatTarget.frontWaypoints.Count; i++)
        {
            retreatPoints.Add(retreatTarget.frontWaypoints[i]);
        }

        for (int i = 0; i < retreatTarget.retreatPoints.Count; i++)
        {
            retreatPoints.Add(retreatTarget.retreatPoints[i]);
        }
    }

    #endregion

    #region Sight Checking Algorithm

    IEnumerator CheckForEnemies()
    {
        while (true)
        {
            if (residentList.Count > 0 && (enemyBuffer == null || enemyBuffer.Length <= 0))
            {
                enemyBuffer = Physics.OverlapSphere(transform.position, sightRadius, enemyMask);

                // If there are enemies in range, ping residents to perform a sight check
                if (enemyBuffer.Length > 0)
                {
                    StartCoroutine(AssignTargetsToResidents());
                }
            }

            else if (residentList.Count > 0 && enemyBuffer.Length > 0)
            {
                StartCoroutine(AssignTargetsToResidents());
            }

            yield return new WaitForSeconds(sightCheckDelay);
        }
    }

    IEnumerator AssignTargetsToResidents()
    {
        for (int i = 0; i < residentList.Count; i++)
        {
            if (residentList[i].unitType == UnitTypes.Trooper ||
                residentList[i].unitType == UnitTypes.Marksman)
                StartCoroutine(residentList[i].CheckForTarget(enemyBuffer));

            else if (residentList[i].unitType == UnitTypes.Mechanic)
                StartCoroutine(residentList[i].Slow(enemyBuffer));
        }

        yield return null;
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
