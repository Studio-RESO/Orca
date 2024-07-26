using App._001_OneShot.Scripts;
using UnityEngine;

namespace App.Common.Scripts
{
    /// <summary>
    /// GameMode
    /// 
    /// </summary>
    public abstract class GameMode
    {
        public GameState GameState { get; internal set; }
        
        protected GameMode(Canvas hudRoot)
        {}
    }
}
