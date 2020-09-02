using System.Collections;
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
        [Tooltip("The message box")]
        private TextMeshProUGUI messageBox;

        [SerializeField]
        [Tooltip("The header of the message box")]
        private Text nameTag;

        public delegate void ReadyHandler(string text);
        public event ReadyHandler NewMessageEvent;

        // Start is called before the first frame update
        void Start()
        {
            // Callback functions
            NewMessageEvent += messageBox.GetComponent<TextRevealEffect>().WriteMessage;
            messageBox.GetComponent<TextRevealEffect>().MessageShownEvent += ExecuteNextAction;

            // TO DO : Runtime loading of the json file for multilanguage support (EN/FR)
            //jsonData = Resources.Load<TextAsset>("Cutscenes/cutscenes");

            story = new Story(jsonData.text);
            itScenes = story.scenes.GetEnumerator();
            itScenes.MoveNext(); // Initialization ?
            itActions = itScenes.Current.actions.GetEnumerator();

            ExecuteNextAction();
            //StartCoroutine("PlayAllCutscene");
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
                Debug.Log("Scène suivante");
                itActions = itScenes.Current.actions.GetEnumerator();
                itActions.MoveNext(); // Initialization
            }
            Debug.Log("Action suivante");
            return itActions.Current;
        }

        void ExecuteAction(Story.Scene.Action action)
        {
            if (action == null) return;

            switch (action.type)
            {
                case "message":

                    nameTag.text = action.metadata;
                    //messageBox.text = action.data;
                    NewMessageEvent(action.data);
                    break;

                case "sound":

                    // TO DO : Play a sound
                    ExecuteNextAction(); // skip for now
                    break;

                case "image":

                    // TO DO : Display an image
                    ExecuteNextAction(); // skip for now
                    break;

                default:

                    // TO DO : Skip action
                    break;
            }
        }

        /*
        IEnumerator PlayAllCutscene()
        {
            foreach (var scene in story.scenes)
            {
                foreach (var action in scene.actions)
                {
                    Debug.Log("event : " + action.type + ", " + action.metadata + ", " + action.data);

                    ExecuteAction(action);

                    yield return new WaitForSeconds(0.5f);
                }
            }
        }
        */
    }
}
