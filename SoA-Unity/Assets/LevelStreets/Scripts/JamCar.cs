using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JamCar : MonoBehaviour
{
    private readonly float minDelay = 2; // seconds
    private readonly float maxDelay = 4; // seconds

    // Start is called before the first frame update
    void Start()
    {
        // Define which car horn type to use
        AkSoundEngine.SetSwitch("Klaxons", new string[5] { "A", "B", "C", "D", "E" }[Random.Range(0, 5)], gameObject);
        StartCoroutine("CarHornCountdown");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator CarHornCountdown()
    {
        float delay;
        for (; ; )
        {
            delay = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(delay);
            AkSoundEngine.PostEvent("Play_Klaxons", gameObject);
        }
    }
}
