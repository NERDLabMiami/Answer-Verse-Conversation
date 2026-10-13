using UnityEngine;
using System.Collections;

namespace VNEngine
{
    // Manages the paused state of the game
    public class Pause : MonoBehaviour
    {
        public static Pause pause;

        public GameObject pause_menu;   // Must be assigned for pausing to work
        public GameObject options_menu; // Must be assigned for options to be displayed

        public string toggle_pause_key = "Pause";

        [HideInInspector]
        public bool paused = false;

        void Awake()
        {
            pause = this;
            AudioListener.pause = false;
        }
        void Start()
        {
            AudioListener.pause = false;
        }

        public void Toggle_Pause()
        {
            if (paused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }


        // Pause
        private void PauseGame()
        {
            AudioManager.audio_manager.Pause_Audio(true);
            pause_menu.SetActive(true);
            Time.timeScale = 0;
            paused = true;
        }
        // Resume/Unpause
        public void ResumeGame()
        {
            AudioManager.audio_manager.Pause_Audio(false);

            pause_menu.SetActive(false);
            Time.timeScale = 1;
            paused = false;
        }



        // Toggles the displaying of the options menu
        public void Toggle_Options()
        {
            options_menu.SetActive(!options_menu.activeSelf);
        }


        void Update()
        {
            if (Input.GetButtonDown(toggle_pause_key))
                Toggle_Pause();

            // Record time played here
        }


        public void SaveButtonClicked()
        {
            Debug.Log("Saving...");
            SaveFile s = new SaveFile();
            s.Save();
        }
    }
}