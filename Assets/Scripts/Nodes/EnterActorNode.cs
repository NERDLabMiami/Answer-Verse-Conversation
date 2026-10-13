using UnityEngine;
using System.Collections;

namespace VNEngine
{
    public enum Entrance_Type { Slide_In, Fade_In, None }

    public class EnterActorNode : Node
    {

        public string actor_name;
        public Actor actor_data;    // Reference to the Actor asset to bring on scene. Used when actor_name_from == Text_From_Editor
        public Dialogue_Source actor_name_from = Dialogue_Source.Text_From_Editor;    // Does our speakers name come from the editor, or a CSV?
        private string actual_actor_name;   // Value actually used within script. It's value depends on what actor_name_from is set to
        public Entrance_Type entrance_type;
        public float fade_in_time = 1.0f;   // How long it takes to fade in, if the fade_in bool is true
        public Actor_Positions destination; // Which space the will occupy
        public Transform custom_position;




        public override void Run_Node()
        {
            ActorInstance actor_script;

            // Text_From_Editor uses the drag-and-drop actor_data (immune to typos); the CSV/Stat
            // sources fall back to the freeform string field, since a dynamically-determined actor can't
            // be known as an object reference at edit time.
            if (actor_name_from == Dialogue_Source.Text_From_Editor)
            {
                if (actor_data == null)
                {
                    Debug.LogError("EnterActorNode has no Actor assigned.", this.gameObject);
                    Finish_Node();
                    return;
                }
                actual_actor_name = actor_data.actor_name;

                // Check if the actor is already present
                if (ActorManager.Is_Actor_On_Scene(actor_data))
                {
                    actor_script = ActorManager.Get_Actor(actor_data);
                    Finish_Node();
                    return;
                }

                if (VNSceneManager.verbose_debug_logs)
                    Debug.Log("Creating new actor " + actual_actor_name);

                actor_script = (destination == Actor_Positions.CUSTOM && custom_position != null)
                    ? ActorManager.Instantiate_Actor(actor_data, destination, custom_position)
                    : ActorManager.Instantiate_Actor(actor_data, destination);
            }
            else
            {
                // Get our actor name
                actual_actor_name = VNSceneManager.GetStringFromSource(actor_name, actor_name_from);

                // Check if the actor is already present
                if (ActorManager.Is_Actor_On_Scene(actual_actor_name))
                {
                    actor_script = ActorManager.Get_Actor(actual_actor_name);
                    Finish_Node();
                    return;
                }

                // Actor is not in the scene. Instantiate it
                if (VNSceneManager.verbose_debug_logs)
                    Debug.Log("Creating new actor " + actual_actor_name);

                actor_script = (destination == Actor_Positions.CUSTOM && custom_position != null)
                    ? ActorManager.Instantiate_Actor(actual_actor_name, destination, custom_position)
                    : ActorManager.Instantiate_Actor(actual_actor_name, destination);
            }
            SaveManager.SetSaveFeature(this, actor_script.gameObject);

            var effective_entrance = UIManager.scene_setup_active ? Entrance_Type.None : entrance_type;
            switch (effective_entrance)
            {
                case Entrance_Type.Slide_In:
                    actor_script.Slide_In(destination, 2.0f);
                    Finish_Node();
                    break;
                case Entrance_Type.Fade_In:
                    actor_script.Place_At_Position(destination);
                    actor_script.Fade_In(fade_in_time);
                    StartCoroutine(Wait(fade_in_time + 0.2f));
                    break;
                case Entrance_Type.None:
                    actor_script.Place_At_Position(destination);
                    Finish_Node();
                    break;
            }
        }

        IEnumerator Wait(float how_long)
        {
            yield return new WaitForSeconds(how_long);
            Finish_Node();
        }

        public override void Button_Pressed()
        {
            //Finish_Node();
        }


        public override void Finish_Node()
        {
            StopAllCoroutines();

            base.Finish_Node();
        }
    }
}