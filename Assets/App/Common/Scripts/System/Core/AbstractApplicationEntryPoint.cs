using System;
using Orca.Example;
using UnityEngine;

namespace ContextSystem
{
    /// <summary>
    /// アプリケーションのエントリーポイントの抽象クラス
    /// </summary>
    [DefaultExecutionOrder(-Int32.MaxValue)]
    public abstract class AbstractApplicationEntryPoint : AbstractEntryPoint
    {
        public ApplicationContext ApplicationContext { get; protected set; }
    }
}
