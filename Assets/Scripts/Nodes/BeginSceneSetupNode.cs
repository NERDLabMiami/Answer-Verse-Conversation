using UnityEngine;

namespace VNEngine
{
    // Raises an opaque black curtain (the foreground layer) and marks the scene as being silently
    // assembled: EnterActorNode/ChangeActorImageNode/SetBackground skip their own animations while
    // this is active. Pair with EndSceneSetupNode to reveal the assembled scene in one fade.
    public class BeginSceneSetupNode : Node
    {
        public override void Run_Node()
        {
            var fg = UIManager.ui_manager.foreground;
            fg.gameObject.SetActive(true);
            fg.color = Color.black;
            UIManager.scene_setup_active = true;

            Finish_Node();
        }

        public override void Button_Pressed()
        {

        }
    }
}
