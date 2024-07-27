using System;
using System.Collections;
using System.Collections.Generic;
using App.Common.Scripts;
using UnityEngine;

internal sealed class ApplicationManager : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private ApplicationConfiguration appConfig;
    
    private ApplicationOperator _appOperator;
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OnBeforeSceneLoadRuntimeMethod()
    {
        GameObject appManagerPrefab = Resources.Load<GameObject>("ApplicationManager");
        if (appManagerPrefab != null)
        {
            GameObject appManagerInstance = Instantiate(appManagerPrefab);
            DontDestroyOnLoad(appManagerInstance);
        }
        else
        {
            Debug.LogError("ApplicationManager prefab not found in Resources folder");
        }
        
    }
    
    private void Awake()
    {
        Debug.Log("ApplicationManager : Awake");
        _appOperator = new ApplicationOperator(appConfig);
        
    }
    
    private void Start()
    {
        
    }
    
    private void OnDestroy()
    {
        if (_appOperator != null) _appOperator.Dispose();
    }
    
    private void OnApplicationFocus(bool hasFocus)
    {
        
    }
    
    private void OnApplicationPause(bool pauseStatus)
    {
        
    }
    
    private void OnApplicationQuit()
    {
        
    }
}
