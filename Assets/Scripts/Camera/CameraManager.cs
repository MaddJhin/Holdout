using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour {

	public Camera thirdPersonCam; 				// Holds the main camera used in third person view
	public Camera tacticalCam;					// Hold the secondary ortographic camera for tactical view

	// Use this for initialization
	void Start () {
		thirdPersonCam.enabled = true;
		tacticalCam.enabled = false;
	}

	void ChangePerspective (){
		thirdPersonCam.enabled = !thirdPersonCam.enabled;
		tacticalCam.enabled = !tacticalCam.enabled;
	}
}
