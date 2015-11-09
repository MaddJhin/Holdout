using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GenericPooler : MonoBehaviour 
{
    public static GenericPooler current;
    public int pooledAmount = 40;
    public Dictionary<string, List<GameObject>> poolDict;
    public List<GameObject> pooledObjects; // List of objects that will be pooled

    // List to hold pool of an object during instantiation
    List<GameObject> objPool; 

    void Awake()
    {
        current = this;
        poolDict = new Dictionary<string, List<GameObject>>();
        objPool = new List<GameObject>();

        foreach (var poolObj in pooledObjects)
        {
            // Clear the temporary list
            List<GameObject> tempPool = new List<GameObject>();

            for (int i = 0; i < pooledAmount; i++)
            {
                GameObject obj = (GameObject)Instantiate(poolObj);
                obj.SetActive(false);
                tempPool.Add(obj);
            }

            poolDict.Add(poolObj.name, tempPool);
            Debug.Log(poolObj.name);
        }  
    }


    /* Function: Fetches an object from the pool
     * Parameters: string
     * Returns: GameObject
     */
    public GameObject GetPooledObject(string objType)
    {
        Debug.Log("Pool arg: " + objType);

        List<GameObject> iterList;                                      // Temp list to grab all objects of requested type

        if (poolDict.ContainsKey(objType))                              // Check if the requested object is in the pool 
        {
            Debug.Log("Key " + objType + " was found");
            Debug.Log("Fetching value " + poolDict[objType]);

            iterList = poolDict[objType];                               // Grab all objects of requested type

            for (int i = 0; i < iterList.Count; i++)                    // Iterate through requested types
            {

                if (!iterList[i].activeInHierarchy)                     // If an inactive one is found return it
                    return iterList[i];
            }
        }

        else
            Debug.Log("Key not found");

        return null;
    }
}
