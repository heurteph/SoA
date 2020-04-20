using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebuggerBehaviour : MonoBehaviour
{

    [SerializeField]
    private RawImage grayRawImage;

    [SerializeField]
    private Slider insideVisionSlider;

    [SerializeField]
    private Slider thresholdVisionSlider;

    [SerializeField]
    private Text brightnessPercentage;

    [SerializeField]
    private Slider loudnessSlider;

    [SerializeField]
    private Slider loudnessThresholdSlider;

    [SerializeField]
    private VisionBehaviour visionBehaviour;

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
        thresholdVisionSlider.maxValue = insideVisionSlider.maxValue;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DisplayVision (Texture2D t2D, float percentage) 
    {
        grayRawImage.texture = t2D;
        brightnessPercentage.text = Mathf.Round(percentage*1000)/10 + "%";

        insideVisionSlider.value = percentage;
        thresholdVisionSlider.value = visionBehaviour.PercentageThreshold;

        if (percentage >= visionBehaviour.PercentageThreshold)
        {
            insideVisionSlider.fillRect.gameObject.GetComponent<Image>().color = Color.red;
        }
        else if (percentage >= visionBehaviour.PercentageThreshold * 0.5f)
        {
            insideVisionSlider.fillRect.gameObject.GetComponent<Image>().color = Color.yellow;
        }
        else
        {
            insideVisionSlider.fillRect.gameObject.GetComponent<Image>().color = Color.white;
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
