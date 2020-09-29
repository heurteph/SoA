using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsManager : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        AkSoundEngine.PostEvent("Play_Music_Menu", gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GoToTitle()
    {
        SceneManager.LoadScene("Title");
    }

    public void OnDestroy()
    {
        AkSoundEngine.PostEvent("Stop_Music_Menu", gameObject);
    }
}
