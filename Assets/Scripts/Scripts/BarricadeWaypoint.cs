using UnityEngine;
using System.Collections;

public class BarricadeWaypoint : MonoBehaviour 
{
    public bool occupied;
    public RefactoredBarricade belongsTo;
	public GameObject resident;                    // Reference to the object occupying the barricade

    private int colliderIndex;
	
	// Use this for initialization
	void Awake () 
    {
        belongsTo = GetComponentInParent<RefactoredBarricade>();
        occupied = false;
	}
}
