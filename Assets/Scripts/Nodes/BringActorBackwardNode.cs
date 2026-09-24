using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace VNEngine
{
    // Brings the actor to the back of the list of actors
    public class BringActorBackwardNode : Node
    {
        public Actor actor_data;    // Reference to the Actor asset to bring backward. Used when actor_name_from == Text_From_Editor
        public Dialogue_Source actor_name_from = Dialogue_Source.Text_From_Editor;
        public string actor_name;   // Used when actor_name_from != Text_From_Editor (CSV key / Stat name)
        private string actual_actor_name;

        public override void Run_Node()
        {
            // Text_From_Editor uses the drag-and-drop actor_data (immune to typos); the CSV/Stat
            // sources fall back to the freeform string field, since a dynamically-determined actor can't
            // be known as an object reference at edit time.
            ActorInstance actor;
            if (actor_name_from == Dialogue_Source.Text_From_Editor)
            {
                actual_actor_name = actor_data != null ? actor_data.actor_name : "";
                actor = actor_data != null ? ActorManager.Get_Actor(actor_data) : null;
            }
            else
            {
                actual_actor_name = VNSceneManager.GetStringFromSource(actor_name, actor_name_from);
                actor = ActorManager.Get_Actor(actual_actor_name);
            }

            if (actor != null)
                ActorManager.Bring_Actor_To_Back(actor);
            else
                Debug.LogWarning(actual_actor_name + " is not on the scene. Remember to correctly name your actor and use 'EnterActorNode'");

            Finish_Node();
        }


        public override void Finish_Node()
        {
            StopAllCoroutines();

            base.Finish_Node();
        }
    }
}