using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public delegate void SoundHandler();
    public event SoundHandler SoundPlayedEvent;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlaySound(string type, string id)
    {
        // TO DO : Set the data in the JSON file to match the Wwise name convention
        // AkSoundEngine.PostEvent(id, gameObject, (uint)AkCallbackType.AK_EndOfEvent, CallbackFunction, null);
        
        SoundPlayedEvent();
        Debug.Log("Son joué");
    }

    void CallbackFunction(object in_cookie, AkCallbackType in_type, object in_info)
    {
        SoundPlayedEvent();
    }
}
