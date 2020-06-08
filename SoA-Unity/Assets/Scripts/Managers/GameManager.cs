using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;
using AK.Wwise;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The Wwise event to reset all sounds")]
    private AK.Wwise.Event stopAllEvent;

    // Start is called before the first frame update
    void Start()
    {
        if(stopAllEvent == null)
        {
            throw new System.NullReferenceException("No reference to the wwise stop all sounds event on the game manager");
        }
        Analytics.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RestartGame()
    {
        // Stop all sounds
        AkSoundEngine.StopAll();

        // Reload the scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
