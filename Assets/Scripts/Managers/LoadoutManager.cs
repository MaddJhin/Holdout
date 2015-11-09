using UnityEngine;
using System.Collections;

/* =============
 * USAGE
 * =============
 * Contains references to each of the player unit prefabs
 * Instantiates the prefab in each of the loadout slots
 * Persistent between level sets
 * Only called on selection of a new level set
 */

public class LoadoutManager : MonoBehaviour 
{
    public GameObject[] squad;

    #region Singleton
    private static LoadoutManager _instance;

    public static LoadoutManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<LoadoutManager>();

                DontDestroyOnLoad(_instance.gameObject);
            }

            return _instance;
        }
    }

    void Awake()
    {
        if (_instance == null)
        {

            Debug.Log("Creating singleton");
            // If this instance is the first in the scene, it becomes the singleton
            _instance = this;
            squad = new GameObject[7];
            DontDestroyOnLoad(this);
        }

        else
        {
            // If another Singleton already exists, destroy this object
            if (this != _instance)
            {
                Debug.Log("Destroying invalid singleton");
                Destroy(this.gameObject);
            }
        }
    }
    #endregion

    // Accepts a type of squad member to add, and their slot in the squad loadout (1 - 7)
    public void AddSquadmate(string memberType, int slot)
    {
        if (slot <= squad.Length)
        {

        }

        else
            Debug.Log("Slot " + "out of range");
    }
}
