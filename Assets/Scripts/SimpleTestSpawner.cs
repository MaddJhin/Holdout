using UnityEngine;
using System.Collections;

public class SimpleTestSpawner : MonoBehaviour {

	public GameObject enemy;                // The enemy prefab to be spawned.
	public float spawnTime = 3f;            // How long between each spawn.
		
	void Start ()
	{
		// Call the Spawn function after a delay of the spawnTime and then continue to call after the same amount of time.
		InvokeRepeating ("Spawn", spawnTime, spawnTime);
	}
	
	
	void Spawn ()
	{	
		// Create an instance of the enemy prefab at the spawn point's position and rotation.
		Instantiate (enemy, transform.position, transform.rotation);
	}
}
