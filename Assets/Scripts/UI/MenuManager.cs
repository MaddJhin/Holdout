using UnityEngine;
using System.Collections;

/* USES:
 * ==============
 * Menu.cs
 * ==============
 * 
 * USAGE:
 * ======================================
 * Used to call method to open menus through 
 * UI buttons. UI button calls the method and 
 * the script sets the animator trigger bools
 * ======================================
 * 
 * Date Created: 21 Aug 2015
 * Last Modified: 21 Aug 2015
 * Authors: Francisco Carrera
 */

public class MenuManager : MonoBehaviour {
	
	public Menu playerPortraitMenu;
	public Menu pauseMenu;
	public Menu waypointMenu;

	private Menu currentMenu;

	#region Singleton
	private static MenuManager _instance;
	
	public static MenuManager instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = GameObject.FindObjectOfType<MenuManager>();
				
				DontDestroyOnLoad(_instance.gameObject);
			}
			
			return _instance;
		}
	}
	
	void Awake()
	{
		if (_instance == null)
		{
			
			Debug.Log("Creating singleton");
			// If this instance is the first in the scene, it becomes the singleton
			_instance = this;
			DontDestroyOnLoad(this);
		}
		
		else
		{
			// If another Singleton already exists, destroy this object
			if (this != _instance)
			{
				Debug.Log("Destroying invalid singleton");
				Destroy(this.gameObject);
			}
		}
	}

	#endregion

	void Start () {
		currentMenu = playerPortraitMenu;
		// Open Starting Menu
		ShowMenu(currentMenu);
	}

	// used to open new menus. Called by buttons
	public void ShowMenu (Menu menu){
		// Close any current menus by setting IsOpen to false
		if (currentMenu != null) currentMenu.IsOpen = false;

		// Set new menu and set IsOpen to true
		currentMenu = menu;
		currentMenu.IsOpen = true;
	}
}
