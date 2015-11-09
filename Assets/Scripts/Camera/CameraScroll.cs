using UnityEngine;
using System.Collections;

public class CameraScroll : MonoBehaviour {

	public Transform cameraTarget; 				// Hold the object the camera view targets
	public float speed = 10.0F;					// Scroll Speed
	
	private Camera cam;							// Holds current cam reference
	
	void Awake(){
		cam = GetComponent<Camera>();
	}

	void Update() {
		// If the camera is disabled, do nothing
		if (!cam.enabled)return;

		//Get the Input for the scrolling movement
		float vertical = Input.GetAxis("Vertical") * speed;
		float side = Input.GetAxis("Horizontal") * speed;

		// Adjust for framerate differences
		vertical *= Time.deltaTime;
		side *= Time.deltaTime;

		// Move camera based on world space to ignore camera rotation
		cameraTarget.Translate(Vector3.forward * vertical, Space.World);
		cameraTarget.Translate(Vector3.right * side, Space.World);
	}
}
