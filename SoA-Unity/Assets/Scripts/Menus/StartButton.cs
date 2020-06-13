using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class StartButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private Color defaultColor;
    private Color hoveringColor;

    private GameObject creditsPanel;

    private ParticleSystem sunSpots;
    ParticleSystem.EmissionModule emission;

    float rateOverTime = 5;

    // Start is called before the first frame update
    void Start()
    {
        defaultColor = new Color(1f, 1f, 1f);
        hoveringColor = new Color(62f / 255f, 178f / 255f, 143f / 255f);

        creditsPanel = GameObject.FindGameObjectWithTag("MenuCredits");
        if (creditsPanel == null)
        {
            throw new System.NullReferenceException("Missing credits panel in the menu");
        }

        sunSpots = transform.GetChild(1).GetComponent<ParticleSystem>();
        emission = sunSpots.emission;
        emission.rateOverTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        //transform.GetChild(0).GetComponent<Text>().color = hoveringColor;
        transform.GetChild(0).GetComponent<Animation>().Play("MenuItemPop");
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
        if (creditsPanel.GetComponent<CanvasGroup>().alpha != 0)
        {
            creditsPanel.GetComponent<Animation>().Play("CreditsFadeOut");
        }
    }
}
