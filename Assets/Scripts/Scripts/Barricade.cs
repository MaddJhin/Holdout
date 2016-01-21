using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Barricade : MonoBehaviour 
{
    public float maxHealth = 100;

    public List<BarricadeWaypoint> frontWaypoints;
    public List<BarricadeWaypoint> backWaypoints;

    UnitStats stats;

	// Use this for initialization
	void Start () 
    {
        stats = GetComponent<UnitStats>();
        BarricadeWaypoint[] temp = GetComponentsInChildren<BarricadeWaypoint>();
        stats.maxHealth = maxHealth;

        foreach(var curr in temp)
        {
            //curr.waypointOwner = this;

            if (curr.tag == "Front Waypoint")
                frontWaypoints.Add(curr);

            else
                backWaypoints.Add(curr);
        }
	}

    void Awake()
    {

    }

    public void AssignFrontWaypoint(GameObject unit)
    {
        PlayerMovement barricadeCache;

        if (barricadeCache = unit.GetComponent<PlayerMovement>())
        {
            foreach (var waypoint in frontWaypoints)
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
                }
            }
        }
    }

	public void AssignRearWaypoint(GameObject unit) {
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
				}
			}
		}
	}
}
