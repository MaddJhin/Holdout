using UnityEngine;
using System.Collections;

public class CameraScroll : MonoBehaviour {

	public Transform cameraTarget; 				// Hold the object the camera view targets
	public float speed = 10.0F;					// Scroll Speed

    // Clamp Fields
    [Header("Camera Movement Constraints")]
    public float MIN_X;
    public float MAX_X;
    public float MIN_Z;
    public float MAX_Z;
	
	private Camera cam;							// Holds current cam reference
    float vertical;
    float side;
	
	void Awake(){
		cam = GetComponent<Camera>();
	}

	void Update() {
		// If the camera is disabled, do nothing
		if (!cam.enabled)return;

        //Get the Input for the scrolling movement

#if UNITY_ANDROID
        if (Input.touchCount > 0)
        {
            side = Input.touches[0].deltaPosition.x * speed;
            vertical = Input.touches[0].deltaPosition.y * speed;
        }

#endif

#if UNITY_EDITOR
        vertical = Input.GetAxis("Vertical") * speed;
        side = Input.GetAxis("Horizontal") * speed;
#endif
 
		// Adjust for framerate differences
		vertical *= Time.deltaTime;
		side *= Time.deltaTime;

		// Move camera based on world space to ignore camera rotation
		cameraTarget.Translate(Vector3.forward * vertical, Space.World);
		cameraTarget.Translate(Vector3.right * side, Space.World);
        cameraTarget.transform.position = new Vector3(
                                                       cameraTarget.transform.position.x,
                                                       cameraTarget.transform.position.y,
                                                       cameraTarget.transform.position.z);
    }
}
