using System.Collections.Generic;
using System.Threading;
using AK.Wwise.Unity.WwiseAddressables;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Orca.Example
{
    public sealed class WwiseSampleScene : MonoBehaviour
    {
        [SerializeField] private GameObject initAudioListener;
        [SerializeField] private Button seButton;

        private AssetService assetService;
        private bool isInitialized;
        
        private readonly CancellationTokenSource cancellationTokenSource = new();

        private async void Awake()
        {
            // NOTE: Wwiseの初期化
            AkSoundEngine.Init(new AkInitializationSettings());
            
            // NOTE: これがないとWwise Profilerと接続できない
            AkSoundEngine.InitCommunication(new AkCommunicationSettings());
            
            // NOTE: 一応Unityのログ側でもモニタできる環境を作る
            AkCallbackManager.Init(new AkCallbackManager.InitializationSettings());
            AkCallbackManager.SetMonitoringCallback(AkMonitorErrorLevel.ErrorLevel_All,
                    (code, level, id, objID, msg) =>
                    {
                        Debug.LogWarning($"[Wwise Monitor] {level} {code} : {msg} --- id={id} objID={objID}");
                    });
            
            assetService = new AssetService("http://localhost:3000/", "catalog_0.1.0.json"); 
            isInitialized = await assetService.InitializeAsync(cancellationTokenSource.Token);
        }

        private async void Start()
        {
            seButton.onClick.AddListener(OnClickedSeButton);

            var taskList = new List<UniTask>
            {
                UniTask.WaitUntil(() => isInitialized),
                UniTask.WaitUntil(AkSoundEngine.IsInitialized)
            };
            await UniTask.WhenAll(taskList);
            
            Debug.Log("AkSoundEngine is initialized");
            Debug.Log(AkAddressableBankManager.Instance.WwiseMajorVersion);
            
            var listener = initAudioListener.GetComponent<AkAudioListener>()
                           ?? initAudioListener.AddComponent<AkAudioListener>();
            listener.gameObject.SetActive(true);
            
            // Init.bnkのAssetを取得
            var initBank =  await assetService.WwiseAssetLoader.AddressableSoundBank.LoadAssetAsync("init");
            if (initBank == null)
            {
                Debug.LogError("Failed to load init bank");
                return;
            }
            
            // InitBankHolderにInit.bnkをセット
            var initHolder = GetComponent<InitBankHolder>();
            initHolder.InitBank.AddressableBank = initBank;
            
            // Init.bnkをローぢ
            AkAddressableBankManager.Instance.LoadInitBank();
            
            // ここからSE用のサウンドバンクの読み込み
            var seBank =  await assetService.WwiseAssetLoader.AddressableSoundBank.LoadAssetAsync("se");
            if (seBank == null)
            {
                Debug.LogError("Failed to load se bank");
                return;
            }

            AkAddressableBankManager.Instance.LoadBank(seBank);
        }

        private void OnDestroy()
        {
            cancellationTokenSource.Cancel();

            if (initAudioListener != null)
            {
                Destroy(initAudioListener.GetComponent<AkAudioListener>());
            }
        }

        private void OnClickedSeButton()
        {
            var playingId = AkSoundEngine.PostEvent("Play_SE_Test_01", gameObject);
            if (playingId == AkSoundEngine.AK_INVALID_PLAYING_ID)
            {
                Debug.LogError("PostEvent failed!");
            }
            else
            {
                Debug.Log($"Posted OK, playingId={playingId}");
            }
        }
    }
}
