using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.PostProcessing;

public class InGameMenuManager : MonoBehaviour
{
    private GameObject gameManager;

    private PostProcessVolume postProcessVolume;
    private Vignette vignette;

    [SerializeField]
    [Tooltip("Reference to the pause menu")]
    private GameObject pauseMenu;

    [SerializeField]
    [Tooltip("Reference to the resume button")]
    private GameObject resumeButton;

    [SerializeField]
    [Tooltip("Reference to the quit button")]
    private GameObject quitButton;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager");
        postProcessVolume = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PostProcessVolume>();
        postProcessVolume.profile.TryGetSettings(out vignette);

        Debug.Assert(pauseMenu != null, "Missing pause menu reference");
        Debug.Assert(gameManager != null, "Missing Game Manager reference");
        Debug.Assert(resumeButton != null, "Missing resume button reference");
        Debug.Assert(quitButton != null, "Missing quit button reference");

        // Link menu visibility to the game manager
        gameManager.GetComponent<GameManager>().GamePausedEvent += DisplayPauseMenu;
        gameManager.GetComponent<GameManager>().GameResumedEvent += HidePauseMenu;

        // Link buttons to the game manager
        resumeButton.GetComponent<Button>().onClick.AddListener(gameManager.GetComponent<GameManager>().ResumeGame);
        quitButton.GetComponent<Button>().onClick.AddListener(gameManager.GetComponent<GameManager>().QuitGame);

        HidePauseMenu();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void DisplayPauseMenu()
    {
        pauseMenu.SetActive(true);
        vignette.active = true;
    }

    private void HidePauseMenu()
    {
        pauseMenu.SetActive(false);
        vignette.active = false;
    }
}
