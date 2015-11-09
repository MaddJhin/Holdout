using UnityEngine;
using UnityEngine.UI;
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

public class WorldMenuManager : MonoBehaviour
{

    public Menu planetMenu;
    public Button launchButton;

    private Menu currentMenu;

    void Start()
    {
        currentMenu = planetMenu;
        // Open Starting Menu
        ShowMenu(currentMenu);
    }

    // used to open new menus. Called by buttons
    public void ShowMenu(Menu menu)
    {
        // Close any current menus by setting IsOpen to false
        if (currentMenu != null) currentMenu.IsOpen = false;

        // Set new menu and set IsOpen to true
        currentMenu = menu;
        currentMenu.IsOpen = true;
    }

    public void SetLevelToLoad(int parameter)
    {
        launchButton.onClick.RemoveAllListeners();
        launchButton.onClick.AddListener(delegate { LoadLevel(parameter); });
        launchButton.onClick.AddListener(delegate { GameManager.instance.AssignLoadoutSlot(); });
    }

    void LoadLevel(int lvlIndex) {
        Application.LoadLevel(lvlIndex);
    }
}