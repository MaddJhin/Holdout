using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
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
    public GameObject[] militiaSpawners;
    public RefactoredBarricade[] barricades;

    [Header("UI References")]
    public GameObject UICanvas;

    [Header("Miscellaneous Attributes")]
    public int spawnOffset = 1;
    public float spawnMilitiaCooldown;
    public int levelPerformance;

    private GameObject UI_playerPanel;
    public  Image[] playerButton;
    private GameObject playerLoadoutPanel;
    private Sprite[] playerUnitIcons;
    private Transform temp;
    private bool isPaused;

    private InputManager IM;
    private List<NewSpawnerRefactored> enemySpawnerCache = new List<NewSpawnerRefactored>();
    private List<NewSpawnerRefactored> militiaSpawnerCache = new List<NewSpawnerRefactored>();
    private GameObject[] playerLoadoutCache;

    void Awake()
    {
        UICanvas.SetActive(true);     
        IM = GameObject.FindObjectOfType<InputManager>();
        RetreatPointSetup();
        playerUnitIcons = Resources.LoadAll<Sprite>("Button Icons");

        if (enemySpawners.Length > 0)
        {
            for (int i = 0; i < enemySpawners.Length; i++)
            {
                enemySpawnerCache.Add(enemySpawners[i].GetComponent<NewSpawnerRefactored>());
            }
        }

        // Get reference to the player portrait buttons
        UI_playerPanel = GameObject.Find("PlayerPortraitsMenu");
        playerButton = new Image[7];

        // Find the panel holding the player portrait buttons
        foreach (Transform t in UI_playerPanel.transform)
        {
            if (t.name == "Panel")
            {
                playerLoadoutPanel = t.gameObject;
            }
        }

        // Grab each portrait button from the panel
        for (int i = 0; i < playerLoadoutPanel.transform.childCount; i++)
        {
            temp = playerLoadoutPanel.transform.GetChild(i).GetChild(0).GetChild(0);
            playerButton[i] = temp.GetComponent<Image>();
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
                playerButton[i].gameObject.SetActive(false);
                continue;
            }

            playerButton[i].gameObject.SetActive(true);
            int unitIndex = GameManager.loadoutIndex[i];
            string unitToSpawn = GameManager.unitToSpawn[unitIndex];

            // Assign Unit Icons to Buttons
            switch (GameManager.unitToSpawn[unitIndex])
            {
                case "Marksman":
                    playerButton[i].sprite = playerUnitIcons[0];
                    break;

                case "Medic":
                    playerButton[i].sprite = playerUnitIcons[1];
                    break;

                case "Mechanic":
                    playerButton[i].sprite = playerUnitIcons[2];
                    break;

                case "Heavy Trooper":
                    playerButton[i].sprite = playerUnitIcons[3];
                    break;

                default:
                    playerButton[i].sprite = null;
                    break;
            }
            
            GameManager.playerLoadout[i] = Instantiate(Resources.Load(unitToSpawn)) as GameObject;
            GameManager.playerLoadout[i].SetActive(false);
            playerLoadoutCache = GameManager.playerLoadout;
        }
        
        Debug.Log("Logging Loadout Analytics");
        
        if (GameManager.playerLoadout != null)
        {
            Analytics.CustomEvent("LevelLoadout", new Dictionary<string, object>
            {
                {"Slot One", GameManager.playerLoadout[0]},
                {"Slot Two", GameManager.playerLoadout[1]},
                {"Slot Three", GameManager.playerLoadout[2]},
                {"Slot Four", GameManager.playerLoadout[3]},
                {"Slot Five", GameManager.playerLoadout[4]},
                {"Slot Six", GameManager.playerLoadout[5]},
                {"Slot Seven", GameManager.playerLoadout[6]},
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
        // Stop Spawning
        // Pop up completed message
        // Update completed level in save file
        // Deactivate all enemies
        // Promp player to continue?

        // Unlock next level in the build order
        GameManager.unlockedLevels[SceneManager.GetActiveScene().buildIndex + 1] = true;

        if (UICanvas != null && nextLevel < 2)
            UICanvas.SetActive(false);

        for (int i = 0; i < GameManager.playerLoadout.Length; i++)
        {
            if (GameManager.playerLoadout[i] != null && GameManager.playerLoadout[i].activeInHierarchy == false)
            {
                GameManager.loadoutIndex[i] = -1;
                Destroy(GameManager.playerLoadout[i]);
            }

            else if (GameManager.playerLoadout[i] != null)
            {
                Analytics.CustomEvent("UnitSurived", new Dictionary<string, object>
                  {
                    {"Scene ID", SceneManager.GetActiveScene().buildIndex},
                    {"Survivor", GameManager.playerLoadout[i].name}
                  });
            }
        }
        
        Analytics.CustomEvent("MissionComplete", new Dictionary<string, object>
          {
            {"Scene ID", SceneManager.GetActiveScene().buildIndex},
            {"Mission Complete", true},
            {"Remaining Barricades", barricades.Length},
            {"Mission Result", levelPerformance}
          });

        if (!Application.isEditor)
            GameManager.Save();

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

    #region Menu Methods

    public void PauseGame()
    {
        if (!isPaused)
        {
            isPaused = true;
            StartCoroutine(PauseDelay());
        }
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
    }

    public void QuitToMenu()
    {
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }

    IEnumerator PauseDelay()
    {
        yield return new WaitForSeconds(0.6f);
        Time.timeScale = 0f;
    }

    #endregion
}