using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;
using AK.Wwise;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private static GameObject singleton;

    private Inputs inputs;

    [SerializeField]
    [Tooltip("The animator to transition to credits")]
    private Animator creditsTransition;

    [SerializeField]
    [Tooltip("Duration of the transitions to credits in seconds")]
    [Range(1, 5)]
    private float transitionDuration = 3;

    [SerializeField]
    [Tooltip("Duration of the fade to restart in seconds")]
    [Range(2, 10)]
    private float restartFadeDuration = 5;

    private Image fade;
    private Image gameOverLogo;
    private Text gameOverMessage;

    private bool isGameOver;
    public bool IsGameOver { get { return isGameOver; } }

    //private Camera mainCamera;
    //private Camera transitionCamera;

    bool firstRun;

    private void Awake()
    {
        if (singleton == null)
        {
            singleton = gameObject;
            DontDestroyOnLoad(gameObject);
        }
        else if (singleton != gameObject)
        {
            Destroy(gameObject);
            return;
        }

        inputs = InputsManager.Instance.Inputs;

        // TO DO : Display a warning message
        inputs.Player.Quit.performed += _ctx => Application.Quit();

        fade = GameObject.FindGameObjectWithTag("Fade").GetComponent<Image>();
        gameOverLogo = GameObject.FindGameObjectWithTag("GameOver").GetComponent<Image>();
        gameOverMessage = GameObject.FindGameObjectWithTag("GameOverMessage").GetComponent<Text>();

        firstRun = true;

        //mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        //transitionCamera = GameObject.FindGameObjectWithTag("TransitionCamera").GetComponent<Camera>();

        if (fade == null)
        {
            throw new System.NullReferenceException("No fade found in the UI");
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Start is called before the first frame update
    void Start()
    {

        if(creditsTransition == null)
        {
            throw new System.NullReferenceException("No animator for the transitions to the credits");
        }

        creditsTransition.SetBool("HasWon", false);
        creditsTransition.SetBool("CreditsLoaded", false);

        /*
        if (stopAllEvent == null)
        {
            throw new System.NullReferenceException("No reference to the wwise stop all sounds event on the game manager");
        }*/
        Analytics.enabled = false;


        //DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /* Defeat functions */

    public void GameOver()
    {
        isGameOver = true;

        //Disable player inputs
        inputs.Player.Disable();

        //Play defeat sound
        AkSoundEngine.PostEvent("Play_Mort", gameObject, (uint)AkCallbackType.AK_EndOfEvent, CallbackFunction, null);

        // Fade out
        fade.GetComponent<Animation>().Play("BlackScreenFadeIn");

        // Display message
        gameOverMessage.GetComponent<Animation>().Play("GameOverMessageFadeInOut");

        // Display logo
        gameOverLogo.GetComponent<Animation>().Play("LogoFadeIn");
    }

    void CallbackFunction(object in_cookie, AkCallbackType in_type, object in_info)
    {
        // Stop all sounds
        AkSoundEngine.StopAll();

        Debug.Log("Loading scene");
        gameOverLogo.GetComponent<Image>().color = new Color(gameOverLogo.GetComponent<Image>().color.r, gameOverLogo.GetComponent<Image>().color.g, gameOverLogo.GetComponent<Image>().color.b, 1);

        // Switch camera
        //transitionCamera.enabled = true;
        //mainCamera.enabled = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        isGameOver = false;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameElise" || scene.name == "Game")
        {
            if (!firstRun)
            {
                Debug.Log("Scene loaded");

                // Fade out
                gameOverLogo.GetComponent<Animation>().Play("LogoFadeOut");
                fade.GetComponent<Animation>().Play("BlackScreenFadeOut");

                inputs.Player.Enable();
            }
        }
        firstRun = false;
    }

    /* Victory functions */

    public void WinGame()
    {
        inputs.Player.Disable();

        // TO DO : Esthesia invincibility (and no damage animation)

        creditsTransition.SetBool("HasWon", true);
    }

    public void DisplayCredits()
    {
        AkSoundEngine.StopAll();
        SceneManager.LoadScene("CreditsScene");

        if (SceneManager.GetActiveScene().name != "CreditsScene")
        {
            Debug.Log("Crédits loading");
            StartCoroutine("WaitForCreditsLoad");
        }
        else
        {
            Debug.Log("Crédits déjà prêts");
        }
    }

    private IEnumerator WaitForCreditsLoad()
    {
        Debug.Log("Dans la coroutine");
        while (SceneManager.GetActiveScene().name != "CreditsScene")
        {
            yield return null;
            Debug.Log("j'attends que les crédits se chargent");
        }

        if (SceneManager.GetActiveScene().name == "CreditsScene")
        {
            creditsTransition.SetBool("CreditsLoaded", true);
            Debug.Log("c'est parti pour le fade in du crédits");
        }
    }
}
