using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;
using AK.Wwise;

public class GameManager : MonoBehaviour
{
    private Inputs inputs;

    [SerializeField]
    [Tooltip("The animator to transition to credits")]
    private Animator creditsTransition;

    [SerializeField]
    [Tooltip("Duration of the transitions in seconds")]
    [Range(1, 5)]
    private float transitionDuration = 3;

    private 

    /*
    [SerializeField]
    [Tooltip("The Wwise event to reset all sounds")]
    private AK.Wwise.Event stopAllEvent;
    */

    // Start is called before the first frame update
    void Start()
    {
        inputs = InputsManager.Instance.Inputs;

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

        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /* Defeat functions */

    public void RestartGame()
    {
        // Stop all sounds
        AkSoundEngine.StopAll();

        // Reload the scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
