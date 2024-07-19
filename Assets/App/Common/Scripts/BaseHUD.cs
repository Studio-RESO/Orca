using UnityEngine;

namespace App.Common.Scripts
{
    public abstract class BaseHUD
    {
        protected Canvas CanvasRoot { get; set; }
        
        public BaseHUD(Canvas canvasRoot)
        {
            CanvasRoot = canvasRoot;
        }
    }
}
