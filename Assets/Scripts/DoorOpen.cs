using UnityEngine;
using System.Collections;

public class DoorOpen : MonoBehaviour 
{		
	public float speed;

    [Tooltip ("Radius in which a unit is checked for eligibility to open a door")]
    public float triggerRadius = 3f;

    [Tooltip ("Distance a unit must be within from the door in order for it to open")]
    public float openTreshold = 1f;

    [Tooltip("How many seconds until the door closes")]
    public float closeTimer = 3f;

    private bool open = false;
    private Vector3 destination;
    private int playerMask;
	private Vector3 startMarker;
    private Vector3 originalPosition;

    void Awake()
    {
        playerMask = LayerMask.GetMask("Player");
    }

	void Start()
    {
		originalPosition = transform.localPosition;
	}

    void Update()
    {
        CheckTrigger();
    }

    void CheckTrigger()
    {
        Collider[] possibleTriggers = Physics.OverlapSphere(transform.position, triggerRadius, playerMask);

        foreach (Collider currTrigger in possibleTriggers)
        {
            // Valid target to open the door
            if (currTrigger.tag == "Player")
            {

                float distanceToDoor = Vector3.Distance(currTrigger.gameObject.transform.position, transform.position);

                //Debug.Log("Distance to Door: " + distanceToDoor);
                //Debug.Log("Is Door Open: " + open);

                // If target is halfway through the trigger radius, open the door
                if (distanceToDoor <= openTreshold && !open)
                {
                    StartCoroutine("OpenDoor");
                }
            }
        }
    }

    IEnumerator OpenDoor()
    {
        Debug.Log("Opening Door");
        float step = speed * Time.deltaTime;

        open = true;
        startMarker = transform.position;
        destination = new Vector3(startMarker.x, startMarker.y + 3, startMarker.z);
        transform.position = Vector3.Lerp(startMarker, destination, speed);
        yield return new WaitForSeconds(closeTimer);
        yield return StartCoroutine("CloseDoor");
    }

    IEnumerator CloseDoor()
    {
        Debug.Log("Closing Door");
        open = false;
        startMarker = transform.position;
        transform.localPosition = Vector3.Lerp(startMarker, originalPosition, speed);
        yield return null;
    }
}
