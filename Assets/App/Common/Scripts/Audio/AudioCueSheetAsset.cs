
using UnityEngine;

[CreateAssetMenu(fileName = "NewCueSheet",
    menuName = "Orca/" + nameof(AudioCueSheetAsset),
    order = 2)]
public class AudioCueSheetAsset : ScriptableObject
{
    /// <summary>
    /// <see cref="AudioCueSheet" />.
    /// </summary>
    public AudioCueSheet audioCueSheet = new();
}
