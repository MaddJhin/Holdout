using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    // Number of waves in the current level
    public int startCountdown;
    public int levelDuration;
    public int nextLevel;
    public int spawnOffset = 1;
    public float spawnMilitiaCooldown;
    public RefactoredBarricade evacShuttle;
    public GameObject[] enemySpawners;
    public GameObject[] militiaSpawners;
    public GameObject UICanvas;

    private GameObject spawnPoint;
    private GameObject UI_playerPanel;
    private InputManager IM;
    private List<NewSpawnerRefactored> enemySpawnerCache = new List<NewSpawnerRefactored>();
    private List<NewSpawnerRefactored> militiaSpawnerCache = new List<NewSpawnerRefactored>();

    void Awake()
    {
        UI_playerPanel = GameObject.Find("PlayerPortraitsMenu");
        IM = GameObject.FindObjectOfType<InputManager>();
        spawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawnPoint");

        if (enemySpawners.Length > 0)
        {
            for (int i = 0; i < enemySpawners.Length; i++)
            {
                enemySpawnerCache.Add(enemySpawners[i].GetComponent<NewSpawnerRefactored>());
            }
        }

        if (militiaSpawners.Length > 0)
        {
            for (int i = 0; i < militiaSpawners.Length; i++)
            {
                militiaSpawnerCache.Add(militiaSpawners[i].GetComponent<NewSpawnerRefactored>());
            }
        }
    }

    void Start()
    {
        AssignLoadoutSlot();
        GameManager.AssignLoadoutUI(UI_playerPanel, IM);
        GameManager.SpawnPlayerUnits(evacShuttle);
        StartCoroutine(LevelCompleteCountdown());
        InvokeRepeating("LevelFailed", 0, 1f);
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
        }
    }

    IEnumerator LevelCompleteCountdown()
    {
        yield return new WaitForSeconds(startCountdown);

        // Start spawners
        if (enemySpawnerCache.Count > 0)
        {
            for (int i = 0; i < enemySpawnerCache.Count; i++)
            {
                StartCoroutine(enemySpawnerCache[i].SpawnLoop());
            }
        }

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

        if (UICanvas != null)
            UICanvas.SetActive(false);

        SceneManager.LoadScene(nextLevel, LoadSceneMode.Single);
    }

    void LevelFailed()
    {
        if (evacShuttle.gameObject.activeInHierarchy == false)
        {
            Debug.Log("Level Failed");

            if (UICanvas != null)
                UICanvas.SetActive(false);

            SceneManager.LoadScene(0, LoadSceneMode.Single);
        }
        
    }

    public void SpawnMilitia()
    {
        if (militiaSpawnerCache.Count > 0)
        {
            for (int i = 0; i < militiaSpawnerCache.Count; i++)
            {
                StartCoroutine(militiaSpawnerCache[i].SpawnLoop());
            }
        }
    }
}