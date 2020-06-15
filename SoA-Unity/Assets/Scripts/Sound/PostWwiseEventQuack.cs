using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostWwiseEventQuack : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("QuackQuack");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator QuackQuack()
    {
        float minDelay = 20, maxDelay = 30;
        for (; ; )
        {
            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));
            AkSoundEngine.PostEvent("Play_Canard", gameObject);
        }
    }
}
