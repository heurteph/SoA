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
    public void PlayRightFootstepSound()
    {
        AKRESULT result = AkSoundEngine.SetSwitch("Droit_Gauche", "Droit", gameObject); // Right foot step sounds
        if(result == AKRESULT.AK_Fail)
        {
            throw new System.Exception("Could set the side of the footstep wwise sound");
        }
        MyEvent.Post(gameObject);
    }

    public void PlayLeftFootstepSound()
    {
        AKRESULT result = AkSoundEngine.SetSwitch("Droit_Gauche", "Gauche", gameObject); // Left foot step sounds

        if (result == AKRESULT.AK_Fail)
        {
            throw new System.Exception("Could set the side of the footstep wwise sound");
        }
        MyEvent.Post(gameObject);
    }
}