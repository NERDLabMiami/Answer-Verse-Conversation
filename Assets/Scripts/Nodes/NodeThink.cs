using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

namespace VNEngine
{
    public class NodeThink : Node
    {
        public Thought thought;
        public Sprite characterImage;
        public TranslatedText[] thoughts;
        public float timeBetweenThoughts = 5.0f;

        private bool thinking = true;
        private float nextThoughtTime;
        private int thoughtIndex = 0;
        private Coroutine thinkingCoroutine;

        // Called initially when the node is run, put most of your logic here
        public override void Run_Node()
        {
            thoughtIndex = 0; // Reset the thought index
            thinking = true; // Ensure thinking is set to true
            nextThoughtTime = Time.time; // Initialize nextThoughtTime

            thought.gameObject.SetActive(true);
            thought.SetCharacter(characterImage);
            thought.SetThought(thoughts[thoughtIndex]);
            thought.ResetColors();

            if (thinkingCoroutine != null)
            {
                StopCoroutine(thinkingCoroutine);
            }
            thinkingCoroutine = StartCoroutine(Thinking_Coroutine());
        }

        // Fades the image from opaque to transparent
        IEnumerator Thinking_Coroutine()
        {
            while (thinking)
            {
                if (Time.time >= nextThoughtTime)
                {
                    if (thoughtIndex < thoughts.Length - 1)
                    {
                        if (thoughtIndex != 0)
                        {
                            Debug.Log("Fading Out Previous Thought");
                            // Fade out previous thought
                            thought.FadeOutText();
                            yield return new WaitForSeconds(timeBetweenThoughts);
                        }

                        thoughtIndex++;
                        thought.SetThought(thoughts[thoughtIndex]);
                        nextThoughtTime = Time.time + timeBetweenThoughts;
                    }
                    else
                    {
                        // For the last thought, just wait for the remaining time
                        yield return new WaitForSeconds(timeBetweenThoughts);
                        thinking = false;
                    }
                }
                yield return null;
            }

            // All thoughts displayed, finish node
            Finish_Node();
        }

        // Do any necessary cleanup here
        public override void Finish_Node()
        {
            if (thinkingCoroutine != null)
            {
                StopCoroutine(thinkingCoroutine);
                thinkingCoroutine = null;
            }

            VNSceneManager.Waiting_till_true = true;
            thought.gameObject.SetActive(false);
            Debug.Log("Waiting is over, setting true: " + gameObject.name);
            base.Finish_Node();
        }
    }
}
