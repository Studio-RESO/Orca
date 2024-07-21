using System;
using App._001_OneShot.Scripts;
using App.Common.Scripts;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

internal sealed class OneShotScene : AbstractScene
{
    public OneShotScene()
    {
        GameMode = new OneShotGameMode(hudRoot);
    }
    
    private void Awake()
    {
        // Debug.Log($"Player ID : {GameMode.PlayerState.Id}");
        // Debug.Log($"Player Name : {GameMode.PlayerState.Name}");
    }
    
    public override void Open()
    {
        base.Open();
    }
    
    public override UniTask<bool> Load()
    {
        throw new NotImplementedException();
    }
    
    public override UniTask Unload()
    {
        throw new NotImplementedException();
    }
}
