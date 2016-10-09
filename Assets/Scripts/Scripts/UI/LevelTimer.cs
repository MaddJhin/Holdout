using UnityEngine;
using UnityEngine.UI;

public class LevelTimer : MonoBehaviour {

    public LevelManager levelManager;

    private float LevelDuration;
    private Text timer;

    void Awake()
    {
        levelManager = FindObjectOfType<LevelManager>();
        timer = GetComponent<Text>();
    }

	// Use this for initialization
	void Start () {
        LevelDuration = (float)levelManager.levelDuration;
	}
	
	// Update is called once per frame
	void Update () {

        LevelDuration -= Time.deltaTime;
        timer.text = "T I M E   L E F T :   " + Mathf.Round(LevelDuration);
	}
}
