using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace VNEngine
{
    public class HideTextNode : Node
    {
        public override void Run_Node()
        {
            UIManager.ui_manager.dialogue_text.gameObject.SetActive(false);

            Finish_Node();
        }


        public override void Button_Pressed()
        {

        }


        public override void Finish_Node()
        {
            base.Finish_Node();
        }
    }
}