using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HearingScript : MonoBehaviour
{
    const int sampleNumber = 1024; // 32768; // 2^15 number of samples for 0,743 seconds
    const float sampleSeconds = 0.2f; //0.743f; // 32768 hertz / 44100

    [SerializeField]
    [Range(1, 5)]
    private int hearingMultiplier = 1;

    private int sampleTotal;

    private float[] sampleData;

    [SerializeField]
    private EnergyBehaviour energyBehaviour;

    [SerializeField]
    private DebuggerBehaviour debuggerBehaviour;

    [SerializeField]
    [Range(0, 0.2f)]
    private float normalLoudnessThreshold = 0.015f;

    [SerializeField]
    [Range(0, 0.2f)]
    private float protectedLoudnessThreshold = 0.02f;

    private float loudnessThreshold;
    public float LoudnessThreshold { get { return loudnessThreshold; } }

    [SerializeField]
    [Range(0, 100)]
    private float loudnessDamage = 25;

    private delegate void LoudnessHandler(float b);
    private event LoudnessHandler LoudnessThresholdEvent;
    private event LoudnessHandler LoudnessUpdateEvent;


    // Awake Function
    void Awake()
    {
        sampleTotal = hearingMultiplier * sampleNumber;
        sampleData = new float[sampleTotal];
        loudnessThreshold = normalLoudnessThreshold;
    }

    // Start is called before the first frame update
    void Start()
    {
        LoudnessThresholdEvent += energyBehaviour.DecreaseEnergy;
        LoudnessUpdateEvent += debuggerBehaviour.DisplayVolume;

        StartCoroutine("Hear");
    }    

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator Hear()
    {
        for (; ;)
        {
            float loudness = 0f; 

            AudioListener.GetOutputData(sampleData, 0);
            //audioSource.GetOutputData(sampleData, 1);

            for (int i = 0; i < sampleTotal; i++)
            {
                float sample = sampleData[i];
                loudness += Mathf.Abs(sample);
            }

            loudness /= sampleTotal; //Average Volume
            LoudnessUpdateEvent(loudness);

            if (loudness >= loudnessThreshold)
            {
                LoudnessThresholdEvent(loudnessDamage);
            }

            yield return new WaitForSeconds(sampleSeconds * hearingMultiplier);
        }
    }

    public void PlugEars()
    {
        loudnessThreshold = protectedLoudnessThreshold;
    }

    public void UnplugEars()
    {
        loudnessThreshold = normalLoudnessThreshold;
    }

} // FINISH
