using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VNEngine
{
    public class RemoveImageFromCanvas : Node
    {
        public GameObject image;
        // Start is called before the first frame update
        public override void Run_Node()
        {
            image.SetActive(false);
            Finish_Node();
        }

        public override void Button_Pressed()
        {

        }

        public override void Finish_Node()
        {
            StopAllCoroutines();
            base.Finish_Node();
        }

    }
}
