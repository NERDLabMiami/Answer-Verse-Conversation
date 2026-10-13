using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace VNEngine
{
    // Assigns or completes a to-do task via TaskManager.
    public class NodeTask : Node
    {
        public string taskTitle;
        public bool complete;

        // Called initially when the node is run, put most of your logic here
        public override void Run_Node()
        {

            if(!complete)
            {
                TaskManager.Instance.AssignTask(taskTitle);
            }
            else
            {
                TaskManager.Instance.CompleteTask(taskTitle);
            }


            Finish_Node();
        }


        // What happens when the user clicks on the dialogue text or presses spacebar? Either nothing should happen, or you call Finish_Node to move onto the next node
        public override void Button_Pressed()
        {
            //Finish_Node();
        }


        // Do any necessary cleanup here, like stopping coroutines that could still be running and interfere with future nodes
        public override void Finish_Node()
        {
            StopAllCoroutines();

            base.Finish_Node();
        }
    }
}