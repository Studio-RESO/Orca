using System;
using UnityEngine;
using UnityEngine.Audio;

[Serializable]
public sealed class AudioUnit
{
    /// <summary>
    /// Unit name.
    /// </summary>
    public string name;
    
    /// <summary>
    /// AudioCLip
    /// </summary>
    public string audioClipPath;
    
    /// <summary>
    /// 音量 (decibel)
    /// </summary>
    public float volume = 1.0f;
}
