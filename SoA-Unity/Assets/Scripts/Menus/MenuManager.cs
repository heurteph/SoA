using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AK.Wwise;
using UnityEngine.SceneManagement;


public enum MENU_STATE { NONE, CREDITS, CONTROLS }

public enum CONTROL_STATE { NONE, MOUSEKEYBOARD, GAMEPAD }

public class MenuManager : MonoBehaviour
{
    private GameObject creditsPanel;
    private GameObject controlsPanel;
    private GameObject gamepadPanel;
    private GameObject mouseKeyboardPanel;
    private GameObject corePanel;
    //private GameObject extendedPanel;

    private GameObject fade;
    public GameObject Fade { get { return fade; } }

    private MENU_STATE menuState;
    public MENU_STATE MenuState { get { return menuState; } }

    private CONTROL_STATE controlState;
    public CONTROL_STATE ControlState { get { return controlState; } set { controlState = value; } }

    // Start is called before the first frame update
    void Awake()
    {
        menuState = MENU_STATE.NONE;

        creditsPanel = GameObject.FindGameObjectWithTag("MenuCredits");
        if (creditsPanel == null)
        {
            throw new System.NullReferenceException("Missing credits panel in the menu");
        }
        creditsPanel.GetComponent<CanvasGroup>().alpha = 0;

        fade = GameObject.FindGameObjectWithTag("Fade");
        fade.GetComponent<Image>().color = new Color(fade.GetComponent<Image>().color.r, fade.GetComponent<Image>().color.g, fade.GetComponent<Image>().color.b, 1);
        fade.GetComponent<Animation>().Play("TitleFadeIn");

        corePanel = creditsPanel.transform.GetChild(0).gameObject;
        //extendedPanel = creditsPanel.transform.GetChild(1).gameObject;

        foreach (Transform child in corePanel.transform)
        {
            child.GetComponent<Text>().color = new Color(1, 1, 1, 0);
        }
        /*
        foreach (Transform child in extendedPanel.transform)
        {
            child.GetComponent<Text>().color = new Color(1, 1, 1, 0);
        }*/

        //AkSoundEngine.PostEvent("Play_Music_Menu", gameObject);

        controlsPanel = GameObject.FindGameObjectWithTag("Controls");
        if (controlsPanel == null)
        {
            throw new System.NullReferenceException("Missing control panel in the menu");
        }
        controlsPanel.GetComponent<CanvasGroup>().alpha = 0;

        mouseKeyboardPanel = GameObject.FindGameObjectWithTag("MouseKeyboard");
        if (mouseKeyboardPanel == null)
        {
            throw new System.NullReferenceException("Missing mouse keyboard panel in the menu");
        }

        gamepadPanel = GameObject.FindGameObjectWithTag("Gamepad");
        if (gamepadPanel == null)
        {
            throw new System.NullReferenceException("Missing gamepad panel in the menu");
        }

        if (PlayerPrefs.HasKey("controls") && PlayerPrefs.GetString("controls").Equals("gamepad"))
        {
            controlState = CONTROL_STATE.GAMEPAD;
            mouseKeyboardPanel.GetComponent<CanvasGroup>().alpha = 0;
            gamepadPanel.GetComponent<CanvasGroup>().alpha = 1;
        }
        else
        {
            controlState = CONTROL_STATE.MOUSEKEYBOARD;
            mouseKeyboardPanel.GetComponent<CanvasGroup>().alpha = 1;
            gamepadPanel.GetComponent<CanvasGroup>().alpha = 0;
        }
    }

    private void Start()
    {
        AkSoundEngine.PostEvent("Play_Music_Main_Title", gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
    public void DisplayControls()
    {
        menuState = MENU_STATE.CONTROLS;
        StartCoroutine("FadeInControls");
    }
    
    public void HideControls()
    {
        menuState = MENU_STATE.NONE;
        StartCoroutine("FadeOutControls");
    }

    IEnumerator FadeInControls()
    {
        controlsPanel.GetComponent<Animation>().Play("CreditsFadeIn");
        yield return new WaitForSeconds(0.01f);
    }

    IEnumerator FadeOutControls()
    {
        controlsPanel.GetComponent<Animation>().Play("CreditsFadeOut");
        yield return new WaitForSeconds(0.01f);
    }

    public void SwitchToMouseKeyboardControls()
    {
        controlState = CONTROL_STATE.MOUSEKEYBOARD;
        StartCoroutine("CrossFadeMouseKeyboardControls");
    }

    public void SwitchToGamepadControls()
    {
        controlState = CONTROL_STATE.GAMEPAD;
        StartCoroutine("CrossFadeGamepadControls");
    }

    IEnumerator CrossFadeMouseKeyboardControls()
    {
        gamepadPanel.GetComponent<Animation>().Play("CreditsFadeOut");
        mouseKeyboardPanel.GetComponent<Animation>().Play("CreditsFadeIn");
        yield return new WaitForSeconds(0.01f);
    }

    IEnumerator CrossFadeGamepadControls()
    {
        mouseKeyboardPanel.GetComponent<Animation>().Play("CreditsFadeOut");
        gamepadPanel.GetComponent<Animation>().Play("CreditsFadeIn");
        yield return new WaitForSeconds(0.01f);
    }

    public void DisplayCredits()
    {
        menuState = MENU_STATE.CREDITS;
        StartCoroutine("FadeInCredits");
    }

    public void HideCredits()
    {
        menuState = MENU_STATE.NONE;
        StartCoroutine("FadeOutCredits");
    }

    public void StartGame()
    {
        // Let's get to work !
        // TO DO : Make sure to add the intro when we have one
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        AkSoundEngine.PostEvent("Stop_Music_Main_Title", gameObject);
    }

    IEnumerator FadeInCredits()
    {
        foreach (Transform child in corePanel.transform)
        {
            child.GetComponent<Animation>().Play("CreditsItemFadeIn");
            yield return new WaitForSeconds(0.01f);
        }
        /*
        foreach (Transform child in extendedPanel.transform)
        {
            child.GetComponent<Animation>().Play("CreditsItemFadeIn");
            yield return new WaitForSeconds(0.01f);
        }*/
    }

    IEnumerator FadeOutCredits()
    {
        foreach (Transform child in corePanel.transform)
        {
            child.GetComponent<Animation>().Play("CreditsItemFadeOut");
            yield return new WaitForSeconds(0.01f);
        }
        /*
        foreach (Transform child in extendedPanel.transform)
        {
            child.GetComponent<Animation>().Play("CreditsItemFadeOut");
            yield return new WaitForSeconds(0.01f);
        }*/
    }

    /* Wwise functions */

    public void PlayClickSound()
    {
        AkSoundEngine.PostEvent("Play_Clic_Selection", gameObject);
    }

    public void PlayHoverSound()
    {
        AkSoundEngine.PostEvent("Play_Survol", gameObject);
    }

    public void PlayParticlesSound()
    {
        AkSoundEngine.PostEvent("Play_Texte_Anim_Particule", gameObject);
    }

    public void StopParticlesSound()
    {
        AkSoundEngine.PostEvent("Stop_Texte_Anim_Particule", gameObject);
    }

} //FINISH
