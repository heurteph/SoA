using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebuggerBehaviour : MonoBehaviour
{

    [SerializeField]
    private RawImage grayRawImage;

    [SerializeField]
    private Text brightnessPercentage;

    [SerializeField]
    private Slider loudnessSlider;

    [SerializeField]
    private HearingScript hearingScript;

    [SerializeField]
    private Text loudnessNumber;


    // Start is called before the first frame update
    void Start()
    {
        loudnessSlider.maxValue = Mathf.Min(hearingScript.loudnessThreshold * 3,1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DisplayTexture2D (Texture2D t2D, float percentage) 
    {
        grayRawImage.texture = t2D;
        brightnessPercentage.text = Mathf.Round(percentage*1000)/10 + "%";
    }

    public void DisplayVolume (float volume)
    {
        loudnessSlider.value = volume;

        Debug.Log("loudness Threshold :" + hearingScript.loudnessThreshold);

        if (volume > hearingScript.loudnessThreshold * 2)
        {
            loudnessSlider.fillRect.gameObject.GetComponent<Image>().color = Color.red;
            Debug.Log("red");
        } 
        else if (volume > hearingScript.loudnessThreshold)
        {
            loudnessSlider.fillRect.gameObject.GetComponent<Image>().color = Color.yellow;
            Debug.Log("yellow");
        }
        else
        {
            loudnessSlider.fillRect.gameObject.GetComponent<Image>().color = Color.white;
            Debug.Log("white");
        }

        loudnessNumber.text = "" + volume;
    }

} // FINISH
