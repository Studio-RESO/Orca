using UnityEngine;

namespace App.Common.Scripts
{
    public abstract class AbstractHUD
    {
        protected Canvas CanvasRoot { get; set; }
        
        public AbstractHUD(Canvas canvasRoot)
        {
            CanvasRoot = canvasRoot;
        }
    }
}
