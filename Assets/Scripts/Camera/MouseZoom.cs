using UnityEngine;
using System.Collections;

public class MouseZoom : MonoBehaviour {

	public float perspectiveZoomSpeed = 0.5f;        // The rate of change of the field of view in perspective mode.
	public float orthoZoomSpeed = 0.5f;        // The rate of change of the orthographic size in orthographic mode.
	
	[Range(0.1f,179.9f)]
	public float maxZoom = 179.9f;
	[Range(0.1f,179.9f)]
	public float minZoom = 0.1f;
	
	private Camera cam;
	
	void Awake(){
		cam = GetComponent<Camera>();
	}

	void Update () {
		// If the camera is disabled, do nothing
		if (!cam.enabled)return;

		cam.fieldOfView -= Input.GetAxis("Mouse ScrollWheel") * perspectiveZoomSpeed;
		
		// Clamp the field of view to make sure it's between 0 and 180.
		cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, minZoom, maxZoom);
	}
}
