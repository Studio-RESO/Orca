using System;
using App._001_OneShot.Scripts;
using App.Common.Scripts;
using UnityEngine;
using Object = UnityEngine.Object;

internal sealed class OneShotScene : BaseScene
{
    public OneShotScene()
    {
        GameMode = new OneShotGameMode(hudRoot);
    }
    
    private void Awake()
    {
        Debug.Log($"Player ID : {GameMode.PlayerState.Id}");
        Debug.Log($"Player Name : {GameMode.PlayerState.Name}");
    }
    
    protected override void Enter()
    {
        base.Enter();
    }
    
    protected override void Leave()
    {
        base.Leave();
    }
    
    protected override void ApplicationFocus()
    {
        base.ApplicationFocus();

    }
    
    protected override void ApplicationPause()
    {
        base.ApplicationPause();
    }
    
    protected override void ApplicationQuit()
    {
        base.ApplicationQuit();
    }
}
