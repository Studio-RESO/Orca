using System;
using System.Collections.Generic;
using UnityEngine.Audio;

[Serializable]
public sealed class AudioCue
{
    /// <summary>
    /// Cue name.
    /// </summary>
    public string name;
    
    /// <summary>
    /// 出力先の <see cref="AudioMixerGroup" />
    /// </summary>
    public AudioMixerGroup mixerGroup;
    
    /// <summary>
    /// 音量 (decibel)
    /// </summary>
    public float volume = 1.0f;
    
    /// <summary>
    /// List of <see cref="AudioUnit" />.
    /// </summary>
    public List<AudioUnit> audioUnits = new();
}
