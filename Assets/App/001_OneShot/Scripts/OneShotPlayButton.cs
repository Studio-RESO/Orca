using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace App._001_OneShot.Scripts
{
    public class OneShotPlayButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private AudioCueSheetAsset audioCueSheetAsset;
        [SerializeField] private AudioCuePlayback audioCuePlayback;
        
        // private OneShotSoundPlayer _player;
        // private AudioClip _clip;
        
        private CancellationToken _cancellationToken;
        
        protected async void Awake()
        {
            _cancellationToken = new CancellationToken();
            
            // _player = gameObject.AddComponent<OneShotSoundPlayer>();
            //
            // var loader = new SoundAssetLoader();
            // _clip = await loader.LoadFromResourcesAsync("Sound/BGM_01");
            
            button.onClick.AddListener(OnClick);
        }
        
        protected async void Start()
        {
            bool isLoaded = await audioCuePlayback.Setup(
                audioCueSheetAsset.audioCueSheet.audioCueList[0],
                _cancellationToken);
            
            if (isLoaded)
            {
                Debug.Log("AudioCue loaded.");
            }
            else
            {
                Debug.LogError("Failed to load AudioCue.");
            }
            
        }
        
        protected void OnDestroy()
        {
            button.onClick.RemoveListener(OnClick);
        }
        
        private void OnClick()
        {
            // Debug.Log(_clip.loadState);
            // if (_clip) _player.Play(_clip);
            
            audioCuePlayback.Play();
        }
    }
}
