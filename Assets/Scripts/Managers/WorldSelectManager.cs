using UnityEngine;
using System.Collections;

public class WorldSelectManager : MonoBehaviour {

	public void LoadLevel (int levelIndex){
		Application.LoadLevel(levelIndex);
	}
}
