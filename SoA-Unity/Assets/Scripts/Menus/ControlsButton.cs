using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

public class ControlsButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, ISelectHandler, IDeselectHandler
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
        emission.rateOverTime = 0;
        velocity = sunSpots.velocityOverLifetime;
        velocity.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        // take the particle system to the center of the button
        Vector2 center = transform.GetChild(0).GetComponent<RectTransform>().rect.center;
        transform.GetChild(1).position = transform.GetComponent<RectTransform>().TransformPoint(new Vector3(center.x, center.y, -250));
    }

    /* Mouse */

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        GetComponent<Button>().Select();
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        ExitButtonAnimation();
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        ValidateButtonAnimation();
    }

    /* Keyboard */

    void ISelectHandler.OnSelect(BaseEventData eventData)
    {
        EnterButtonAnimation();
    }

    void IDeselectHandler.OnDeselect(BaseEventData eventData)
    {
        ExitButtonAnimation();
    }

    /* Generic */

    private void EnterButtonAnimation()
    {
        if (menuManager.GetComponent<MenuManager>().MenuState != MENU_STATE.CONTROLS)
        {
            transform.GetChild(0).GetComponent<Animation>().Play("MenuItemColorIn");
            emission.rateOverTime = rateOverTime;
        }
    }

    private void ExitButtonAnimation()
    {
        if (menuManager.GetComponent<MenuManager>().MenuState != MENU_STATE.CONTROLS)
        {
            transform.GetChild(0).GetComponent<Animation>().Play("MenuItemColorOut");
            emission.rateOverTime = 0;
        }
    }

    private void ValidateButtonAnimation()
    {
        if (menuManager.GetComponent<MenuManager>().MenuState == MENU_STATE.CREDITS)
        {
            menuManager.GetComponent<MenuManager>().HideCredits();
            transform.parent.GetChild(2).GetChild(0).GetComponent<Animation>().Play("MenuItemUngreyed");
        }
        if (menuManager.GetComponent<MenuManager>().MenuState != MENU_STATE.CONTROLS)
        {
            menuManager.GetComponent<MenuManager>().DisplayControls();
            transform.GetChild(0).GetComponent<Animation>().Play("MenuItemFlash");
            StartCoroutine("BurstSpots");
        }
    }

    IEnumerator BurstSpots()
    {
        emission.rateOverTime = 30;
        yield return null;
        emission.rateOverTime = 0; // ou rateOverTime si encore dessus
        velocity.radial = 15;
        while (velocity.radial.constant > 0)
        {
            velocity.radial = Mathf.Max(0, velocity.radial.constant - Time.deltaTime * 10);
            yield return null;
        }
    }
}
