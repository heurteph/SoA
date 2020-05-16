using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostWwiseEventCry : MonoBehaviour
{
    [SerializeField]
    private AK.Wwise.Event MyEvent;

    // Use this for initialization.
    void Start()
    {

    }

    // Update is called once per frame.
    private void Update()
    {
        
    }

    public void PlayCrySound()
    {
        MyEvent.Post(gameObject);
    }
}
