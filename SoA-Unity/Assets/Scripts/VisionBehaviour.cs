using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisionBehaviour : MonoBehaviour
{
    [SerializeField]
    private Camera cameraInside;

    [SerializeField]
    private EnergyBehaviour energyBehaviour;

    [SerializeField]
    [Range(0,1)]
    private float percentageThreshold;

    [SerializeField]
    [Range(0, 255)]
    private float brightnessThreshold;

    [SerializeField]
    [Range(0, 100)]
    private float brightnessDamage;


    private delegate void BrightnessThresholdHandler(float b);
    private event BrightnessThresholdHandler brightnessThresholdEvent;



    void Awake()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        brightnessThresholdEvent += energyBehaviour.DecreaseEnergy;
    }

    // Update is called once per frame
    void Update()
    {
        Texture2D t2D = RenderTexturetoTexture2D(cameraInside.targetTexture);
        ComputeBrightnessAverage(t2D);
    }

    Texture2D RenderTexturetoTexture2D (RenderTexture rt)
    {
        Texture2D t2D = new Texture2D(256, 256, TextureFormat.RGB24, false);
        RenderTexture.active = rt;
        t2D.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        t2D.Apply();
        return t2D;
    }

    /* Many methods to compute brightness level of a frame */

    void ComputeBrightnessAverage (Texture2D t2D)
    {
        float sum = 0;

        for (int i=0; i < t2D.width; i++)
        {
            for (int j=0; j< t2D.height; j++)
            {
                if (t2D.GetPixel(i, j).grayscale*255 >= brightnessThreshold)
                {
                    sum++;
                }
            }
        }

        Debug.Log(sum);

        if (sum >= percentageThreshold*t2D.width*t2D.height)
        {
            brightnessThresholdEvent(brightnessDamage);
        }


    }

} // FINISH
