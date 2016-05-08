using UnityEngine;
using System.Collections;

public class CameraFacingBillboard : MonoBehaviour {

    public Camera m_Camera;

    void Start()
    {
        GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
        m_Camera = camera.GetComponent<Camera>();
    }

    void Update()
    {
        transform.LookAt(transform.position + m_Camera.transform.rotation * Vector3.forward,
            m_Camera.transform.rotation * Vector3.up);
    }
}
