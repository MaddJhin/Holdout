using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputManager : MonoBehaviour {

	public Camera thirdPersonCam; 				// Holds the main camera used in third person view
	public Camera tacticalCam;					// Hold the secondary ortographic camera for tactical view
	public PlayerMovement setTargetOn;
    public float lerpSpeed = 1;

	private MenuManager menuManager;
	private bool thirdPerson;
	private Camera activeCam;
    private BarricadeWaypoint waypoint_cache;
    private BarricadeWaypoint[] waypointList;
    private List<Light> waypointMarkerList;
	private Button frontWaypointButton;
	private Button rearWaypointButton;
    private float startTime;

    private Renderer rendCache;                 // Used to cache the selected unit's renderer
    private Color colorCache;                   // Used to cache the outline color of selected unit
    private Color newColorCache;                // Used to cache the new outline color

    #region Singleton
    private static InputManager _instance;

    public static InputManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<InputManager>();

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
    }
    #endregion

    void Start()
    {
		menuManager = MenuManager.instance;
		thirdPersonCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
		tacticalCam = GameObject.FindGameObjectWithTag("TacticalCamera").GetComponent<Camera>();
		frontWaypointButton = GameObject.FindGameObjectWithTag("FrontWaypointButton").GetComponent<Button>();
		rearWaypointButton = GameObject.FindGameObjectWithTag("RearWaypointButton").GetComponent<Button>();

		thirdPersonCam.enabled = true;
		tacticalCam.enabled = false;
		thirdPerson = true;
		activeCam = thirdPersonCam;
        startTime = Time.time;
    }

    void Update () {
		// Run when user clicks
		if (!Input.GetMouseButtonDown(0))
		{
			return;
		}

		// Raycast to mouse
		Ray ray = activeCam.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;

		// Return if nothing was hit
        if (!Physics.Raycast(ray, out hit))
        {
            return;
        }

        // If player selects barricade, call SetTarget on the selected player character, checking for null.
        else if (thirdPerson)
        {
			if (!EventSystem.current.IsPointerOverGameObject())
			{
				if (hit.collider.tag == "Barricade" && setTargetOn != null)
				{
					Barricade barricade = hit.collider.gameObject.GetComponent<Barricade>();
					SetWaypointButtons(barricade);
					
					menuManager.ShowMenu(menuManager.waypointMenu);
					Debug.Log("Barricade Found");
				}

				else
				{
					// Remove UI highlight
					setTargetOn = null;
				}
			}
        }
		else if (!thirdPerson)
		{
			Vector3 location = new Vector3 (hit.point.x, thirdPersonCam.transform.parent.position.y, hit.point.z);
			thirdPersonCam.transform.parent.position = location;

			ChangePerspective();
		}
	}

	public void SetTarget (PlayerMovement player)
    {
        Debug.Log("Target set to: " + player);

        // Reset previous target's outline color before setting new target
        if (rendCache != null)
            rendCache.material.SetColor("_OutlineColor", colorCache);

        // Set new target and cache their renderer for future color changes
		setTargetOn = player;
        rendCache = player.gameObject.GetComponentInChildren<Renderer>();

        Debug.Log("Cached renderer " + rendCache);

        // Create new color from cache, change alpha, and apply
        if (rendCache != null)
        {
            colorCache = rendCache.material.GetColor("_OutlineColor");
            newColorCache = colorCache;
            newColorCache.a = (255F);
            rendCache.material.SetColor("_OutlineColor", newColorCache);
        }
	}

	public void Move (BarricadeWaypoint target){
		if (setTargetOn.targetWaypoint != null)
		{
			setTargetOn.targetWaypoint.occupied = false;
		}
		setTargetOn.targetWaypoint = target;
		target.occupied = true;
		setTargetOn.SetDestination(target.transform);
	}

	public void ChangePerspective (){
		thirdPersonCam.enabled = !thirdPersonCam.enabled;
		tacticalCam.enabled = !tacticalCam.enabled;
		thirdPerson = !thirdPerson;
		if (thirdPerson)
		{
			activeCam = thirdPersonCam;
		}
		else
		{
			activeCam = tacticalCam;
		}
	}

	void SetWaypointButtons (Barricade barricade){		
		for (int f = 0; f < barricade.frontWaypoints.Count; f++)
		{
			if (barricade.frontWaypoints[f].occupied == false)
			{
				EnableButton(frontWaypointButton);
				
				AddListeners(frontWaypointButton,barricade.frontWaypoints[f]);
				break;
			}
			
			Debug.Log("All front waypoints ocuupied");
			DisableButton(frontWaypointButton);
		}
		
		for (int b = 0; b < barricade.backWaypoints.Count; b++)
		{
			if (barricade.backWaypoints[b].occupied == false)
			{
				EnableButton(rearWaypointButton);
				
				AddListeners(rearWaypointButton, barricade.backWaypoints[b]);
				break;
			}
			
			Debug.Log("All rear waypoints ocuupied");
			DisableButton(rearWaypointButton);
		}
	}

	void AddListeners (Button b, BarricadeWaypoint parameter){
		b.onClick.RemoveAllListeners();
		b.onClick.AddListener(delegate { Move(parameter); });
	}

	void EnableButton(Button b){
		b.interactable = true;
		b.gameObject.SetActive(true);
	}

	void DisableButton(Button b){
		b.interactable = false;
		b.gameObject.SetActive(false);
	}

    public void FocusCamera(GameObject target)
    {
        Vector3 currentLocation = thirdPersonCam.transform.parent.position;
        Vector3 targetlocation = new Vector3(target.transform.position.x, 
                                             thirdPersonCam.transform.parent.position.y, 
                                             target.transform.position.z);

        thirdPersonCam.transform.parent.position = Vector3.Lerp(currentLocation, targetlocation, 1f);
    }
}
