using App.Common.Scripts;
using UnityEngine;

namespace App._001_OneShot.Scripts
{
    public class OneShotGameMode : GameMode
    {
        public OneShotGameMode(Canvas hudRoot) : base(hudRoot)
        {
            GameState = new OneShotGameState();
        }
    }
}
