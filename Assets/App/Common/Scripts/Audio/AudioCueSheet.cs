using System;
using System.Collections.Generic;

[Serializable]
public sealed class AudioCueSheet
{
    /// <summary>
    /// CueSheet name.
    /// </summary>
    public string name;
    
    /// <summary>
    /// List of <see cref="AudioCue" />.
    /// </summary>
    public List<AudioCue> audioCueList = new();
}
