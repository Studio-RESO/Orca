using App._001_OneShot.Scripts;
using UnityEngine;

namespace App.Common.Scripts
{
    public abstract class GameMode
    {
        public GameState GameState { get; internal set; }
        public PlayerController PlayerController { get; internal set; }
        public PlayerState PlayerState { get; internal set; }
        
        public BaseHUD Hud { get; internal set; }
        
        protected GameMode(Canvas hudRoot)
        {}
    }
}
