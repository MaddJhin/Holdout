using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputManager : MonoBehaviour {

	public Camera thirdPersonCam; 				// Holds the main camera used in third person view
	public PlayerUnitControl setTargetOn;
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
    private int touchCountCache;

    private Renderer[] rendCache;                 // Used to cache the selected unit's renderer
    private Renderer targetRend;
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
            // If this instance is the first in the scene, it becomes the singleton
            _instance = this;
        }

        else
        {
            // If another Singleton already exists, destroy this object
            if (this != _instance)
            {
                Destroy(this.gameObject);
            }
        }
    }
    #endregion

    void Start()
    {
		menuManager = MenuManager.instance;
		thirdPersonCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
		frontWaypointButton = GameObject.FindGameObjectWithTag("FrontWaypointButton").GetComponent<Button>();
		rearWaypointButton = GameObject.FindGameObjectWithTag("RearWaypointButton").GetComponent<Button>();

		thirdPersonCam.enabled = true;
		thirdPerson = true;
		activeCam = thirdPersonCam;
        startTime = Time.time;
    }

    void Update ()
    {

        for (int i = 0; i < Input.touchCount; ++i)
        {
            if (Input.GetTouch(i).phase == TouchPhase.Began)
            {
                touchCountCache = Input.GetTouch(i).tapCount;
            }
        }

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
					RefactoredBarricade barricade = hit.collider.gameObject.GetComponent<RefactoredBarricade>();
					SetWaypointButtons(barricade);
					
					menuManager.ShowMenu(menuManager.waypointMenu);
				}
			}
        }

		else if (!thirdPerson)
		{
			Vector3 location = new Vector3 (hit.point.x, thirdPersonCam.transform.parent.position.y, hit.point.z);
			thirdPersonCam.transform.parent.position = location;
		}
	}

	public void SetTarget (PlayerUnitControl player)
    {

        // Reset previous target's outline color before setting new target
        if (targetRend != null && player != setTargetOn)
          targetRend.material.SetColor("_OutlineColor", colorCache);

        // Set new target and cache their renderer for future color changes
        setTargetOn = player;

        if (touchCountCache > 1)
            FocusCamera(player.gameObject);
      
        rendCache = player.gameObject.GetComponentsInChildren<Renderer>();
        
        // Create new color from cache, change alpha, and apply
        if (rendCache != null)
        {
            for (int i = 0; i < rendCache.Length; i++)
            {
                if (rendCache[i].material.HasProperty("_OutlineColor"))
                {
                    targetRend = rendCache[i];

                    if (colorCache != rendCache[i].material.GetColor("_OutlineColor"))
                    {
                        colorCache = rendCache[i].material.GetColor("_OutlineColor");
                        newColorCache = colorCache;
                        newColorCache.a = (255F);
                    }

                    rendCache[i].material.SetColor("_OutlineColor", newColorCache);
                }
            }
        }
        
         
    }

	public void Move (BarricadeWaypoint target, RefactoredBarricade barricade)
    {
		if (setTargetOn.currentWaypoint != null)
		{
			setTargetOn.currentWaypoint.occupied = false;
            setTargetOn.currentBarricade.residentList.Remove(setTargetOn);

            if (setTargetOn.unitType == UnitTypes.Mechanic)
                StartCoroutine(setTargetOn.EndFortify());

            else if (setTargetOn.unitType == UnitTypes.Medic)
                StartCoroutine(setTargetOn.DeactivateHeal());
		}
        setTargetOn.healthRegenRate = 0;
		setTargetOn.currentWaypoint = target;
        setTargetOn.currentBarricade = barricade;
		target.occupied = true;
        target.resident = setTargetOn.gameObject;
        setTargetOn.currentWaypoint = target;
        barricade.residentList.Add(setTargetOn);

        if (setTargetOn.agent.enabled)
        {
            setTargetOn.agent.ResetPath();
            setTargetOn.agent.SetDestination(target.transform.position);
        }
	}

	void SetWaypointButtons (RefactoredBarricade barricade)
    {		
		for (int f = 0; f < barricade.frontWaypoints.Count; f++)
		{
			if (barricade.frontWaypoints[f].occupied == false)
			{
				EnableButton(frontWaypointButton);
				
				AddListeners(frontWaypointButton,barricade.frontWaypoints[f], barricade);
				break;
			}
			
			DisableButton(frontWaypointButton);
		}

        if (barricade.backWaypoints.Count > 1)
        {
            for (int b = 0; b < barricade.backWaypoints.Count; b++)
            {
                if (barricade.backWaypoints[b].occupied == false)
                {
                    EnableButton(rearWaypointButton);

                    AddListeners(rearWaypointButton, barricade.backWaypoints[b], barricade);
                    break;
                }

                DisableButton(rearWaypointButton);
            }
        }

        else
            DisableButton(rearWaypointButton);
	}

	void AddListeners (Button b, BarricadeWaypoint parameter, RefactoredBarricade barricade){
		b.onClick.RemoveAllListeners();
		b.onClick.AddListener(delegate { Move(parameter, barricade); });
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
