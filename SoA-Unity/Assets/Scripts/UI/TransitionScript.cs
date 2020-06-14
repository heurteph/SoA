using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionScript : MonoBehaviour
{
    private GameObject singleton;

    private GameObject gameManager;

    private bool done;

    private void Awake()
    {
        // Must exist between scenes to achieve transitions
        if (singleton == null)
        {
            singleton = transform.parent.gameObject;
            DontDestroyOnLoad(transform.parent.gameObject); // save all canvas
        }
        else if (singleton != transform.parent.gameObject)
        {
            Destroy(transform.parent.gameObject); // destroy all canvas
            return;
        }

        gameManager = GameObject.FindGameObjectWithTag("GameManager");

        if (gameManager == null)
        {
            throw new System.NullReferenceException("Missing reference to the game manager");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        done = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TriggerCredits()
    {
        if (!done)
        {
            Debug.Log("Fin de l'anim de fade out avant les crédits");
            gameManager.GetComponent<GameManager>().DisplayCredits();
            done = true;
        }
    }
}
