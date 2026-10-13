namespace VNEngine
{
    // Fades the black curtain raised by BeginSceneSetupNode out (alpha only), revealing the scene
    // that was silently assembled underneath, then disables the curtain so it stops blocking input.
    public class EndSceneSetupNode : SetBackground
    {
        public override void Run_Node()
        {
            // A loaded save should just restore "curtain is down", never replay the reveal fade
            if (executed_from_load)
            {
                UIManager.ui_manager.foreground.gameObject.SetActive(false);
                Finish_Node();
                return;
            }

            UIManager.scene_setup_active = false;
            set_foreground = true;
            fade_in = false;
            fade_out = true;
            fade_style = Fade_Style.Fade_Alpha;

            base.Run_Node();
        }

        public override void Finish_Node()
        {
            UIManager.ui_manager.foreground.gameObject.SetActive(false);
            base.Finish_Node();
        }
    }
}
