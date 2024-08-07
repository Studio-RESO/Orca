using System;
using Orca.Example;
using UnityEngine;

namespace ContextSystem
{
    [DefaultExecutionOrder(-Int32.MaxValue)]
    public abstract class AbstractApplicationEntryPoint : AbstractEntryPoint
    {
        public ApplicationContext ApplicationContext { get; protected set; }
        
        protected void OnDestroy()
        {
            ApplicationContext.Dispose();
        }
    }
}
