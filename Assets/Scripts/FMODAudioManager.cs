using System.Collections;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using Debug = FMOD.Debug;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class FMODAudioManager : MonoBehaviour
{
    public static FMODAudioManager Instance { get; private set; }
    private EventInstance bgMusic;
    private string currentEventPath = "";
    private AudioSource audioSource;
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public AudioSource GetAudioSource()
    {
        if (audioSource == null)
        {
            audioSource = Camera.main.gameObject.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                UnityEngine.Debug.LogError("No audio source found!");
            }
        }
        return audioSource;
    }
    /// <summary>
    /// Plays a one-shot event, optionally setting a parameter before playback.
    /// </summary>
    /// <param name="eventPath">FMOD event path (e.g., "SFX/Cheer")</param>
    /// <param name="paramName">Name of the parameter to set (optional)</param>
    /// <param name="paramValue">Value to set the parameter to (ignored if paramName is null or empty)</param>
    public void PlayOneShot(string eventPath, string paramName = null, float paramValue = 0f)
    {
        if (string.IsNullOrEmpty(eventPath)) return;

        var instance = RuntimeManager.CreateInstance($"event:/{eventPath}");

        if (!string.IsNullOrEmpty(paramName))
        {
            instance.setParameterByName(paramName, paramValue);
        }

        instance.start();
        instance.release(); // Automatically cleans up once finished
    }
    public IEnumerator PlayOneShotAndWaitPrecise(string eventPath, string paramName = null, float paramValue = 0f)
    {
        if (string.IsNullOrEmpty(eventPath)) yield break;

        var instance = RuntimeManager.CreateInstance($"event:/{eventPath}");
        if (!string.IsNullOrEmpty(paramName))
            instance.setParameterByName(paramName, paramValue);

        instance.start();
        instance.release();

        FMOD.Studio.PLAYBACK_STATE state;
        do
        {
            yield return null;
            instance.getPlaybackState(out state);
        }
        while (state != FMOD.Studio.PLAYBACK_STATE.STOPPED);
    }

    public void Pause(bool _on)
    {
        bgMusic.setPaused(_on);
    }
    public void PlayMusic(string eventPath)
    {
        if(eventPath == "") return;
        eventPath = ($"event:/{eventPath}");
        if (currentEventPath == eventPath) return; // Already playing
        StopMusic(); // stop existing music if any

        currentEventPath = eventPath;
        bgMusic = RuntimeManager.CreateInstance(currentEventPath);
        bgMusic.start();
    }

    public void StopMusic(bool allowFadeOut = true)
    {
        if (bgMusic.isValid())
        {
            bgMusic.stop(allowFadeOut ? STOP_MODE.ALLOWFADEOUT : STOP_MODE.IMMEDIATE);
            bgMusic.release();
        }

        currentEventPath = "";
    }

    public void SetMusicParameter(string paramName, float value)
    {
        if (bgMusic.isValid())
        {
            bgMusic.setParameterByName(paramName, value);
        }
    }

    public void SetMusicVolume(float volume)
    {
        if (bgMusic.isValid())
        {
            bgMusic.setVolume(volume);
        }
    }

    public bool IsPlaying()
    {
        if (!bgMusic.isValid()) return false;

        bgMusic.getPlaybackState(out PLAYBACK_STATE state);
        return state == PLAYBACK_STATE.PLAYING;
    }
    
    /// <summary>
    /// Plays a single sound effect (OneShot).
    /// </summary>
    /// <param name="clip">Clip to play</param>

}

