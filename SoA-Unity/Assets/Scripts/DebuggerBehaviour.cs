using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class DebuggerBehaviour : MonoBehaviour
{
    // Inside Vision
    [SerializeField]
    private RawImage grayRawInsideImage;

    [SerializeField]
    private Slider insideVisionSlider;

    [SerializeField]
    private Slider thresholdInsideVisionSlider;

    [SerializeField]
    private VisionBehaviour insideVisionBehaviour;

    [SerializeField]
    private Text insideBrightnessPercentage;

    // Outside Vision
    [SerializeField]
    private RawImage grayRawOutsideImage;

    [SerializeField]
    private Slider outsideVisionSlider;

    [SerializeField]
    private Slider thresholdOutsideVisionSlider;

    [SerializeField]
    private VisionBehaviour outsideVisionBehaviour;

    [SerializeField]
    private Text outsideBrightnessPercentage;

    // Loudness
    [SerializeField]
    private Slider loudnessSlider;

    [SerializeField]
    private Slider loudnessThresholdSlider;

    [SerializeField]
    private HearingScript hearingScript;

    [SerializeField]
    private Text loudnessNumber;

    [SerializeField]
    private Slider energyBar;

    // Start is called before the first frame update
    void Start()
    {
        loudnessSlider.maxValue = Mathf.Min(hearingScript.LoudnessThreshold * 3,1); // TO REDESIGN
        loudnessThresholdSlider.maxValue = loudnessSlider.maxValue;
        energyBar.maxValue = 1000; // TO DO : Place in game manager
        energyBar.value = energyBar.maxValue;

        insideVisionSlider.maxValue = 1;
        thresholdInsideVisionSlider.maxValue = insideVisionSlider.maxValue;

        outsideVisionSlider.maxValue = 1;
        thresholdOutsideVisionSlider.maxValue = outsideVisionSlider.maxValue;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void DisplayVision(GameObject sender, Texture2D t2D, float percentage)
    {
        Debug.Log("Sender : " + sender.name);
        if (String.Equals(sender.name,"CameraInside"))
        {
            DisplayInsideVision(t2D, percentage);
            Debug.Log("Display from inside vision");
        }
        else if (String.Equals(sender.name,"CameraOutside"))
        {
            DisplayOutsideVision(t2D, percentage);
            Debug.Log("Display from outside vision");
        }
    }

    public void DisplayInsideVision (Texture2D t2D, float percentage) 
    {
        grayRawInsideImage.texture = t2D;
        insideBrightnessPercentage.text = Mathf.Round(percentage*1000)/10 + "%";

        insideVisionSlider.value = percentage;
        thresholdInsideVisionSlider.value = insideVisionBehaviour.PercentageThreshold;

        if (percentage >= insideVisionBehaviour.PercentageThreshold)
        {
            insideVisionSlider.fillRect.gameObject.GetComponent<Image>().color = Color.red;
        }
        else if (percentage >= insideVisionBehaviour.PercentageThreshold * 0.5f)
        {
            insideVisionSlider.fillRect.gameObject.GetComponent<Image>().color = Color.yellow;
        }
        else
        {
            insideVisionSlider.fillRect.gameObject.GetComponent<Image>().color = Color.white;
        }
    }

    public void DisplayOutsideVision(Texture2D t2D, float percentage)
    {
        grayRawOutsideImage.texture = t2D;
        outsideBrightnessPercentage.text = Mathf.Round(percentage * 1000) / 10 + "%";

        outsideVisionSlider.value = percentage;
        thresholdOutsideVisionSlider.value = outsideVisionBehaviour.PercentageThreshold;

        if (percentage >= outsideVisionBehaviour.PercentageThreshold)
        {
            outsideVisionSlider.fillRect.gameObject.GetComponent<Image>().color = Color.red;
        }
        else if (percentage >= outsideVisionBehaviour.PercentageThreshold * 0.5f)
        {
            outsideVisionSlider.fillRect.gameObject.GetComponent<Image>().color = Color.yellow;
        }
        else
        {
            outsideVisionSlider.fillRect.gameObject.GetComponent<Image>().color = Color.white;
        }
    }


    public void DisplayVolume (float volume)
    {
        loudnessSlider.value = volume;
        loudnessThresholdSlider.value = hearingScript.LoudnessThreshold;

        if (volume >= hearingScript.LoudnessThreshold)
        {
            loudnessSlider.fillRect.gameObject.GetComponent<Image>().color = Color.red;
        } 
        else if (volume >= hearingScript.LoudnessThreshold * 0.5f)
        {
            loudnessSlider.fillRect.gameObject.GetComponent<Image>().color = Color.yellow;
        }
        else
        {
            loudnessSlider.fillRect.gameObject.GetComponent<Image>().color = Color.white;
        }

        loudnessNumber.text = "" + volume;
    }

    public void DisplayEnergy (float energy)
    {
        energyBar.value = energy;
    }

} // FINISH
