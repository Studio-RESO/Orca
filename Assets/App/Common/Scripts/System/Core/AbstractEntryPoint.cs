using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ContextSystem
{
    public class AbstractEntryPoint : MonoBehaviour, IEntryPoint
    {
        protected IEnumerable<T> GetComponentsByType<T>(FindObjectsSortMode sortMode) where T : MonoBehaviour
        {
            T[] allObjects = FindObjectsByType<T>(sortMode);
            return allObjects.Select(e => e.GetComponent<T>());
        }
    }
}
