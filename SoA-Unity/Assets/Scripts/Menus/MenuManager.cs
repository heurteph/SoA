﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum MENU_STATE { NONE, CREDITS, CONTROLS }

public class MenuManager : MonoBehaviour
{
    private GameObject creditsPanel;
    private GameObject corePanel;
    private GameObject extendedPanel;

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

        corePanel = creditsPanel.transform.GetChild(0).gameObject;
        extendedPanel = creditsPanel.transform.GetChild(1).gameObject;

        foreach (Transform child in corePanel.transform)
        {
            child.GetComponent<Text>().color = new Color(1, 1, 1, 0);
        }
        foreach (Transform child in extendedPanel.transform)
        {
            child.GetComponent<Text>().color = new Color(1, 1, 1, 0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
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

    IEnumerator FadeInCredits()
    {
        foreach (Transform child in corePanel.transform)
        {
            child.GetComponent<Animation>().Play("CreditsItemFadeIn");
            yield return new WaitForSeconds(0.01f);
        }
        foreach (Transform child in extendedPanel.transform)
        {
            child.GetComponent<Animation>().Play("CreditsItemFadeIn");
            yield return new WaitForSeconds(0.01f);
        }
    }

    IEnumerator FadeOutCredits()
    {
        foreach (Transform child in corePanel.transform)
        {
            child.GetComponent<Animation>().Play("CreditsItemFadeOut");
            yield return new WaitForSeconds(0.01f);
        }
        foreach (Transform child in extendedPanel.transform)
        {
            child.GetComponent<Animation>().Play("CreditsItemFadeOut");
            yield return new WaitForSeconds(0.01f);
        }
    }
}
