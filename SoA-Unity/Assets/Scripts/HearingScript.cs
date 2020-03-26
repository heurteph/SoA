using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HearingScript : MonoBehaviour
{
    const int sampleNumber = 1024; // 32768; // 2^15 number of samples for 0,743 seconds
    const float sampleSeconds = 0.743f; // 32768 hertz / 44100

    [SerializeField]
    [Range(1, 5)]
    private int hearingMultiplier = 1;

    private int sampleTotal;

    private float[] sampleData;

    [SerializeField]
    private EnergyBehaviour energyBehaviour;


    [SerializeField]
    [Range(0, 0.2f)]
    private float loudnessThreshold;

    [SerializeField]
    [Range(0, 100)]
    private float loudnessDamage;

    private delegate void LoudnessThresholdHandler(float b);
    private event LoudnessThresholdHandler LoudnessThresholdEvent;

    // Awake Function
    void Awake()
    {
    sampleTotal = hearingMultiplier * sampleNumber;

    sampleData = new float[sampleTotal];

    }


    // Start is called before the first frame update
    void Start()
    {
        LoudnessThresholdEvent += energyBehaviour.DecreaseEnergy;

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
            //audioSource.GetOutputData(sampleData, 0);

            /*  CEST TROP LOURD §§§ ET CA MARCHE PAS §§ NIQUE TA RACE MACRON ET LE GONORAVERUS */
            for (int i = 0; i < sampleTotal; i++)
            {
                float sample = sampleData[i];

                Debug.Log("Sample : " + sample);
                loudness += Mathf.Abs(sample);
            }

            Debug.Log("Total : " + loudness);
            loudness /= sampleTotal; //Average Volume

            if (loudness >= loudnessThreshold)
            {
                LoudnessThresholdEvent(loudnessDamage);
            }

            yield return new WaitForSeconds(sampleSeconds*hearingMultiplier);
        }
    }


} // FINISH
