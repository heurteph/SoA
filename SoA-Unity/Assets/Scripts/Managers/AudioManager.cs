using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    public static AudioManager Instance { get { return instance; } private set { instance = value; } }

    private GameObject[] gameObjectWithAudioSources;
    public GameObject[] GameObjectWithAudioSources { get { return gameObjectWithAudioSources; } private set { gameObjectWithAudioSources = value; } }

    private void Awake()
    {
        if(instance != null && this != instance)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        gameObjectWithAudioSources = FindObjectsOfType<AudioSource>().Select(item => item.gameObject).ToArray();

        Debug.Log(gameObjectWithAudioSources.Length + " AudioSources");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
} // FINISH
