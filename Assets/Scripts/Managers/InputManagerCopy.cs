using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InputManagerCopy : MonoBehaviour {

	public float surfaceOffset = 1.5f;
	public PlayerMovement setTargetOn;

    private BarricadeWaypoint waypoint_cache;
    private BarricadeWaypoint[] waypointList;
    private List<Light> waypointMarkerList;

    #region Singleton
    private static InputManagerCopy _instance;

    public static InputManagerCopy instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<InputManagerCopy>();

                DontDestroyOnLoad(_instance.gameObject);
            }

            return _instance;
        }
    }

    void Awake()
    {
        if (_instance == null)
        {

            Debug.Log("Creating singleton");
            // If this instance is the first in the scene, it becomes the singleton
            _instance = this;
            DontDestroyOnLoad(this);
        }

        else
        {
            // If another Singleton already exists, destroy this object
            if (this != _instance)
            {
                Debug.Log("Destroying invalid singleton");
                Destroy(this.gameObject);
            }
        }

        waypointMarkerList = new List<Light>();
    }
    #endregion

    void Start()
    {
        waypointList = FindObjectsOfType<BarricadeWaypoint>();

        foreach (var waypoint in waypointList)
        {
            waypointMarkerList.Add(waypoint.GetComponent<Light>());
            Debug.Log(waypoint);
        }
    }

    void Update () {
		// Run when user clicks
		if (!Input.GetMouseButtonDown(0))
		{
			return;
		}

		// Raycast to mouse
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;

		// Return if nothing was hit
        if (!Physics.Raycast(ray, out hit))
        {
            return;
        }

        // If player selects barricade, call SetTarget on the selected player character, checking for null.
        else
        {
            waypoint_cache = hit.collider.gameObject.GetComponent<BarricadeWaypoint>();

            if (hit.collider.tag == "Barricade" && setTargetOn != null)
            {
                // Invoke UI
            }
        }

		// Place target reticule 
		//transform.position = hit.point + hit.normal*surfaceOffset;
	}

	public void SetTarget (PlayerMovement player)
    {
        StartCoroutine("EnableWaypointLights");
        Debug.Log("Target set to: " + player);
		setTargetOn = player;
	}
}
