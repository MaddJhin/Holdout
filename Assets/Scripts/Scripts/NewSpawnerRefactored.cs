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
        public string defaultTarget;
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

    [Tooltip("Dictates time between spawner activations")]
    public float spawnerCooldown;

    public List<SpawnerWave> waves;

    private bool canSpawn = true;  // Determines whether or not a wave can be spawned

    public void AddWave()
    {
        waves.Add(new SpawnerWave());
    }

    public void RemoveWave(SpawnerWave wave)
    {
        waves.Remove(wave);
    }

    public IEnumerator SpawnLoop()
    {
        canSpawn = false;
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

        yield return new WaitForSeconds(spawnerCooldown);
        canSpawn = true;
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
                    EnemyUnitControl control;
                    if (control = obj.GetComponent<EnemyUnitControl>())
                    {
                        control.targetLocation = GameObject.Find(spawnSet.defaultTarget);
                    }

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

    public void BeginSpawnLoop()
    {
        if (canSpawn)
            StartCoroutine(SpawnLoop());
    }
}
