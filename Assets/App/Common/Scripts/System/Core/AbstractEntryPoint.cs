using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ContextSystem
{
    /// <summary>
    /// エントリーポイントの抽象クラス
    /// </summary>
    public abstract class AbstractEntryPoint : MonoBehaviour, IEntryPoint
    {
        /// <summary>
        /// すべてのゲームオブジェクトから指定のコンポーネントをアタッチしたものだけを取得する
        /// </summary>
        /// <param name="sortMode">取得したものをどのようにソートするか</param>
        /// <typeparam name="T">コンポーネントの型</typeparam>
        /// <returns>取得した複数のゲームオブジェクト</returns>
        protected IEnumerable<T> GetComponentsByType<T>(FindObjectsSortMode sortMode) where T : MonoBehaviour
        {
            T[] allObjects = FindObjectsByType<T>(sortMode);
            return allObjects.Select(e => e.GetComponent<T>());
        }
    }
}
