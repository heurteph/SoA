using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostWwiseEvent : MonoBehaviour
{
    [SerializeField]
    private AK.Wwise.Event MyEvent;

    // Use this for initialization.
    void Start()
    {

    }

    // Update is called once per frame.
    void Update()
    {

    }
    public void PlayFootstepSound()
    {
        MyEvent.Post(gameObject);
    }
}