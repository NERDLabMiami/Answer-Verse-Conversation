using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Autosave : MonoBehaviour
{
    public float timeAllowedForSaving = 5f;
    private string sceneToLoad;
    private float timeToLoadNextScene;
    // Start is called before the first frame update
    void Start()
    {
        timeToLoadNextScene = Time.time + timeAllowedForSaving;
        if(PlayerPrefs.HasKey("Scene After Save"))
        {
            sceneToLoad = PlayerPrefs.GetString("Scene After Save", sceneToLoad);
        }
        else
        {
            sceneToLoad = PlayerPrefs.GetString("Next Scene", sceneToLoad);

        }

    }

    // Update is called once per frame
    void Update()
    {
        if (timeToLoadNextScene <= Time.time)
        {
            SceneManager.LoadScene(sceneToLoad);
        }

    }
}
