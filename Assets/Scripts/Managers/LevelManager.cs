using UnityEngine;
using System.Collections;

public class LevelManager : MonoBehaviour 
{
    // Number of waves in the current level
    public int totalWaves;
    public int currentWave;
    public string nextLevel;

    private GameManager gameManager;
    private Spawner[] activeSpawners;

    void Awake()
    {
        // Get reference to the active GameManager
        gameManager = FindObjectOfType<GameManager>();
        activeSpawners = FindObjectsOfType<Spawner>();
    }

    void Update()
    {
        // If level is complete, use GameManager to bring up relevant menu
        for (int i = 0; i < activeSpawners.Length; i++)
        {
            if (activeSpawners[i].waveCounter >= activeSpawners[i].waves.Count)
            {
                Debug.Log("Level Complete");
            }
        }
    }

    void IsWaveComplete()
    {
        // Check if current wave is complete
        // If it is, move to next wave and begin it
    }

    bool IsLevelComplete()
    {
        if (currentWave > totalWaves)
        {
            Debug.Log("Level Complete");
            return true;
        }

        else
            return false;
    }
}
