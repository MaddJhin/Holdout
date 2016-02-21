using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    // Number of waves in the current level
    [Header("Level Progress Attributes")]
    public int startCountdown;
    public int levelDuration;
    public int nextLevel;
    
    [Header("Level Object References")]
    public RefactoredBarricade evacShuttle;
    public GameObject[] enemySpawners;
    public GameObject[] militiaSpawners;
    public GameObject[] barricades;
    public GameObject UICanvas;

    [Header("Miscellaneous Attributes")]
    public int spawnOffset = 1;
    public float spawnMilitiaCooldown;
    public int levelPerformance;

    private GameObject UI_playerPanel;
    private InputManager IM;
    private List<NewSpawnerRefactored> enemySpawnerCache = new List<NewSpawnerRefactored>();
    private List<NewSpawnerRefactored> militiaSpawnerCache = new List<NewSpawnerRefactored>();

    void Awake()
    {
        UICanvas.SetActive(true);
        Debug.Log(UICanvas.activeInHierarchy);
        UI_playerPanel = GameObject.Find("PlayerPortraitsMenu");
        IM = GameObject.FindObjectOfType<InputManager>();

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
        UICanvas.SetActive(true);
        Debug.Log(UICanvas.activeInHierarchy);
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
            if (GameManager.loadoutIndex[i] == -1)
            {
                GameManager.playerLoadout[i] = null;
                continue;
            }

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

        levelPerformance = CalculateLevelPerformance();
        Debug.Log(levelPerformance);

        if (UICanvas != null && nextLevel < 2)
            UICanvas.SetActive(false);

        for (int i = 0; i < GameManager.playerLoadout.Length; i++)
        {
            if (GameManager.playerLoadout[i].activeInHierarchy == false)
            {
                GameManager.loadoutIndex[i] = -1;
                Destroy(GameManager.playerLoadout[i]);
            }
        }

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

    int CalculateLevelPerformance()
    {
        int scoreCounter = 0;

        for (int i = 0; i < barricades.Length; i++)
        {
            if (barricades[i].activeInHierarchy)
                scoreCounter++;
        }

        if (scoreCounter >= barricades.Length)
            return 3;

        else if (scoreCounter <= 1)
            return 1;

        else
            return 2;
    }
}