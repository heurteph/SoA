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
    private Text brightnessPercentage;

    [SerializeField]
    private Slider loudnessSlider;

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
        loudnessSlider.maxValue = Mathf.Min(hearingScript.loudnessThreshold * 3,1);

        energyBar.maxValue = 1000; //To do GAME MANAGER
        energyBar.value = energyBar.maxValue;

        insideVisionSlider.maxValue = 1;
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

        if (percentage >= visionBehaviour.percentageThreshold)
        {
            insideVisionSlider.fillRect.gameObject.GetComponent<Image>().color = Color.red;
            Debug.Log("red");
        }
        else if (percentage >= visionBehaviour.percentageThreshold * 0.5f)
        {
            insideVisionSlider.fillRect.gameObject.GetComponent<Image>().color = Color.yellow;
            Debug.Log("yellow");
        }
        else
        {
            insideVisionSlider.fillRect.gameObject.GetComponent<Image>().color = Color.white;
            Debug.Log("white");
        }
    }

    public void DisplayVolume (float volume)
    {
        loudnessSlider.value = volume;

        Debug.Log("loudness Threshold :" + hearingScript.loudnessThreshold);

        if (volume >= hearingScript.loudnessThreshold)
        {
            loudnessSlider.fillRect.gameObject.GetComponent<Image>().color = Color.red;
            Debug.Log("red");
        } 
        else if (volume >= hearingScript.loudnessThreshold * 0.5f)
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

    public void DisplayEnergy (float energy)
    {
        energyBar.value = energy;
    }

} // FINISH
