﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImagesManager : MonoBehaviour
{
    [SerializeField]
    private GameObject background;

    [SerializeField]
    private GameObject right;

    [SerializeField]
    private GameObject left;

    public delegate void ImageHandler();
    public event ImageHandler ImageShownEvent;

    [Space]
    [Header("Image Animation Options")]

    [SerializeField]
    [Tooltip("The duration for one image to appear")]
    [Range(0.1f,1f)]
    private float fadeDuration = 1f;

    // Start is called before the first frame update
    void Awake()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeImage(string position, string id)
    {
        StartCoroutine(FadeImage(position, id));
    }

    private IEnumerator FadeImage(string position, string id)
    {
        Image image = null;

        switch (position)
        {
            case "background":
                image = background.GetComponent<Image>();
                break;

            case "left":
                image = left.GetComponent<Image>();

                ImageShownEvent();
                break;

            case "right":
                image = right.GetComponent<Image>();

                ImageShownEvent();
                break;
        }

        Color color;
        color = image.color;
        color.a = 1;
        image.color = color;

        //float fadeDuration = 5; // in seconds
        float timeStep = 0.1f;  // in seconds

        float halfwayPauseDuration = 0.1f;

        float halfDuration = fadeDuration / 2f - halfwayPauseDuration;

        float fadeStep = timeStep / halfDuration;

        while (image.color.a != 0)
        {
            color = image.color;
            color.a = Mathf.Clamp(image.color.a - fadeStep, 0, 1);
            image.color = color;

            yield return new WaitForSeconds(timeStep);
        }

        // For test
        // image.color = new Color(0, 1, 0.5f, 0);

        //TO DO : Load the actual sprite

        Sprite sprite = Resources.Load<Sprite>(string.Concat("Cutscene\\Images\\", id));
        if(sprite == null)
        {
            throw new InvalidResourceException(id, "Cutscene\\Images\\");
        }
        image.sprite = sprite;
        image.preserveAspect = true;

        yield return new WaitForSeconds(halfwayPauseDuration);

        while (image.color.a != 1)
        {
            color = image.color;
            color.a = Mathf.Clamp(image.color.a + fadeStep, 0, 1);
            image.color = color;

            yield return new WaitForSeconds(timeStep);
        }

        if(position == "background")
            ImageShownEvent();
    }

    /*
    private IEnumerator CrossFade(string id)
    {
        Image fadeIn  = (background == background1 ? background2 : background1).GetComponent<Image>(),
              fadeOut = (background == background1 ? background1 : background2).GetComponent<Image>();
        Color color;
        color = fadeIn.color;
        color.a = 0;
        fadeIn.color = color;
        color = fadeOut.color;
        color.a = 1;
        fadeOut.color = color;

        //float fadeDuration = 5; // in seconds
        float timeStep = 0.1f;  // in seconds

        float fadeStep = timeStep / fadeDuration;

        while(fadeIn.color.a != 1)
        {
            color = fadeIn.color;
            color.a = Mathf.Clamp(fadeIn.color.a + fadeStep, 0, 1);
            fadeIn.color = color;

            color = fadeOut.color;
            color.a = Mathf.Clamp(fadeOut.color.a - fadeStep, 0, 1);
            fadeOut.color = color;

            yield return new WaitForSeconds(timeStep);
        }

        ImageShownEvent();
    }
    */
}
