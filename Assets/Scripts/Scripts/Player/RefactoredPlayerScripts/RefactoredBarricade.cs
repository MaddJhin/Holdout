using UnityEngine;
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
        //InvokeRepeating("CheckForEnemies", checkAfter, sightCheckDelay);
        InvokeRepeating("CheckForRetreat", checkAfter, 0.35f);
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
    void CheckForRetreat()
    {
        if (stats.currentHealth < 1 && residentList.Count > 0 && gameObject.activeInHierarchy)
        {
            for (int i = 0; i < residentList.Count; i++)
            {
                StartCoroutine(residentList[i].RetreatFrom(this));
            }

            gameObject.SetActive(false);
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

    void CheckForEnemies()
    {
        if (residentList.Count > 0)
        {
            Collider[] targetsInRange = Physics.OverlapSphere(transform.position, sightRadius, enemyMask);

            // If there are enemies in range, ping residents to perform a sight check
            if (targetsInRange.Length > 0)
            {
                for (int i = 0; i < residentList.Count; i++)
                {
                    if (residentList[i].unitType == UnitTypes.Trooper ||
                        residentList[i].unitType == UnitTypes.Marksman)
                        StartCoroutine(residentList[i].CheckForTarget(targetsInRange));

                    else if (residentList[i].unitType == UnitTypes.Mechanic)
                        StartCoroutine(residentList[i].Slow(targetsInRange));
                }
            }
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
