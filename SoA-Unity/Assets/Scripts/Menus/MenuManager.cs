using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AK.Wwise;
using UnityEngine.SceneManagement;


public enum MENU_STATE { NONE, CREDITS, CONTROLS }

public class MenuManager : MonoBehaviour
{
    private GameObject creditsPanel;
    private GameObject controlImage;
    private GameObject corePanel;
    //private GameObject extendedPanel;

    private GameObject fade;
    public GameObject Fade { get { return fade; } }

    private MENU_STATE menuState;
    public MENU_STATE MenuState { get { return menuState; } }

    // Start is called before the first frame update
    void Start()
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

        AkSoundEngine.PostEvent("Play_Music_Main_Title", gameObject);
        //AkSoundEngine.PostEvent("Play_Music_Menu", gameObject);

        controlImage = GameObject.FindGameObjectWithTag("Controls");
        if (controlImage == null)
        {
            throw new System.NullReferenceException("Missing control image in the menu");
        }
        controlImage.GetComponent<CanvasGroup>().alpha = 0;

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
        
            controlImage.GetComponent<Animation>().Play("CreditsFadeIn");
            yield return new WaitForSeconds(0.01f);
        
    }

    IEnumerator FadeOutControls()
    {
            controlImage.GetComponent<Animation>().Play("CreditsFadeOut");
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
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        SceneManager.LoadScene("GameElise");
        AkSoundEngine.PostEvent("Stop_Music_Main_Title", gameObject);
        //AkSoundEngine.PostEvent("Stop_Music_Menu", gameObject);
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
} //FINISH
