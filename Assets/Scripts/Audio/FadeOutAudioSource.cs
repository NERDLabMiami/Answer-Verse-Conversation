using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeOutAudioSource : MonoBehaviour
{
    // Start is called before the first frame update
    private AudioSource cameraAudio;
    void Start()
    {
        AudioSource audioToFade = GetComponent<AudioSource>();
        cameraAudio = Camera.main.gameObject.AddComponent<AudioSource>();
        cameraAudio.clip = audioToFade.clip;
        cameraAudio.outputAudioMixerGroup = audioToFade.outputAudioMixerGroup;
        cameraAudio.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public AudioSource GetAudioSource()
    {
        return cameraAudio;
    }

}
