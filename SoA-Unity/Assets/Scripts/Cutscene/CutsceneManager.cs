﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace story
{
    [System.Serializable]
    public class Story
    {
        public List<Scene> scenes;

        [System.Serializable]
        public class Scene
        {
            public List<Action> actions;

            [System.Serializable]
            public class Action
            {
                public string type;
                public string metadata;
                public string data;
            };
        };

        public Story(string text)
        {
            Deserialize(text);
        }
        public void Deserialize(string text)
        {
            JsonUtility.FromJsonOverwrite(text, this);
        }
    }


    public class CutsceneManager : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Story in JSON format")]
        private TextAsset jsonData;

        [SerializeField]
        [Tooltip("Story deserialized")]
        private Story story;

        private List<Story.Scene>.Enumerator itScenes;
        private List<Story.Scene.Action>.Enumerator itActions;

        [Space]

        // TO DO : Use dictionaries or hashmaps instead of list to find them easily

        [SerializeField]
        [Tooltip("List of the images")]
        private Image[] images;

        [SerializeField]
        [Tooltip("List of the sounds")]
        private AkEvent[] sounds;

        [Space]

        [SerializeField]
        [Tooltip("The message manager")]
        private GameObject messagesManager;

        [SerializeField]
        [Tooltip("The images manager")]
        private GameObject imagesManager;

        [SerializeField]
        [Tooltip("The sound manager")]
        private GameObject soundManager;

        public delegate void ReadyHandler(string type, string id);
        public event ReadyHandler NewMessageEvent;
        public event ReadyHandler NewImageEvent;
        public event ReadyHandler NewSoundEvent;

        // Start is called before the first frame update
        void Start()
        {
            // Callback functions
            NewMessageEvent += messagesManager.GetComponent<MessagesManager>().WriteMessage;
            NewImageEvent += imagesManager.GetComponent<ImagesManager>().ChangeImage;
            NewSoundEvent += soundManager.GetComponent<SoundManager>().PlaySound;

            messagesManager.GetComponent<MessagesManager>().MessageShownEvent += ExecuteNextAction;
            imagesManager.GetComponent<ImagesManager>().ImageShownEvent += ExecuteNextAction;
            soundManager.GetComponent<SoundManager>().SoundPlayedEvent += ExecuteNextAction;

            // TO DO : Runtime loading of the json file for multilanguage support (EN/FR)
            //jsonData = Resources.Load<TextAsset>("Cutscenes/cutscenes");

            story = new Story(jsonData.text);
            itScenes = story.scenes.GetEnumerator();
            itScenes.MoveNext(); // Initialization ?
            itActions = itScenes.Current.actions.GetEnumerator();

            // Start with the first action
            ExecuteNextAction();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void ExecuteNextAction()
        {
            ExecuteAction(GetNextAction());
        }

        public Story.Scene.Action GetNextAction()
        {
            if (!itActions.MoveNext())
            {
                if (!itScenes.MoveNext())
                {
                    Debug.Log("Fin de la cutscene");
                    // Close cutscene and start game
                    return null;
                }
                //Debug.Log("Scène suivante");
                itActions = itScenes.Current.actions.GetEnumerator();
                itActions.MoveNext(); // Initialization
            }
            return itActions.Current;
        }

        void ExecuteAction(Story.Scene.Action action)
        {
            if (action == null) return;

            switch (action.type)
            {
                case "message":

                    NewMessageEvent(action.metadata, action.data);
                    break;

                case "image":

                    NewImageEvent(action.metadata, action.data);
                    break;

                case "sound":

                    NewSoundEvent(action.metadata, action.data);
                    break;

                default:

                    ExecuteNextAction(); // Skip unknown actions
                    break;
            }
        }
    }
}
