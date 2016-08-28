using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.SceneManagement;

/*
 * USAGE
 * ========================
 * Singleton design pattern
 * Manages the transitions between levels
 * Does this using the info from the current scene's LevelManager
 * ========================
 * 
 * Date Created: 18 June 2016
 * Last Modified: 18 June 2016
 * Authors: Andrew Tully
 */

public static class GameManager
{
    public static GameObject[] playerLoadout = new GameObject[7];
    public static List<GameObject> availableUnits;                             // List of units available to the player
    public static int[] loadoutIndex = { -1, -1, -1, -1, -1, -1, -1 };

    public static string[] unitToSpawn = 
        {
        "Marksman",
        "Mechanic",
        "Medic",
        "Heavy Trooper"
        };

    [HideInInspector]
    // Tracks unlocked levels
    // Size of 11, as there are 11 scenes in the game
    // First three elements are unlocked by default; main menu, level select, and first level
    public static bool[] unlockedLevels =
        {
        true,
        true,
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false
        };

    // Following region handles the tracking of objectives and transition conditions
    #region Objectives & Transitions

    static void SwitchScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    #endregion

    // Following region allows for units to be assigned to a loadout, and spawned
    #region Player Unit Management

    /* Function: Sets the player character index to use 
    * Parameters: player character id to spawn, order in which to spawn character
    * Returns: Void
    */
    public static void AssignLoadoutIndex(int characterIndex, int orderIndex)
    {
        loadoutIndex[orderIndex] = characterIndex;

    }

    /* Function: Spawns all units in the loadout into the scene
     * Parameters: None
     * Returns: None
     */
    public static void SpawnPlayerUnits(RefactoredBarricade evacShuttle)
    {
        for (int i = 0; i < playerLoadout.Length; i++)
        {
            if (playerLoadout[i] != null)
            {
                playerLoadout[i].transform.position = evacShuttle.frontWaypoints[i].transform.position;
                playerLoadout[i].SetActive(true);
            }        
        }
    }

    /* Function: Gives each element of the control UI a reference to it's relevant unit
     * Parameters: None
     * Returns: None
     */
    public static void AssignLoadoutUI(GameObject UI_playerPanel, InputManager IM)
    {
        Button[] b = UI_playerPanel.GetComponentsInChildren<Button>();                                       // Gets each button in the canvas
        int i;

        for (i = 0; i < playerLoadout.Length; i++)
        {
            if (playerLoadout[i] != null)
            {
                PlayerUnitControl param = playerLoadout[i].GetComponent<PlayerUnitControl>();     // Cache the character controller to be added

                b[i].onClick.RemoveAllListeners();                                                          // Remove all previous listeners
                b[i].onClick.AddListener(delegate { IM.SetTarget(param); });                                // Add a new listener with the cached controller
                HpBarManager hpData = b[i].GetComponent<HpBarManager>();
                param.hpBar = hpData;
            }
        }
    }

    #endregion

    // Handles save & load logic
    #region Data Management

    public static void Save()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(Application.persistentDataPath + "/gameInfo.dat", FileMode.Open);

        // Create save data container
        PlayerData saveData = new PlayerData();

        // Record data in container
        saveData.savedLoadout = playerLoadout;
        saveData.savedAvailableLevels = unlockedLevels;

        bf.Serialize(file, saveData);
        file.Close();
    }

    public static void Load()
    {
        if (File.Exists(Application.persistentDataPath + "/gameInfo.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/gameInfo.dat", FileMode.Open);
            PlayerData loadData = (PlayerData)bf.Deserialize(file);
            file.Close();

            playerLoadout = loadData.savedLoadout;
            unlockedLevels = loadData.savedAvailableLevels;
        }
    }

    #endregion
}

// Data containing to write save data to a file
[Serializable]
class PlayerData
{
    // Save loadout
    public GameObject[] savedLoadout = new GameObject[7];

    // Save unlocked levels
    public bool[] savedAvailableLevels = new bool[11];
}