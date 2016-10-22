using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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
    public Menu LoadoutMenu;
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

    public void MenuToShow(Menu menu)
    {
        currentMenu = menu;
    }

    public void SetLevelToLoad(int parameter)
    {
        if (GameManager.unlockedLevels[parameter] == true)
        {
            launchButton.onClick.RemoveAllListeners();
            launchButton.onClick.AddListener(delegate { LoadLevel(parameter); });
        }

        else
            Debug.Log("Level Locked");
    }

    void LoadLevel(int lvlIndex)
    {
        for (int i = 0; i < GameManager.loadoutIndex.Length; i++)
        {
            if (GameManager.loadoutIndex[i] == -1)
            {
                Debug.Log("A full loadout is required to launch the mission");
                break;
            }

            if (i == (GameManager.loadoutIndex.Length - 1))
            {
                Debug.Log("Load level Called");
                SceneManager.LoadScene(lvlIndex);
                Debug.Log("New level loaded");
            }
        }
        
        
    }
}