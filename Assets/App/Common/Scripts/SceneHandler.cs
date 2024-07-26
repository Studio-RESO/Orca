using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace App.Common.Scripts
{
    public interface ISceneHandler
    {
        SceneDefines CurrentSceneDef { get; }
        AbstractScene CurrentScene { get; }
        
        SceneDefines BeforeSceneDef { get; }
        AbstractScene BeforeScene { get; }
        
        void LoadScene(SceneDefines scene, params KeyValuePair<string, object>[] args);
    }
    
    internal sealed class SceneHandler : ISceneHandler
    {
        public SceneDefines CurrentSceneDef { get; private set; }
        public AbstractScene CurrentScene { get; private set; }
        
        public SceneDefines BeforeSceneDef { get; private set; }
        public AbstractScene BeforeScene { get; private set; }
        
        private readonly IApplicationProvider _provider;
        
        private bool IsLoading { get; set; }
        
        public SceneHandler(IApplicationProvider provider)
        {
            _provider = provider;
            IsLoading = false;
        }
        
        public async void LoadScene(SceneDefines sceneDef, params KeyValuePair<string, object>[] args)
        {
            Debug.Log($"Load Scene : start {sceneDef} load.");
            
            IsLoading = true;
            
            var added = SceneManager.LoadSceneAsync(sceneDef.ToString(), LoadSceneMode.Additive);
            if (added != null) await UniTask.WaitUntil(() => !added.isDone);
            
            await UnloadSceneAsync(CurrentSceneDef);
            
            var scene = GetAbstractScene(SceneManager.GetSceneByName(sceneDef.ToString()));
            if (scene == null)
            {
                Debug.LogError($"Load Scene : {sceneDef} not found.");
                
                IsLoading = false;
                
                return;
            }
            
            if (BeforeScene != null)
            {
                BeforeSceneDef = CurrentSceneDef;
                BeforeScene = CurrentScene;
            }
            
            CurrentScene = scene;
            CurrentSceneDef = sceneDef;
            
            await CurrentScene.Load();
            
            CurrentScene.Open();
            
            IsLoading = false;
        }
        
        public async UniTask UnloadSceneAsync(SceneDefines scene)
        {
            var unloaded = SceneManager.UnloadSceneAsync(scene.ToString());
            
            if (unloaded != null) await UniTask.WaitUntil(() => !unloaded.isDone);
            
            Debug.Log($"Unload Scene : {scene} unloaded.");
        }
        
        private AbstractScene GetAbstractScene(Scene scene)
        {
            if (scene == default)
            {
                return null;
            }
            
            var rootObjects = scene.GetRootGameObjects();
            for (var i = 0; i < rootObjects.Length; i++)
            {
                if (rootObjects[i].TryGetComponent<AbstractScene>(out var abstractScene))
                {
                    return abstractScene;
                }
            }
            
            return null;
        }
    }
}
