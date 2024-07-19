using System;
using System.Collections;
using System.Collections.Generic;
using App.Common.Scripts;
using UnityEngine;

internal sealed class ApplicationManger : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private ApplicationConfiguration appConfig;
    
    private ApplicationOperator _appOperator;
    
    private void Awake()
    {
        DontDestroyOnLoad(this);
    }
    
    private void Start()
    {
        _appOperator = new ApplicationOperator(appConfig);
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
