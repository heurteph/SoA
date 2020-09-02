using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImagesManager : MonoBehaviour
{
    [SerializeField]
    private GameObject background;

    [SerializeField]
    private GameObject character;

    private GameObject background1;
    private GameObject background2;

    public delegate void ImageHandler();
    public event ImageHandler ImageShownEvent;

    [Space]
    [Header("Image Animation Options")]

    [SerializeField]
    [Tooltip("The duration for one image to appear")]
    [Range(0.1f,1f)]
    private float fadeDuration = 0.5f;

    // Start is called before the first frame update
    void Awake()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeImage(string type, string id)
    {
        switch(type)
        {
            case "background":
                StartCoroutine(FadeBackground(id));
                break;

            case "character":
                break;
        }
    }

    private IEnumerator FadeBackground(string id)
    {
        Image image = background.GetComponent<Image>();

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

        image.color = new Color(0, 1, 0.5f, 0);
        //Sprite sprite = Resources.Load<Sprite>(string.Concat("Images/", id));
        //image.sprite = sprite;

        yield return new WaitForSeconds(halfwayPauseDuration);

        while (image.color.a != 1)
        {
            color = image.color;
            color.a = Mathf.Clamp(image.color.a + fadeStep, 0, 1);
            image.color = color;

            yield return new WaitForSeconds(timeStep);
        }

        ImageShownEvent();
        Debug.Log("Image affichée");
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
