using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VNEngine { 

public class MoveActor : Node
{
        Dialogue_Source actor_name_from = Dialogue_Source.Text_From_Editor;
        public string actor_name;
        private string actual_actor_name;
        public float distance;
        public bool wait_until_actor_has_stopped = false;

        public override void Run_Node()
        {

            actual_actor_name = VNSceneManager.GetStringFromSource(actor_name, actor_name_from);

            if (!ActorManager.Is_Actor_On_Scene(actual_actor_name))
            {
                Debug.LogError(actual_actor_name + " actor is not on the scene. Please use an Enter Actor Node for " + actual_actor_name);
                Finish_Node();
                return;
            }

            ActorManager.Get_Actor(actual_actor_name).desired_position.x += distance;
            //            ActorManager.Move_Actor_Outwards(ActorManager.Get_Actor(actual_actor_name));

            if (wait_until_actor_has_stopped)
                StartCoroutine(Wait_Until_Actor_Is_Stopped());
            else
                Finish_Node();
        }

        IEnumerator Wait_Until_Actor_Is_Stopped()
        {
            yield return 0;

            Actor actor = ActorManager.Get_Actor(actual_actor_name);
            if (actor == null)
            {
                Debug.Log("Could not find actor " + actual_actor_name);
                Finish_Node();
            }

            while (actor.is_moving)
                yield return 0;

            Finish_Node();
        }

        // Do any necessary cleanup here, like stopping coroutines that could still be running and interfere with future nodes
        public override void Finish_Node()
        {
            StopAllCoroutines();

            base.Finish_Node();
        }


    }

}
