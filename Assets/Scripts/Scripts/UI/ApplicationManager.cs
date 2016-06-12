using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ApplicationManager : MonoBehaviour {
	
    // Load Mission Select Screen
    public void PlayGame()
    {
        SceneManager.LoadScene(1, LoadSceneMode.Single);
    }

	public void Quit () 
	{
		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
		#else
		Application.Quit();
		#endif
	}
}
