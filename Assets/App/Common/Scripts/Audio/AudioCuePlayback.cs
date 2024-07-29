using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioCuePlayback : MonoBehaviour
{
    private List<AudioUnit> _audioUnitList = new();
    private AudioSource _audioSource;
    
    public bool IsPlaying => _audioSource.isPlaying;
    
    protected void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }
    
    public async UniTask<bool> Setup(AudioCue cue, CancellationToken cancellationToken)
    {
        _audioUnitList = cue.audioUnits;
        
        var loader = new SoundAssetLoader();
        // TODO: Addressableを使用するか選択できるようにする。DI設計。
        var clip = await loader.LoadFromResourcesAsync(_audioUnitList[0].audioClipPath, cancellationToken);
        
        // メモリにAudioClipのデータをロード
        // TODO: ロード関係の処理は任意のタイミングで行えるようにする。シーン遷移時や、アプリケーション終了時など。
        clip.LoadAudioData();
        
        await UniTask.WaitUntil(() => clip.loadState != AudioDataLoadState.Loading, cancellationToken: cancellationToken);
        
        _audioSource.clip = clip;
        _audioSource.volume = _audioUnitList[0].volume;
        // TODO: pitchの設定を追加
        // _audioSource.pitch = _audioUnitList[0].pitch;
        _audioSource.loop = false;
        _audioSource.playOnAwake = false;
        _audioSource.outputAudioMixerGroup = cue.mixerGroup;
        
        return clip.loadState == AudioDataLoadState.Loaded;
    }
    
    public void Play()
    {
        // TODO: どのように再生するか形式を設定できるようにする。
        _audioSource.PlayScheduled(0);
    }
}
