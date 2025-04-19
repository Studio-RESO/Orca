using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Wwise
{
    public class RemoteSoundBankTest : MonoBehaviour
    {
        [SerializeField] private AkInitializer akInitializer;
        [SerializeField] private Button button;
        [SerializeField] private List<string> packageNames;

        private readonly S3DataLoader s3DataLoader = new S3DataLoader();

        private void Awake()
        {
            button.onClick.AddListener(OnClickButton);
        }

        private async UniTaskVoid Start()
        {
            var packageFetcher = new WwisePackageFetcher();
            
            AkSoundEngine.AddBasePath(packageFetcher.GetBasePath());
            
            var fetchList = packageNames.Select(packageFetcher.Fetch);
            await UniTask.WhenAll(fetchList);

            Debug.Log("All packages fetched.");
            
            var loadingList = packageNames.Select(x => LoadFilePackageAsync(x + ".pck"));
            await UniTask.WhenAll(loadingList);

            Debug.Log("All packages loaded.");

            await LoadBankAsync("SE");
            await LoadBankAsync("BGM");

            //var bankData = await s3DataLoader.LoadAsync("SE.bnk");

            //var seBank = await s3DataLoader.Download("SE.bnk");
            //await LoadBankBinaryAsync(seBank);
            //var bgmBank = await s3DataLoader.Download("BGM.bnk");
            //await LoadBankBinaryAsync(bgmBank);
            AkSoundEngine.PostEvent("Play_BGM_Test_01", gameObject);
        }

        public async UniTask LoadBankBinaryAsync(byte[] bankData)
        {
            var gcHandle = GCHandle.Alloc(bankData, GCHandleType.Pinned);
            var bankPtr = gcHandle.AddrOfPinnedObject();

            var (result, bankID, bankType) = await LoadBankMemoryCopyAsync(bankPtr, (uint)bankData.Length);

            if (result == AKRESULT.AK_Success)
            {
                Debug.Log($"Success: {bankID}, {bankType}");
            }
            else if (result == AKRESULT.AK_BankAlreadyLoaded)
            {
                Debug.LogError($"AlreadyLoaded: {bankID}, {bankType}");
            }
            else
            {
                Debug.LogError($"Failed: {bankID}, {bankType}");
            }

            gcHandle.Free();
        }

        private async UniTask<(AKRESULT result, uint bankID)> LoadBankAsync(string bank)
        {
            return await UniTask.RunOnThreadPool(() =>
            {
                var result = AkSoundEngine.LoadBank(bank, out var bankID);
                return (result, bankID);
            });
        }

        private async UniTask<(AKRESULT result, uint bankID, uint bankType)> LoadBankMemoryCopyAsync(IntPtr bankPtr, uint bankSize)
        {
            return await UniTask.RunOnThreadPool(() =>
            {
                var result = AkSoundEngine.LoadBankMemoryCopy(bankPtr, bankSize, out var bankID, out var bankType);
                return (result, bankID, bankType);
            });
        }

        private async UniTask<(AKRESULT result, uint packageID)> LoadFilePackageAsync(string packageName)
        {
            return await UniTask.RunOnThreadPool(() =>
            {
                var result = AkSoundEngine.LoadFilePackage(packageName, out var packageID);
                return (result, packageID);
            });
        }

        private void OnClickButton()
        {
            AkSoundEngine.PostEvent("Play_SE_Test_01", gameObject);
        }
    }
}
