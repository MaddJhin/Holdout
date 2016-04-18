using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Analytics;

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
    public RefactoredBarricade[] barricades;
    public GameObject UICanvas;

    [Header("Miscellaneous Attributes")]
    public int spawnOffset = 1;
    public int levelPerformance;

    private GameObject UI_playerPanel;
    private InputManager IM;
    private List<NewSpawnerRefactored> enemySpawnerCache = new List<NewSpawnerRefactored>();
    private GameObject[] playerLoadoutCache;

    void Awake()
    {
        UICanvas.SetActive(true);
        UI_playerPanel = GameObject.Find("PlayerPortraitsMenu");
        IM = GameObject.FindObjectOfType<InputManager>();
        RetreatPointSetup();

        if (enemySpawners.Length > 0)
        {
            for (int i = 0; i < enemySpawners.Length; i++)
            {
                enemySpawnerCache.Add(enemySpawners[i].GetComponent<NewSpawnerRefactored>());
            }
        }
    }

    void Start()
    {
        UICanvas.SetActive(true);
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
            playerLoadoutCache = GameManager.playerLoadout;
        }
        
        Debug.Log("Logging Loadout Analytics");
        if (GameManager.playerLoadout[0] != null)
        {
            Analytics.CustomEvent("LevelLoadout", new Dictionary<string, object>
            {
                {"Slot One", GameManager.playerLoadout[0].name},
                {"Slot Two", GameManager.playerLoadout[1].name},
                {"Slot Three", GameManager.playerLoadout[2].name},
                {"Slot Four", GameManager.playerLoadout[3].name},
                {"Slot Five", GameManager.playerLoadout[4].name},
                {"Slot Six", GameManager.playerLoadout[5].name},
                {"Slot Seven", GameManager.playerLoadout[6].name},
            });
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

            else
            {
                Debug.Log("Logging Survivor Analytics");
                Analytics.CustomEvent("UnitSurived", new Dictionary<string, object>
                  {
                    {"Scene ID", SceneManager.GetActiveScene().buildIndex},
                    {"Survivor", GameManager.playerLoadout[i].name}
                  });
            }
        }

        Debug.Log("Logging Completion Analytics");
        Analytics.CustomEvent("MissionComplete", new Dictionary<string, object>
          {
            {"Scene ID", SceneManager.GetActiveScene().buildIndex},
            {"Mission Complete", true},
            {"Remaining Barricades", barricades.Length},
            {"Mission Result", levelPerformance}
          });

        SceneManager.LoadScene(nextLevel, LoadSceneMode.Single);
    }

    void LevelFailed()
    {
        if (evacShuttle.gameObject.activeInHierarchy == false)
        {
            Debug.Log("Level Failed");

            if (UICanvas != null)
                UICanvas.SetActive(false);

            Debug.Log("Logging Failure Analytics");
            Analytics.CustomEvent("MissionFailed", new Dictionary<string, object>
            {
                {"Scene ID", SceneManager.GetActiveScene().buildIndex},
                {"Mission Complete", false},
                {"Remaining Barricades", barricades.Length},
                {"Mission Result", levelPerformance}
            });

            SceneManager.LoadScene(0, LoadSceneMode.Single);
        }
        
    }

    int CalculateLevelPerformance()
    {
        int scoreCounter = 0;

        for (int i = 0; i < barricades.Length; i++)
        {
            if (barricades[i].gameObject.activeInHierarchy)
                scoreCounter++;
        }

        if (scoreCounter >= barricades.Length)
            return 3;

        else if (scoreCounter <= 1)
            return 1;

        else
            return 2;
    }

    void RetreatPointSetup()
    {
        for (int i = 0; i < barricades.Length; i++)
        {
            barricades[i].GetRetreatPoints(barricades[i].retreatBarricade);
        }
    }
}