using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
#if ANSWERVERSE_FMOD
using FMODUnity;
using FMOD.Studio;
using STOP_MODE = FMOD.Studio.STOP_MODE;
#endif

namespace VNEngine
{
    // Single audio seam for the whole project. Plain Unity AudioSource playback is the
    // default; defining ANSWERVERSE_FMOD in Player Settings > Scripting Define Symbols
    // switches music/ambience/SFX to FMOD. Voice-over and talking beeps always play
    // through plain AudioSources in both modes (see Play_Voice_Clip/Play_Talking_Beep).
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager audio_manager;

#if ANSWERVERSE_FMOD
        public StudioEventEmitter ambience_emitter;
        private EventInstance music_instance;
        private string current_music_event_path = "";
#else
        public AudioSource ambience_audio_source;
        public AudioSource background_music_audio_source;
#endif
        // Voice always plays through this AudioSource, in both backends
        public AudioSource voice_audio_source;

        bool muted = false;     // If muted, NO audio will play
        public GameObject sound_effect_prefab;  // Must be an object with an audiosource, and necessary for PlaySoundEffect nodes (non-FMOD path)

        // Volume controls
        [HideInInspector]
        public float master_volume = 1;
        [HideInInspector]
        public float music_volume = 1;
        [HideInInspector]
        public float voice_volume = 1;
        [HideInInspector]
        public float effects_volume = 1;

        public Slider Master_Volume_Slider;
        public Slider Music_Volume_Slider;
        public Slider Voice_Volume_Slider;
        public Slider Effects_Volume_Slider;

        // Talking beeps always round-robin through this pool, in both backends -
        // per-character playback is a bad fit for FMOD event instances
        public List<AudioSource> talking_beeps_list;


        void Awake()
        {
            audio_manager = this;
        }
        void Start()
        {
            // Load player preferences
            Master_Volume_Changed(PlayerPrefs.GetFloat("MasterVolume", 1));
            Voice_Volume_Changed(PlayerPrefs.GetFloat("VoiceVolume", 1));
            Music_Volume_Changed(PlayerPrefs.GetFloat("MusicVolume", 1));
            Effects_Volume_Changed(PlayerPrefs.GetFloat("EffectsVolume", 1));

            // Set sliders
            if (Master_Volume_Slider)
                Master_Volume_Slider.value = master_volume;
            if (Music_Volume_Slider)
                Music_Volume_Slider.value = music_volume;
            if (Voice_Volume_Slider)
                Voice_Volume_Slider.value = voice_volume;
            if (Effects_Volume_Slider)
                Effects_Volume_Slider.value = effects_volume;
        }


        // Fades out the current background music over seconds_it_takes_to_fade_out
        public IEnumerator Fade_Out_Music(float seconds_it_takes_to_fade_out)
        {
#if ANSWERVERSE_FMOD
            Stop_Music(true);
            yield return null;
#else
            if (background_music_audio_source != null)
            {
                while (background_music_audio_source.volume > 0)
                {
                    background_music_audio_source.volume -= (1 / seconds_it_takes_to_fade_out) * Time.deltaTime;
                    yield return null;
                }
                background_music_audio_source.Stop();
            }
            yield return null;
#endif
        }
        // Fades out the current ambience over seconds_it_takes_to_fade_out
        public IEnumerator Fade_Out_Ambience(float seconds_it_takes_to_fade_out)
        {
#if ANSWERVERSE_FMOD
            if (ambience_emitter != null)
                ambience_emitter.EventInstance.stop(STOP_MODE.ALLOWFADEOUT);
            yield return null;
#else
            if (ambience_audio_source != null)
            {
                while (ambience_audio_source.volume > 0)
                {
                    ambience_audio_source.volume -= (1 / seconds_it_takes_to_fade_out) * Time.deltaTime;
                    yield return null;
                }
                ambience_audio_source.Stop();
            }
            yield return null;
#endif
        }


        // Starts the new background music and starts it playing (non-FMOD path)
        public void Set_Music(AudioClip new_music)
        {
#if !ANSWERVERSE_FMOD
            if (new_music == null || new_music == background_music_audio_source.clip)
                return;

            background_music_audio_source.clip = new_music;
            background_music_audio_source.loop = true;
            background_music_audio_source.volume = music_volume;
            background_music_audio_source.Play();
#endif
        }
#if ANSWERVERSE_FMOD
        // Starts the new background music by FMOD event path (FMOD path)
        public void Set_Music_FMOD(string event_path)
        {
            if (string.IsNullOrEmpty(event_path)) return;
            string full_path = $"event:/{event_path}";
            if (current_music_event_path == full_path) return; // Already playing

            Stop_Music(false);
            current_music_event_path = full_path;
            music_instance = RuntimeManager.CreateInstance(current_music_event_path);
            music_instance.setVolume(music_volume);
            music_instance.start();
        }

        private void Stop_Music(bool allow_fade_out)
        {
            if (music_instance.isValid())
            {
                music_instance.stop(allow_fade_out ? STOP_MODE.ALLOWFADEOUT : STOP_MODE.IMMEDIATE);
                music_instance.release();
            }
            current_music_event_path = "";
        }
#endif


        // Starts the new ambience and starts it playing
        public void Set_Ambience(AudioClip new_ambience)
        {
#if ANSWERVERSE_FMOD
            if (ambience_emitter != null)
                ambience_emitter.Play();
#else
            if (new_ambience == null) return;
            ambience_audio_source.clip = new_ambience;
            ambience_audio_source.loop = true;
            ambience_audio_source.volume = music_volume;
            ambience_audio_source.Play();
#endif
        }


        // Toggles the muting of ALL audio in the scene
        public void Toggle_Audio_Muting()
        {
            if (!muted)
                AudioListener.volume = 0;
            else
                AudioListener.volume = master_volume;

            muted = !muted;
        }


        // Voice-over always plays through voice_audio_source, regardless of ANSWERVERSE_FMOD -
        // voice lines are pre-baked clips, not FMOD-authored events
        public void Play_Voice_Clip(AudioClip voice_clip)
        {
            voice_audio_source.Stop();
            voice_audio_source.clip = voice_clip;
            voice_audio_source.Play();
        }


        // Creates a new gameobject with its own AudioSource that destroys itself after the sound is done playing (non-FMOD path)
        public void Play_Sound_Effect(AudioClip clip)
        {
#if !ANSWERVERSE_FMOD
            GameObject sound_obj = Instantiate(sound_effect_prefab) as GameObject;
            AudioSource audio = sound_obj.GetComponent<AudioSource>();
            audio.clip = clip;
            audio.playOnAwake = true;
            audio.volume = effects_volume;
            audio.Play();
#endif
        }
#if ANSWERVERSE_FMOD
        // Plays a one-shot SFX event by FMOD path (FMOD path). One-shots use PlayOneShot rather
        // than a persistent StudioEventEmitter so SFX gets its own channel instead of reusing ambience.
        public void Play_Sound_Effect_FMOD(string event_path)
        {
            if (string.IsNullOrEmpty(event_path)) return;

            var instance = RuntimeManager.CreateInstance($"event:/{event_path}");
            instance.start();
            instance.release();
        }
#endif


        // Called every time a character is printed to the screen, plays 1 instance of the given sound.
        // Always uses the pooled AudioSource list, in both backends, so beeps never restart voice-over.
        public void Play_Talking_Beep(AudioClip beep)
        {
            foreach (AudioSource aud in talking_beeps_list)
            {
                if (!aud.isPlaying)
                {
                    aud.clip = beep;
                    aud.Play();
                    return;
                }
            }
            Debug.Log("Could not find silent AudioSource for Play_Talking_Beep to use. Please add more AudioSources to the talking_beeps_list in AudioManager", this.gameObject);
        }


        // Pauses/resumes all audio channels in both backends
        public void Pause_Audio(bool on)
        {
            // Pauses every plain AudioSource in the scene: voice_audio_source, talking_beeps_list,
            // and (non-FMOD) ambience_audio_source/background_music_audio_source
            AudioListener.pause = on;

#if ANSWERVERSE_FMOD
            if (music_instance.isValid())
                music_instance.setPaused(on);
            if (ambience_emitter != null && ambience_emitter.EventInstance.isValid())
                ambience_emitter.EventInstance.setPaused(on);
#endif
        }


        // Volume options managed by the sliders in the pause menu
        public void Master_Volume_Changed(float new_volume)
        {
            master_volume = new_volume;

            // Change the audio listener's volume
            AudioListener.volume = master_volume;

            SavePlayerPreference("MasterVolume", new_volume);
        }
        public void Voice_Volume_Changed(float new_volume)
        {
            voice_volume = new_volume;
            if (voice_audio_source != null)
                voice_audio_source.volume = new_volume;

            // Change the talking beeps volume, as they count as voice volume
            foreach (AudioSource aud in talking_beeps_list)
            {
                aud.volume = voice_volume;
            }

            SavePlayerPreference("VoiceVolume", new_volume);
        }
        public void Music_Volume_Changed(float new_volume)
        {
            music_volume = new_volume;

#if ANSWERVERSE_FMOD
            if (music_instance.isValid())
                music_instance.setVolume(new_volume);
#else
            if (background_music_audio_source != null)
                background_music_audio_source.volume = new_volume;
            if (ambience_audio_source != null)
                ambience_audio_source.volume = new_volume;
#endif

            SavePlayerPreference("MusicVolume", new_volume);
        }
        public void Effects_Volume_Changed(float new_volume)
        {
            effects_volume = new_volume;
            SavePlayerPreference("EffectsVolume", new_volume);
        }


        // Player preferences live in Windows registries and persist between plays
        public void SavePlayerPreference(string key, bool value)
        {
            PlayerPrefs.SetInt(key, System.Convert.ToInt32(value));
            PlayerPrefs.Save();
        }
        public void SavePlayerPreference(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
            PlayerPrefs.Save();
        }
    }
}
