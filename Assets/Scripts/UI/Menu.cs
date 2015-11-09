using UnityEngine;
using System.Collections;

/* USAGE:
 * ======================================
 * Used to pass the IsOpen value to menu panels
 * Sets if menu panels are open, interactable 
 * or block raycasts
 * ======================================
 * 
 * Date Created: 21 Aug 2015
 * Last Modified: 21 Aug 2015
 * Authors: Francisco Carrera
 */

[RequireComponent(typeof (Animator))]
[RequireComponent(typeof (CanvasGroup))]

public class Menu : MonoBehaviour {

	public Animator animator;

	private CanvasGroup canvasGroup;

	// Get Set bool IsOpen
	public bool IsOpen
	{
		get { return animator.GetBool("IsOpen"); }
		set { animator.SetBool("IsOpen", value); }
	}
	
	public void Awake(){
		animator = GetComponent<Animator>();
		canvasGroup = GetComponent<CanvasGroup>();

		// Set offset on awake to zero, allows menues to be moved in editor
		var rect = GetComponent<RectTransform>();
		rect.offsetMax = rect.offsetMin = new Vector2 (0, 0);
	}

	// Update is called once per frame
	void Update () {
		// If the state not open, make menu not interactable / block raycasts 
		if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Open"))
		{
			canvasGroup.blocksRaycasts = canvasGroup.interactable = false;
		}
		// Otherwise, make it interactable / block raycasts
		else
		{
			canvasGroup.blocksRaycasts = canvasGroup.interactable = true;
		}
	}
}
