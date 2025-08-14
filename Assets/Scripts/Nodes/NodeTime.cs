using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace VNEngine
{
    // Not used in real code. Merely a template to copy and paste from when creating new nodes.
    public class NodeTime : Node
    {
        public float weeksPassed;

        // Called initially when the node is run, put most of your logic here
        public override void Run_Node()
        {

            if(StatsManager.Numbered_Stat_Exists("weeks"))
            {
                StatsManager.Add_To_Numbered_Stat("weeks", weeksPassed);
            }
            else
            {
                StatsManager.Set_Numbered_Stat("weeks", weeksPassed);
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