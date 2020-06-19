using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class StartButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    GameObject menuManager; 
    
    private ParticleSystem sunSpots;
    ParticleSystem.EmissionModule emission;
    ParticleSystem.VelocityOverLifetimeModule velocity;
    float rateOverTime = 5;

    // Start is called before the first frame update
    void Start()
    {
        menuManager = GameObject.FindGameObjectWithTag("MenuManager");
        if (menuManager == null)
        {
            throw new System.NullReferenceException("Missing MenuManager object");
        }

        sunSpots = transform.GetChild(1).GetComponent<ParticleSystem>();
        emission = sunSpots.emission;
        emission.enabled = true;
        emission.rateOverTime = 0;
        velocity = sunSpots.velocityOverLifetime;
        velocity.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        //transform.GetChild(0).GetComponent<Text>().color = hoveringColor;
        //transform.GetChild(0).GetComponent<Animation>().Play("MenuItemPop");
        transform.GetChild(0).GetComponent<Animation>().Play("MenuItemColorIn");
        emission.rateOverTime = rateOverTime;
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        //transform.GetChild(0).GetComponent<Text>().color = defaultColor;
        transform.GetChild(0).GetComponent<Animation>().Play("MenuItemColorOut");
        emission.rateOverTime = 0;
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        /*
        if (creditsPanel.GetComponent<CanvasGroup>().alpha != 0)
        {
            creditsPanel.GetComponent<Animation>().Play("CreditsFadeOut");
        }*/
        transform.GetChild(0).GetComponent<Animation>().Play("MenuItemFlash");

        StartCoroutine("BurstSpots");

        if (menuManager.GetComponent<MenuManager>().MenuState == MENU_STATE.CREDITS)
        {
            menuManager.GetComponent<MenuManager>().HideCredits();
            transform.parent.GetChild(2).GetChild(0).GetComponent<Animation>().Play("MenuItemUngreyed");
        }
        else if (menuManager.GetComponent<MenuManager>().MenuState == MENU_STATE.CONTROLS)
        {
            menuManager.GetComponent<MenuManager>().HideControls();
            transform.parent.GetChild(1).GetChild(0).GetComponent<Animation>().Play("MenuItemUngreyed");
        }

        menuManager.GetComponent<MenuManager>().Fade.GetComponent<Animation>().Play("TitleFadeOut");
    }

    IEnumerator BurstSpots()
    {
        emission.rateOverTime = 30;
        yield return null;
        emission.rateOverTime = 0; // ou rateOverTime si encore dessus
        velocity.radial = 15;
        while(velocity.radial.constant > 0)
        {
            velocity.radial = Mathf.Max(0, velocity.radial.constant - Time.deltaTime * 10);
            yield return null;
        }
    }
}
