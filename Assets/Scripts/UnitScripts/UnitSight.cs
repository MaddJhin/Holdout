using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/* USAGE:
 * =========================
 * Used to detect when a player object is in range
 * Sets that object as the current target
 * Selected target used by the unit's AI script
 * =========================
 * 
 * NOTES:
 * =========================
 * May need to be removed and merged with specific unit AI scripts
 * Will allow for better control over how unit's prioritise targets
 * General solution may be too broad
 * =========================
 * 
 * Date Created: 27 May 2015
 * Last Modified: 29 May 2015
 * Authors: Andrew Tully
 */

public class UnitSight : MonoBehaviour
{
    [HideInInspector]
    public bool targetInRange;

    [HideInInspector]
    public GameObject actionTarget;

    [HideInInspector]
    public string defaultTarget;

    [HideInInspector]
    public float sightDistance;

    // Stores priority of GameObject tags
    // First element is the highest priority
    // E.G: if element[0] contains the string "Player", the "Player" tag is highest priority
    [HideInInspector]
    public List<string> priorityList = new List<string>();

    [HideInInspector]
    public float targetDistance; // Distance between this object and it's target

    private UnitStats targetStats;

    // Setting up LayerMask for Player and Barricades
    private int playerMask;

    void Awake()
    {
        playerMask = LayerMask.GetMask("Player");
    }

	void Start(){
		actionTarget = GameObject.Find(defaultTarget);
	}

    void Update()
    {
        SetTarget();

        // If target doesn't exit, switch to default target
        if (actionTarget == null)
        {
            actionTarget = GameObject.Find(defaultTarget);
        }

        // If the target is inactive, set it to null
        else if (actionTarget.activeInHierarchy == false)
            actionTarget = null;

        else if ((actionTarget.tag == "Player") &
                    (Vector3.Distance(actionTarget.transform.position, gameObject.transform.position) > sightDistance))
        {
            actionTarget = null;
        }

        // Calculate distance every update
        else
        {
            targetDistance = Vector3.Distance(actionTarget.transform.position, gameObject.transform.position);

        }
    }

    void SetTarget()
    {
        float curDistance = sightDistance;
        GameObject tempTarget = null;

        // Grab all available targets around the unit
        Collider[] possibleTargets = Physics.OverlapSphere(transform.position, sightDistance, playerMask);

        foreach (string targetTag in priorityList)
        {
            foreach (Collider possibleTarget in possibleTargets)
            {
                if (possibleTarget.gameObject.tag == targetTag)
                {
                    float distance = Vector3.Distance(possibleTarget.transform.position, transform.position);
                    if (distance < curDistance)
                    {
                        if (possibleTarget.gameObject != gameObject)
                        {
                            curDistance = distance;
                            tempTarget = possibleTarget.gameObject;
                        }
                    }
                }
            }
            // If actionTarget is not null, target found
            // Assign target to attack script, return to not continue down priority list.
            if (tempTarget != null)
            {
                actionTarget = tempTarget;
                return;
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightDistance);
    }
}
