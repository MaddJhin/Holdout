using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public enum WaveTypes
{
    Single,
    RepeatingWave,
    InfiniteRepeatingWave
}

/* Function: Struct to define a set of objects to spawn, as well as their count
 */


[System.Serializable]
public class SpawnerWave
{
    [System.Serializable]
    public struct UnitSet
    {
        public GameObject objectToSpawn;
        public int objectCount;
        public float spawnDelay;
    }

    [Tooltip("Dictates whether the wave will repeat, and how many times")]
    public WaveTypes waveType;
    public int repeatAmount = 1;
    public float waveDelay = 1f;

    public string name = "wave";                                         // Name of the wave
    public List<UnitSet> spawnList;                            // List of unit sets to spawn

    
}

public class NewSpawnerRefactored : MonoBehaviour
{   
    [Tooltip("Dictates whether the spawner will repeat it's waves, and how many times")]
    public WaveTypes spawnerType;

    [Tooltip("How much a spawner will repeat it's waves")]
    public int repeatAmount;

    [Tooltip("Randomises the order of waves")]
    public bool randomiseWaves;

    public List<SpawnerWave> waves;

    private GameManager gm;

    public void AddWave()
    {
        waves.Add(new SpawnerWave());
    }

    public void RemoveWave(SpawnerWave wave)
    {
        waves.Remove(wave);
    }

    void Awake()
    {
        gm = GameObject.FindObjectOfType<GameManager>();
    }

    // Use this for initialization
    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        switch (spawnerType)
        {
            case WaveTypes.Single:

                yield return StartCoroutine(SpawnWave(waves));
                break;

            case WaveTypes.RepeatingWave:

                for (int i = 0; i < repeatAmount; i++)
                {
                    yield return StartCoroutine(SpawnWave(waves));
                }
                break;

            case WaveTypes.InfiniteRepeatingWave:

                while (true)
                {
                    yield return StartCoroutine(SpawnWave(waves));
                }
                break;

            default:
                break;
        }

        gameObject.SetActive(false);
        gm.IsSpawnInactive();
        yield return null;
    }

    IEnumerator SpawnWave(List<SpawnerWave> wavesToSpawn)
    {
        if (randomiseWaves)
        {
            List<int> spawnedWaves = new List<int>();

            for (int i = 0; i < wavesToSpawn.Count; i++)
            {

                int currWaveIndex = selectRandWave(spawnedWaves, wavesToSpawn);
                spawnedWaves.Add(currWaveIndex);
                SpawnerWave currWave = wavesToSpawn[currWaveIndex];

                if (currWave.waveDelay > 0)
                    yield return new WaitForSeconds(currWave.waveDelay);

                switch (currWave.waveType)
                {
                    case WaveTypes.Single:
                        yield return StartCoroutine(SpawnUnits(currWave));
                        break;

                    case WaveTypes.RepeatingWave:

                        for (int j = 0; i < currWave.repeatAmount; i++)
                        {
                            yield return StartCoroutine(SpawnUnits(currWave));
                        }

                        break;

                    case WaveTypes.InfiniteRepeatingWave:

                        while (true)
                        {
                            yield return StartCoroutine(SpawnUnits(currWave));
                        }

                        break;

                    default:
                        break;
                }
            }
        }

        foreach (var currWave in wavesToSpawn)
        {
            if (currWave.waveDelay > 0)
                yield return new WaitForSeconds(currWave.waveDelay);

            switch (currWave.waveType)
            {
                case WaveTypes.Single:
                    yield return StartCoroutine(SpawnUnits(currWave));
                    break;

                case WaveTypes.RepeatingWave:

                    for (int i = 0; i < currWave.repeatAmount; i++)
                    {
                        yield return StartCoroutine(SpawnUnits(currWave));
                    }

                    break;

                case WaveTypes.InfiniteRepeatingWave:

                    while (true)
                    {
                        yield return StartCoroutine(SpawnUnits(currWave));
                    }

                    break;

                default:
                    break;
            }
        }

        yield return null;
    }

    IEnumerator SpawnUnits(SpawnerWave waveToSpawn)
    {
        foreach (var spawnSet in waveToSpawn.spawnList)
        {
            if (spawnSet.spawnDelay > 0)
                yield return new WaitForSeconds(spawnSet.spawnDelay);

            for (int i = 0; i < spawnSet.objectCount; i++)
            {
                GameObject obj = GenericPooler.current.GetPooledObject(spawnSet.objectToSpawn.name);

                if (obj == null)
                    Debug.Log("Object not found in pool");

                else
                {
                    obj.transform.position = transform.position;
                    obj.transform.rotation = transform.rotation;
                    obj.SetActive(true);
                }
            }
        }

        yield return null;
    }

    int selectRandWave(List<int> usedIndex, List<SpawnerWave> waves)
    {
        int waveIndex = Random.Range(0, (waves.Count - 1));

        if (usedIndex.Contains(waveIndex))
        {
            return selectRandWave(usedIndex, waves);
        }

        return waveIndex;
    }
}
