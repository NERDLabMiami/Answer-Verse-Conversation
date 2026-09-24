using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace VNEngine
{
    // Moves the actor away from the center of the screen, while still staying on its current side (LEFT or RIGHT)
    public class MoveActorOutwardsNode : Node
    {
        public Actor actor_data;    // Reference to the Actor asset to move outwards. Used when actor_name_from == Text_From_Editor
        public Dialogue_Source actor_name_from = Dialogue_Source.Text_From_Editor;
        public string actor_name;   // Used when actor_name_from != Text_From_Editor (CSV key / Stat name)
        private string actual_actor_name;

        public bool wait_until_actor_has_stopped = false;


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

            if (actor == null)
            {
                Debug.LogError(actual_actor_name + " actor is not on the scene. Please use an Enter Actor Node for " + actual_actor_name);
                Finish_Node();
                return;
            }

            ActorManager.Move_Actor_Outwards(actor);

            if (wait_until_actor_has_stopped)
                StartCoroutine(Wait_Until_Actor_Is_Stopped());
            else
                Finish_Node();
        }

        IEnumerator Wait_Until_Actor_Is_Stopped()
        {
            yield return 0;

            ActorInstance actor = ActorManager.Get_Actor(actual_actor_name);
            if (actor == null)
            {
                Debug.Log("Could not find actor " + actual_actor_name);
                Finish_Node();
            }

            while (actor.is_moving)
                yield return 0;

            Finish_Node();
        }


        public override void Finish_Node()
        {
            base.Finish_Node();
        }
    }
}