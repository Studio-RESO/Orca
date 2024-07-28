using UnityEngine;
using UnityEngine.UI;

namespace App._001_OneShot.Scripts
{
    public class OneShotPlayButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        
        private OneShotSoundPlayer _player;
        private AudioClip _clip;
        
        protected async void Awake()
        {
            _player = gameObject.AddComponent<OneShotSoundPlayer>();
            
            var loader = new SoundAssetLoader();
            _clip = await loader.LoadFromResourcesAsync("Sound/SE_01");
            
            Debug.Log(_clip.loadState);
            
            button.onClick.AddListener(OnClick);
        }
        
        protected void OnDestroy()
        {
            button.onClick.RemoveListener(OnClick);
        }
        
        private void OnClick()
        {
            Debug.Log(_clip.loadState);
            if (_clip) _player.Play(_clip);
        }
    }
}
