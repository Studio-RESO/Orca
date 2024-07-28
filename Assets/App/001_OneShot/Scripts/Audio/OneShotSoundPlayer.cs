using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;

public class OneShotSoundPlayer : MonoBehaviour
{
    [SerializeField] private AudioMixerGroup audioMixerGroup;
    
    private AudioSource _audioSource;
    private CancellationTokenSource _cancellationToken;
    
    protected void Awake()
    {
        _audioSource = gameObject.AddComponent<AudioSource>();
        _cancellationToken = new CancellationTokenSource();
    }
    
    protected void OnDestroy()
    {
        _cancellationToken.Cancel();
        _cancellationToken.Dispose();
    }
    
    public void Play(AudioClip clip)
    {
        Debug.Log(_audioSource);
        _audioSource.clip = clip;
        _audioSource.outputAudioMixerGroup = audioMixerGroup;
        _audioSource.PlayOneShot(clip);
    }
}
