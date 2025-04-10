using System;
using System.Collections.Generic;
using AK.Wwise.Unity.WwiseAddressables;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public sealed class WwiseSampleScene : MonoBehaviour
{
    [SerializeField] private string[] soundBankNames;
    [SerializeField] private GameObject initAudioListener;
    [SerializeField] private Button seButton;
    
    private readonly Dictionary<string, uint> loadedSoundBanks = new ();

    private void Awake()
    {
        // var initializer = GetComponent<AkInitializer>() ?? gameObject.AddComponent<AkInitializer>();
        // initializer.InitializeInitializationSettings();

        var listener = initAudioListener.GetComponent<AkAudioListener>() ?? initAudioListener.AddComponent<AkAudioListener>();
        listener.gameObject.SetActive(true);
    }

    private async void Start()
    {
        Addressables.LoadResourceLocationsAsync("prefabs/cube").Completed += async handle =>
        {
            foreach (var loc in handle.Result)
            {
                Debug.Log($"-------------------------------------------------------");
                Debug.Log($"status = {handle.Status}");
                Debug.Log($"InternalId = {loc.InternalId}");
                
                var size = await Addressables.GetDownloadSizeAsync("prefabs/cube").ToUniTask();
                Debug.Log($"Need to download {size} bytes");
            }
        };
        
        LoadAllSoundBanks();
        
        seButton.onClick.AddListener(OnClickedSeButton);
    }
    
    private void OnDestroy()
    {
        UnloadAllSoundBanks();
        
        if (initAudioListener != null)
        {
            Destroy(initAudioListener.GetComponent<AkAudioListener>());
        }
        
        if (GetComponent<AkInitializer>() != null)
        {
            Destroy(GetComponent<AkInitializer>());
        }
    }

    /// <summary>
    /// 全てのサウンドバンクを読み込む
    /// </summary>
    public void LoadAllSoundBanks()
    {
        foreach (var bankName in soundBankNames)
        {
            LoadSoundBank(bankName);
        }
    }

    /// <summary>
    /// 特定のサウンドバンクを読み込む
    /// </summary>
    /// <param name="bankName">バンク名（拡張子なし、例："MainSoundBank"）</param>
    /// <returns>読み込み成功したかどうか</returns>
    private bool LoadSoundBank(string bankName)
    {
        if (string.IsNullOrEmpty(bankName))
        {
            Debug.LogError("SoundBank name is null or empty");
            return false;
        }

        // すでに読み込まれている場合はスキップ
        if (loadedSoundBanks.ContainsKey(bankName))
        {
            Debug.Log($"SoundBank '{bankName}' is already loaded");
            return true;
        }

        // サウンドバンクを読み込む
        var result = AkSoundEngine.LoadBank(bankName,  out var bankID);

        if (result == AKRESULT.AK_Success)
        {
            loadedSoundBanks.Add(bankName, bankID);
            Debug.Log($"Successfully loaded SoundBank: {bankName} (ID: {bankID})");
            return true;
        }
        
        Debug.LogError($"Failed to load SoundBank: {bankName}. Error code: {result}");
        return false;
    }

    /// <summary>
    /// 特定のサウンドバンクをアンロードする
    /// </summary>
    /// <param name="bankName">バンク名（拡張子なし）</param>
    /// <returns>アンロード成功したかどうか</returns>
    private bool UnloadSoundBank(string bankName)
    {
        if (loadedSoundBanks.TryGetValue(bankName, out var bankID))
        {
            var result = AkSoundEngine.UnloadBank(bankID, IntPtr.Zero);

            if (result == AKRESULT.AK_Success)
            {
                loadedSoundBanks.Remove(bankName);
                Debug.Log($"Successfully unloaded SoundBank: {bankName}");
                return true;
            }
            else
            {
                Debug.LogError($"Failed to unload SoundBank: {bankName}. Error code: {result}");
                return false;
            }
        }

        Debug.LogWarning($"SoundBank '{bankName}' was not loaded, so cannot unload");
        return false;
    }

    /// <summary>
    /// 全てのサウンドバンクをアンロードする
    /// </summary>
    public void UnloadAllSoundBanks()
    {
        // ディクショナリのキーのコピーを作成（反復処理中に削除するため）
        var bankNames = new string[loadedSoundBanks.Count];
        loadedSoundBanks.Keys.CopyTo(bankNames, 0);

        foreach (var bankName in bankNames)
        {
            UnloadSoundBank(bankName);
        }
    }

    private void OnClickedSeButton()
    {
        AkSoundEngine.PostEvent("Play_SE_Test_01", gameObject);
    }
}
