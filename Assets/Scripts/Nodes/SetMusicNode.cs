using UnityEngine;
using System.Collections;

namespace VNEngine
{
    // Sets the looping background music that's currently playing
    // This node must have a child object with an Audio Source so that the child
    // can be moved to be a child of MusicManager
    public class SetMusicNode : Node
    {
        public bool fadeOutPreviousMusic;
        public float fadeOutTime = 1.0f;

#if ANSWERVERSE_FMOD
        public string event_path;
#else
        public AudioClip new_music;
#endif
        public override void Run_Node()
        {
#if ANSWERVERSE_FMOD
            AudioManager.audio_manager.Set_Music_FMOD(event_path);
            Finish_Node();
#else
            if (new_music == null || new_music == AudioManager.audio_manager.background_music_audio_source.clip)
            {
                Finish_Node();
                return;
            }

            if (fadeOutPreviousMusic && AudioManager.audio_manager.background_music_audio_source.isPlaying)
            {
                // Fade out the previous background music for a smooth transition
                StartCoroutine(AudioManager.audio_manager.Fade_Out_Music(fadeOutTime));
                StartCoroutine(Wait(fadeOutTime));  // Wait, then add our background music
            }
            else
            {
                AudioManager.audio_manager.Set_Music(new_music);
                Finish_Node();
            }
#endif
        }


        // Waits a number of seconds before adding and playing the music. Allows the background music to fade out properly
        // before moving on.
        public IEnumerator Wait(float seconds)
        {
            yield return new WaitForSeconds(seconds);
#if !ANSWERVERSE_FMOD
            AudioManager.audio_manager.Set_Music(new_music);
#endif
            Finish_Node();
        }


        public override void Finish_Node()
        {
            StopAllCoroutines();

            base.Finish_Node();
        }
    }
}