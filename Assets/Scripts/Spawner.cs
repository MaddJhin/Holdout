using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class WaveAction
{
    public string name;
    public float delay;
    public string prefab;
    public int spawnCount;
    public string message;
}

[System.Serializable]
public class Wave
{
    public string name;
    public List<WaveAction> actions;
}

public class Spawner : MonoBehaviour
{
    public float difficultyFactor = 0.9f;
    public List<Wave> waves;
    private Wave m_CurrentWave;
    public Wave CurrentWave { get { return m_CurrentWave; } }
    private float m_DelayFactor = 1.0f;
    public int waveCounter;
    public string targetObject;

    private UnitSight tempSight;        // Temporary UnitSight reference for setting default targets
    private GameManager gm;

    IEnumerator SpawnLoop()
    {
        m_DelayFactor = 1.0f;
        
        Debug.Log("Checking Waves");
        foreach (Wave W in waves)
        {
            m_CurrentWave = W;
            foreach (WaveAction A in W.actions)
            {
                if (A.delay > 0)
                    yield return new WaitForSeconds(A.delay * m_DelayFactor);
                if (A.message != "")
                {
                    Debug.Log(A.message);
                }
                if (A.prefab != null && A.spawnCount > 0)
                {
                    for (int i = 0; i < A.spawnCount; i++)
                    {
                        Debug.Log("Spawning " + A.prefab);
                        GameObject obj = GenericPooler.current.GetPooledObject(A.prefab);

                        if (obj.tag != "Player")
                        {
                            gm.AddObjective();
                        }

                        if (targetObject != null)
                        {
                            tempSight = obj.GetComponent<UnitSight>();
                            tempSight.defaultTarget = targetObject;
                        }

                        if (obj != null)
                        {
                            obj.transform.position = transform.position;
                            obj.transform.rotation = transform.rotation;
                            obj.SetActive(true);
                        }
                    }
                }
                    
            }

            waveCounter++;
            Debug.Log("Wave Counter" + waveCounter);
            yield return null;  // prevents crash if all delays are 0
        }

        Debug.Log("Spawn Loop Complete");
        gameObject.SetActive(false);
        gm.IsSpawnInactive();

        //m_DelayFactor *= difficultyFactor;
        yield return null;  // prevents crash if all delays are 0        
    }

    void Start()
    {
        Debug.Log("Starting spawn loop");
        StartCoroutine(SpawnLoop());
        gm = GameObject.FindObjectOfType<GameManager>();
    }

}