using UnityEngine;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    // Number of waves in the current level
    public int startCountdown;
    public int levelDuration;
    public int nextLevel;
    public int spawnOffset = 1;

    private GameObject spawnPoint;
    private GameObject UI_playerPanel;
    private InputManager IM;

    void Awake()
    {
        UI_playerPanel = GameObject.Find("PlayerPortraitsMenu");
        IM = GameObject.FindObjectOfType<InputManager>();
        spawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawnPoint");
    }

    void Start()
    {
        Debug.Log("Level Manager Start");
        AssignLoadoutSlot();
        GameManager.AssignLoadoutUI(UI_playerPanel, IM);
        GameManager.SpawnPlayerUnits(spawnPoint, spawnOffset);
    }

    /* Function: Assigns a unit to the appropriate loadout slot
    * Parameters: None
    * Returns: Void
    */
    void AssignLoadoutSlot()
    {
        for (int i = 0; i < GameManager.loadoutIndex.Length; i++)
        {
            int unitIndex = GameManager.loadoutIndex[i];
            string unitToSpawn = GameManager.unitToSpawn[unitIndex];
            GameManager.playerLoadout[i] = Instantiate(Resources.Load(unitToSpawn)) as GameObject;
            GameManager.playerLoadout[i].SetActive(false);
            Debug.Log("Unit index: " + unitIndex + ", spawning: " + unitToSpawn);
        }
    }

    IEnumerator LevelCompleteCountdown()
    {
        yield return new WaitForSeconds(startCountdown);
        // Start spawners
        yield return new WaitForSeconds(levelDuration);
        LevelCompleted();
    }

    void LevelCompleted()
    {
        Debug.Log("Level Complete");
        // Stop Spawning
        // Pop up completed message
        // Update completed level in save file
        // Deactivate all enemies
        // Promp player to continue?
        // Load next level
    }
}