using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace VNEngine
{
    // Removes the actor from the scene. If sliding out, waits until the actor if offscreen first
    public class ExitActorNode : Node
    {
        public Dialogue_Source actor_name_from = Dialogue_Source.Text_From_Editor;
        public string actor_name;
        public Actor actor_data;    // Reference to the Actor asset to remove. Used when actor_name_from == Text_From_Editor
        private string actual_actor_name;
        public bool slide_out = false;  // If true, have the character slide out before being destroyed
        public bool fade_out = false;
        public float fade_out_time = 1.0f;

        public override void Run_Node()
        {
            ActorInstance actor;

            // Text_From_Editor uses the drag-and-drop actor_data (immune to typos); the CSV/Stat
            // sources fall back to the freeform string field, since a dynamically-determined actor can't
            // be known as an object reference at edit time.
            if (actor_name_from == Dialogue_Source.Text_From_Editor)
            {
                if (actor_data == null)
                {
                    Debug.LogError("ExitActorNode has no Actor assigned.", this.gameObject);
                    Finish_Node();
                    return;
                }

                actor = ActorManager.Get_Actor(actor_data);
            }
            else
            {
                actual_actor_name = VNSceneManager.GetStringFromSource(actor_name, actor_name_from);
                actor = ActorManager.Get_Actor(actual_actor_name);
            }

            if (actor != null)
            {
                if (!slide_out && !fade_out)
                {
                    ActorManager.Remove_Actor(actor);
                    Finish_Node();
                }
                else
                {
                    if (fade_out)
                    {
                        // Fade out
                        actor.Fade_Out(fade_out_time);
                    }
                    else
                    {
                        // Slide out
                        actor.Slide_Out(1f);
                    }

                    StartCoroutine(Wait(actor));
                }
            }
            else
            {
                Debug.Log((actor_data != null ? actor_data.actor_name : actor_name) + " is not on the scene. Remember to correctly name your actor and use 'EnterActorNode'");

                Finish_Node();
            }
        }


        // Wait until the actor has left the scene
        IEnumerator Wait(ActorInstance actor)
        {
            while (actor != null && actor.gameObject.activeSelf)
                yield return new WaitForSeconds(0.1f);
            Finish_Node();
        }


        public override void Finish_Node()
        {
            base.Finish_Node();
        }
    }
}