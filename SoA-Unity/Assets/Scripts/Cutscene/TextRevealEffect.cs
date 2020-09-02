using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using story;

public class TextRevealEffect : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The cutscene manager")]
    private GameObject cutsceneManager;

    public delegate void MessageHandler();
    public event MessageHandler MessageShownEvent;

    //private string text;

    public delegate IEnumerator DisplayMessage(string text);
    DisplayMessage del;

    [Space]
    [Header("Message Animation Options")]

    [SerializeField]
    [Tooltip("The delay between two letters")]
    [Range(1,5)]
    private float messageSpeed = 1;

    [SerializeField]
    [Tooltip("The number of characters appearing at the same time")]
    [Range(1, 10)]
    private float rolloverCharacterSpread = 5;

    [SerializeField]
    [Tooltip("How long a message remains in seconds")]
    [Range(0.5f, 5)]
    private float messageDuration = 1.5f;

    private TextMeshProUGUI textMesh;

    // Start is called before the first frame update
    void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        if (textMesh == null)
        {
            throw new System.NullReferenceException("No TextMeshProUGUI attached to " + transform.name);
        }

        textMesh.text = string.Empty;
        
        del = new DisplayMessage(RevealFade);
    }

    public void WriteMessage(string text /*, callback */)
    {
        // Possible runtime change of the display method
        // DisplayMessage del = new DisplayMessage(callback);
        StartCoroutine(del(text));
    }

    IEnumerator RevealLetterByLetter(string text)
    {
        textMesh.text = text;
        textMesh.maxVisibleCharacters = 0;

        float delay;
        textMesh.ForceMeshUpdate(); // update textInfo to get actual characterCount
        for (int i = 0; i <= textMesh.textInfo.characterCount; i++)
        {
            textMesh.maxVisibleCharacters = i;
            delay = 0.1f / (2 * messageSpeed);
            yield return new WaitForSeconds(delay);
        }

        yield return new WaitForSeconds(messageDuration);

        MessageShownEvent();
    }

    IEnumerator RevealWordByWord(string text)
    {
        textMesh.text = text;
        textMesh.maxVisibleWords = 0;

        float delay;
        textMesh.ForceMeshUpdate(); // update textInfo to get actual wordCount
        for (int i = 0; i <= textMesh.textInfo.wordCount + 1; i++)
        {
            textMesh.maxVisibleWords = i;
            delay = 0.1f / (2 * messageSpeed);
            yield return new WaitForSeconds(delay);
        }

        yield return new WaitForSeconds(messageDuration);

        MessageShownEvent();
    }

    IEnumerator RevealFade(string text)
    {
        textMesh.text = text;
        textMesh.ForceMeshUpdate();

        TMP_TextInfo textInfo = textMesh.textInfo;
        Color32[] newVertexColors;

        int currentCharacterIndex = 0;
        int startingCharacterIndex = currentCharacterIndex;
        bool isRangeMax = false;

        while (!isRangeMax)
        {
            int characterCount = textInfo.characterCount;

            // Spread should not exceed the number of characters.
            //float RolloverCharacterSpread = 5; // how many character appearing at once
            byte fadeSteps = (byte)Mathf.Max(1, 255 / rolloverCharacterSpread);

            for (int i = startingCharacterIndex; i < currentCharacterIndex + 1; i++)
            {
                // Skip characters that are not visible
                if (!textInfo.characterInfo[i].isVisible) continue;

                // Get the index of the material used by the current character.
                int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;

                // Get the vertex colors of the mesh used by this text element (character or sprite).
                newVertexColors = textInfo.meshInfo[materialIndex].colors32;

                // Get the index of the first vertex used by this text element.
                int vertexIndex = textInfo.characterInfo[i].vertexIndex;

                // Get the current character's alpha value.
                byte alpha = (byte)Mathf.Clamp(newVertexColors[vertexIndex + 0].a + fadeSteps, 0, 255);

                // Set new alpha values.
                newVertexColors[vertexIndex + 0].a = alpha;
                newVertexColors[vertexIndex + 1].a = alpha;
                newVertexColors[vertexIndex + 2].a = alpha;
                newVertexColors[vertexIndex + 3].a = alpha;

                // Tint vertex colors
                // Note: Vertex colors are Color32 so we need to cast to Color to multiply with tint which is Color.
                //newVertexColors[vertexIndex + 0] = (Color)newVertexColors[vertexIndex + 0] * ColorTint;
                //newVertexColors[vertexIndex + 1] = (Color)newVertexColors[vertexIndex + 1] * ColorTint;
                //newVertexColors[vertexIndex + 2] = (Color)newVertexColors[vertexIndex + 2] * ColorTint;
                //newVertexColors[vertexIndex + 3] = (Color)newVertexColors[vertexIndex + 3] * ColorTint;

                if (alpha == 255)
                {
                    startingCharacterIndex += 1;

                    if (startingCharacterIndex == characterCount)
                    {
                        isRangeMax = true; // Would end the coroutine.

                        /*
                        
                        // Update mesh vertex data one last time.
                        textMesh.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

                        yield return new WaitForSeconds(1.0f);

                        // Reset the text object back to original state.
                        textMesh.ForceMeshUpdate();

                        yield return new WaitForSeconds(1.0f);

                        // Reset our counters.
                        currentCharacterIndex = 0;
                        startingCharacterIndex = 0;

                        */
                    }
                }
            }

            // Upload the changed vertex colors to the Mesh.
            textMesh.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

            if (currentCharacterIndex + 1 < characterCount) currentCharacterIndex += 1;

            yield return new WaitForSeconds(0.01f / messageSpeed);
        }

        yield return new WaitForSeconds(messageDuration);
        textMesh.alpha = 0;

        MessageShownEvent();
    }
}
