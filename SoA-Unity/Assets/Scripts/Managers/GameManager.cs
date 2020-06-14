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

        fade = GameObject.FindGameObjectWithTag("Fade").GetComponent<Image>();

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

        StartCoroutine("TerminateGame");
    }

    private IEnumerator TerminateGame()
    {
        //Disable player inputs
        inputs.Player.Disable();

        //Play defeat sound
        AkSoundEngine.PostEvent("Play_Mort", gameObject);

        // Fade out

        while (!Mathf.Approximately(fade.color.a, 1))
        {
            fade.color = new Color(fade.color.r, fade.color.g, fade.color.b, Mathf.Min(fade.color.a + Time.deltaTime / restartFadeDuration, 1));
            yield return null;
        }

        // Stop all sounds
        AkSoundEngine.StopAll();

        // Reload the scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        // Display logo while waiting for scene to reload
        //yield return new WaitForSeconds(5.043f - restartFadeDuration); // duration of the game over jingle : 5,043 s
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameElise" || scene.name == "Game")
        {
            StartCoroutine("RestartGame");
        }
    }

    private IEnumerator RestartGame()
    {
        inputs.Player.Enable();

        // Fade in
        fade.color = new Color(0, 0, 0, 1);
        while (fade.color.a > 0)
        {
            Debug.Log("DeltaTime : " + Time.deltaTime);
            fade.color = new Color(fade.color.r, fade.color.g, fade.color.b, Mathf.Max(fade.color.a - Time.deltaTime / restartFadeDuration, 0));
            yield return null;
        }
        fade.color = new Color(fade.color.r, fade.color.g, fade.color.b, 0);
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
