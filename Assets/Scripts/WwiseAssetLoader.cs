using AK.Wwise.Unity.WwiseAddressables;
using Asteria;
using UnityEngine;

namespace Orca.Example
{
    public class WwiseAssetLoader
    {
        public IAssetLoader<WwiseAddressableSoundBank> AddressableSoundBank
        {
            get;
        }

        public IAssetLoader<GameObject> AkEvent
        {
            get;
        }
        
        public WwiseAssetLoader(IAddressableLoader addressableLoader)
        {
            AddressableSoundBank = CreateLoader<WwiseAddressableSoundBank>(addressableLoader, "audio/wwise/soundbanks");
            AkEvent = CreateLoader<GameObject>(addressableLoader, "prefabs");
        }
        
        private IAssetLoader<T> CreateLoader<T>(IAddressableLoader addressableLoader, string prefix) where T : Object
        {
            return new AssetLoader<T>(addressableLoader, prefix);
        }
    }
}
