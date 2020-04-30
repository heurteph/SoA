using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AK.Wwise;

public class HearingScript : MonoBehaviour
{
    const int sampleNumber = 1024; // 32768; // 2^15 number of samples for 0,743 seconds
    const float sampleSeconds = 0.2f; //0.743f; // 32768 hertz / 48000

    private int sampleTotal;
    private float[] sampleData;

    [SerializeField]
    [Range(1, 5)]
    private int hearingMultiplier = 1;

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

    [SerializeField]
    private EnergyBehaviour energyBehaviour;

    [SerializeField]
    private DebuggerBehaviour debuggerBehaviour;

    private delegate void LoudnessHandler(float b);
    private event LoudnessHandler LoudnessThresholdEvent;
    private event LoudnessHandler LoudnessUpdateEvent;

    private delegate void DamagingSourceHandler(GameObject o);
    private event DamagingSourceHandler DamagingSourceEvent;

    private AudioManager audioManager;

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
        audioManager = AudioManager.Instance;
        if (audioManager == null)
        {
            throw new System.NullReferenceException("The audio manager could not be loaded");
        }

        LoudnessThresholdEvent += energyBehaviour.DecreaseEnergy;
        LoudnessUpdateEvent += debuggerBehaviour.DisplayVolume;

        // DamagingSourceEvent += cameraFollow.TargetingObstacle;

        StartCoroutine("Hear");
        //StartCoroutine("WWiseHear");
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
            //AudioListener.GetOutputData(sampleData+1024, 1);

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

                DamagingSourceEvent(ClosestAudioSource());
            }

            yield return new WaitForSeconds(sampleSeconds * hearingMultiplier);
        }
    }

    IEnumerator WwiseHear()
    {
        for (; ; )
        {
            float loudness = 0f;

            int type = 1;
            AkSoundEngine.GetRTPCValue("VolumeEcoutePerso", null, 0, out loudness, ref type);

            LoudnessUpdateEvent(loudness);

            if (loudness >= loudnessThreshold)
            {
                LoudnessThresholdEvent(loudnessDamage);

                DamagingSourceEvent(ClosestAudioSource());
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

    public GameObject ClosestAudioSource()
    {
        float minDistance = Mathf.Infinity;
        GameObject closestAudioSource = null;

        foreach (GameObject o in audioManager.GameObjectWithAudioSources)
        {
            if (o != null)
            {
                float distanceFromAudio = (o.transform.position - transform.parent.transform.parent.transform.position).magnitude;

                if (minDistance > distanceFromAudio)
                {
                    minDistance = distanceFromAudio;
                    closestAudioSource = o;
                }
            }
        }

        Debug.Log(closestAudioSource.transform.name + closestAudioSource.transform.position + " is the closest AudioSource");

        return closestAudioSource;
    }

} // FINISH
