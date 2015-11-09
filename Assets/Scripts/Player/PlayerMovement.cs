using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/* USED BY:
 * ==============
 * PlayerControlMarksman.cs
 * PlayerControlTrooper.cs
 * PlayerControlMedic.cs
 * PlayerControlMechanic.cs
 * ==============
 * 
 * USAGE:
 * ======================================
 * Targets and moves player characters
 * Takes it's values from control script
 * Enables modularity
 * ======================================
 * 
 * Date Created: 27 Jul 2015
 * Last Modified: 	8 Aug 2015
 * Authors: Francisco Carrera
 */

public class PlayerMovement : MonoBehaviour {

    [HideInInspector]
	public Transform actionTarget;

    [HideInInspector]
	public List<string> priorityList = new List<string>();
	public Transform targetBarricade;
    public BarricadeWaypoint targetWaypoint;

    [HideInInspector]
	public float maxBarricadeDistance = 10f;

    [HideInInspector]
	public float sightDistance = 10;

	NavMeshAgent agent;
	PlayerAction actionControl;
	Animator m_Animator;
	float m_TurnAmount;
	float m_ForwardAmount;
	UnitStats targetStats;
	Transform self;
	bool playerMove = false;
    int enemyMask;

    void Awake()
    {
        enemyMask = LayerMask.GetMask("Enemy");
    }
	
	// Use this for initialization
	void Start () {
		agent = GetComponentInChildren<NavMeshAgent>();
		m_Animator = GetComponent<Animator>();
		actionControl = GetComponent<PlayerAction>();
		targetBarricade = this.transform;
		self = this.GetComponent<Transform>();
	}

	// Update is called once per frame
	void Update () {

		//Update animation and next target every frame.
		

		//Set action target. Make sure it is in range.  
		SetTarget();
		if (actionTarget != null && Vector3.Distance(actionTarget.position, gameObject.transform.position) > sightDistance) 
		{
			actionTarget = null;
		}

		// If the attack target isn't active, it's considered null
		else if (actionTarget != null && actionTarget.gameObject.activeInHierarchy == false)
		{
			Debug.Log("Inactive Found");
			actionTarget = null;
			Debug.Log(actionTarget);
		}

		if ( Vector3.Distance (targetBarricade.position, transform.position) > maxBarricadeDistance)
		{
			Debug.Log("Tether reached. Returning to barricade");
			SetDestination(targetBarricade);
		}

		// If the player issues a move command, the playerUnit shoudl continue until destination
		if (playerMove)
		{
			if (!agent.pathPending)
			{
				if (agent.remainingDistance <= agent.stoppingDistance)
				{
					if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
					{
						playerMove = false;
					}
				}
			}
			return; // No more updates until agent reaches player designated destination
		}

		// If there is an available action target move to that otheriwse go to the last choosen barricade. 
		if (actionTarget != null) 
		{
			if(agent.enabled)agent.SetDestination(actionTarget.position);
			transform.LookAt(actionTarget.position);
		}
		else
		{
			if(agent.enabled)agent.SetDestination(targetBarricade.position);
		} 
	}

	void Move()
	{
		// Check to see if the agent is not already moving, if so set a new destination
		if (!agent.pathPending)
		{
			if (agent.remainingDistance <= agent.stoppingDistance)
			{
				if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
				{
					if(agent.enabled)agent.SetDestination (actionTarget.position);
				}
			}
		}
	}

	public void SetDestination (Transform target){
		targetBarricade = target;
		if(agent.enabled)
		{
			agent.ResetPath();
			agent.SetDestination (target.position);
		}
		playerMove = true;
	}

	void SetTarget (){
		float curDistance = sightDistance;
		// Grab all available targets around character
		Collider[] possibleTargets = Physics.OverlapSphere (transform.position, sightDistance, enemyMask);

		// Search possible targets, choose clossest one with highest priority first.
		// Stop searching after getting the closest target avaialble of the highest priority is found
		foreach (string targetTag in priorityList)
		{
			foreach (Collider possibleTarget in possibleTargets)
			{
				if (possibleTarget.gameObject.tag == targetTag)
				{
					float distance = Vector3.Distance(possibleTarget.transform.position, transform.position);
					if (distance < curDistance)
					{
						if(possibleTarget.transform != self)
						{
							curDistance = distance;
							actionTarget = possibleTarget.transform;
						}
					}
				}
			}
			// If actionTarget is not null, target found
			// Assign target to attack script, return to not continue down priority list.
			if (actionTarget != null) 
			{
				actionControl.actionTarget = actionTarget;
				return;
			}

		}
	}

    public UnitStats SetHealTarget(Vector3 startPos, float range, LayerMask targetMask, string mode)
    {
        Collider[] possibleTargets = Physics.OverlapSphere(startPos, range, targetMask);

        // First list holds game objects found as potential heal targets
        // Second list contaisn their health values
        // Correspending indexes in the list allows for easy searching
        List<UnitStats> statListCache = new List<UnitStats>();
        List<float> healthList = new List<float>();
        UnitStats statCache;

        if (possibleTargets != null)
        {
            foreach (Collider possibleTarget in possibleTargets)
            {
                // If the potential target has UnitStats cache the object and it's current health
                if (statCache = possibleTarget.GetComponent<UnitStats>())
                {
                    if (statCache.currentHealth < statCache.maxHealth && statCache.gameObject != gameObject && gameObject.tag == mode)
                    {
                        statListCache.Add(statCache);
                        healthList.Add(statCache.currentHealth);
                    }
                }

                else
                    Debug.Log("No UnitStats available for target");
            }

            // Find the lowest health value out of the potential targets
            float min = healthList[0];
            int minIndex = 0;

            for (int i = 1; i < statListCache.Count; ++i)
            {
                if (healthList[i] < min)
                {
                    min = healthList[i];
                    minIndex = i;
                }
            }

            return statListCache[minIndex];
        }

        return null;
    }
	

	void HighlightPlayer (){

	}
}
	