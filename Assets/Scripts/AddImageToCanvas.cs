using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VNEngine
{
    public class AddImageToCanvas : Node
    {
        public GameObject image;
        // Start is called before the first frame update
        public override void Run_Node()
        {
            image.transform.SetParent(UIManager.ui_manager.background_static_image_parent.transform, false);
            Finish_Node();

        }

        public override void Button_Pressed()
        {
//            image.SetActive(false);

        }

        public override void Finish_Node()
        {
            StopAllCoroutines();
            base.Finish_Node();
        }

    }
}
