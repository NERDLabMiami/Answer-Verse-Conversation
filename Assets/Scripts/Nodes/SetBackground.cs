using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace VNEngine
{
    public class SetBackground : Node
    {
        public enum Fade_Style
        {
            Fade_To_Black,  // Dips through solid black/white. Good for full scene changes.
            Fade_Alpha      // Fades transparency only, letting whatever is behind show through.
        }

        public Sprite sprite;
        public bool set_foreground;    // Foreground sits in front of Actors. Unchecked = sets the Background (behind Actors).
        public bool fade_out;          // Fades the current image out before/instead of showing the new one
        public bool fade_in;           // Fades the new image in instead of showing it instantly
        public Fade_Style fade_style = Fade_Style.Fade_To_Black;
        public float fade_duration = 2.0f;

        public override void Run_Node()
        {
            StopAllCoroutines();
            activateImage();    // Ensure whatever image we're operating on is active

            // Loading a save, or being inside a Begin/End Scene Setup bracket, should apply the
            // final state instantly, never play a fade animation
            if (executed_from_load || UIManager.scene_setup_active || (!fade_in && !fade_out))
            {
                Apply_Instant();
                Finish_Node();
                return;
            }

            if (fade_out && fade_in)
                StartCoroutine(Fade_Out_Then_In_Coroutine(fade_duration));
            else if (fade_in)
                StartCoroutine(Fade_In_Coroutine(fade_duration));
            else if (fade_out)
                StartCoroutine(Fade_Out_Coroutine(fade_duration));
        }


        private void Apply_Instant()
        {
            Image img = getImage();
            img.sprite = sprite;
            img.color = Color.white;

            if (set_foreground)
                SaveManager.SetSaveFeature(this, UIManager.ui_manager.foreground.gameObject);
            else
                SaveManager.SetSaveFeature(this, UIManager.ui_manager.background.gameObject);
        }


        // In case the foreground or background is disabled, activate it
        private void activateImage()
        {
            if (set_foreground)
                UIManager.ui_manager.foreground.gameObject.SetActive(true);
            else
                UIManager.ui_manager.background.gameObject.SetActive(true);
        }
        private Image getImage()
        {
            if (set_foreground)
                return UIManager.ui_manager.foreground;
            else
                return UIManager.ui_manager.background;
        }


        // The color the image should be in while it's fully hidden, for the current fade_style
        private Color Get_Hidden_Color(Image img)
        {
            if (fade_style == Fade_Style.Fade_To_Black)
                return Color.black;
            else
                return new Color(img.color.r, img.color.g, img.color.b, 0f);
        }

        // The color the image should be in while it's fully visible, for the current fade_style
        private Color Get_Visible_Color(Image img)
        {
            if (fade_style == Fade_Style.Fade_To_Black)
                return Color.white;
            else
                return new Color(img.color.r, img.color.g, img.color.b, 1f);
        }


        // Shared fade helper: lerps img.color from `from` to `to` over `over_time` seconds
        IEnumerator Fade_To_Color_Coroutine(Image img, Color from, Color to, float over_time)
        {
            img.color = from;

            float value = 0;
            while (value < 1f)
            {
                value += Time.deltaTime / Mathf.Max(over_time, 0.0001f);
                img.color = Color.Lerp(from, to, value);
                yield return null;
            }
            img.color = to;
        }


        IEnumerator Fade_Out_Then_In_Coroutine(float over_time)
        {
            Image img = getImage();
            Color visible = Get_Visible_Color(img);
            Color hidden = Get_Hidden_Color(img);

            yield return Fade_To_Color_Coroutine(img, visible, hidden, over_time / 2f);

            // Change the image while it's hidden and you can't tell it's being changed
            img.sprite = sprite;

            yield return Fade_To_Color_Coroutine(img, hidden, visible, over_time / 2f);

            SaveFeature();
            Finish_Node();
        }


        IEnumerator Fade_In_Coroutine(float over_time)
        {
            Image img = getImage();
            img.sprite = sprite;
            Color hidden = Get_Hidden_Color(img);
            Color visible = Get_Visible_Color(img);

            yield return Fade_To_Color_Coroutine(img, hidden, visible, over_time);

            SaveFeature();
            Finish_Node();
        }


        IEnumerator Fade_Out_Coroutine(float over_time)
        {
            Image img = getImage();
            Color visible = Get_Visible_Color(img);
            Color hidden = Get_Hidden_Color(img);

            yield return Fade_To_Color_Coroutine(img, visible, hidden, over_time);

            SaveFeature();
            Finish_Node();
        }


        // Ensure we save the foreground or background in case the user Saves and Loads the game later
        private void SaveFeature()
        {
            if (set_foreground)
                SaveManager.SetSaveFeature(this, UIManager.ui_manager.foreground.gameObject);
            else
                SaveManager.SetSaveFeature(this, UIManager.ui_manager.background.gameObject);
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
