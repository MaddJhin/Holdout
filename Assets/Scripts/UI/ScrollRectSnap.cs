using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/*USAGE:
 * ==================================================
 * Used to snap a scroll rect to the child objects
 * Child objects must be equally spaced, 
 * anchored in the middle of the scroll rect content panel. 
 * ===================================================
 * 
 * Date Created: 13 Aug 2015
 * Last Modified: 13 Aug 2015
 * Authors: Francisco Carrera, (credit to denise from Respect Studios)
*/


public class ScrollRectSnap : MonoBehaviour {

	// Public Variables
	[Tooltip ("Content panel to slide. Must be set to unrestricted slide, one point anchor")]
	public RectTransform contentPanel; 	// Holds the content panel to scroll
	[Tooltip ("Child objects of the content panel to snap to")]
	public Button[] bttns;				// Child elements of the content panel to snap to
	[Tooltip ("Empty Object centered and anchored same as the content panel")]
	public RectTransform center; 		// Centered object used to compare distance for each button
	public float snappingSpeed = 5f;

	// Private Variables
	private float[] distance; 			// All buttons' distance to the center
	private bool draggging = false;		// True when dragging pannel
	private int bttnsDistance; 			// Holds the distance between the buttons
	private int minButtnNum; 			// Holds the number of the button with the smallest distance to center

	// Use this for initialization
	void Start () {
		// Set length of distance array to button array lenght 
		int bttnLenght = bttns.Length;
		distance = new float[bttnLenght];

		// Get Distance between buttons (second button position minus first button position)
		bttnsDistance = (int)Mathf.Abs(bttns[1].GetComponent<RectTransform>().anchoredPosition.x 
		                               - bttns[0].GetComponent<RectTransform>().anchoredPosition.x);
	}
	
	// Update is called once per frame
	void Update () {

		// Get the distance between each button and the center
		for (int i = 0; i < bttns.Length; i++)
		{
			distance[i] = Mathf.Abs (center.transform.position.x - bttns[i].transform.position.x);
		}

		// Get the array index of the closest child element to the center
		float minDistance = Mathf.Min(distance);
		for (int a = 0; a < bttns.Length; a++)
		{
			if (minDistance == distance[a])
			{
				minButtnNum = a;
			}
		}

		// When not dragging, lerp to the closest button position (it's index times distance betwen buttons)
		if (!draggging)
		{
			LerpToBttn(minButtnNum * -bttnsDistance);
		}
	}

	// Pass the desired x position and lerp the content panel anchor point to position
	void LerpToBttn(int position){
		float newX = Mathf.Lerp (contentPanel.anchoredPosition.x, position, Time.deltaTime * snappingSpeed);
		Vector2 newPosition = new Vector2 (newX, contentPanel.anchoredPosition.y);

		contentPanel.anchoredPosition = newPosition;
	}

	// Use Event Trigger Component to detect the start of a drag and set dragging true
	public void StartDrag(){
		draggging = true;
	}

	// Use Event Trigger Component to detect the end of a drag and set dragging false
	public void EndDrag(){
		draggging = false;
	}
}

