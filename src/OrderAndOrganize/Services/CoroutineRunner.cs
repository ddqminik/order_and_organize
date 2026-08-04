using System.Collections;
using UnityEngine;

namespace OrderAndOrganize.Services
{
    /// <summary>
    /// Provides a persistent MonoBehaviour for running coroutines from non-MonoBehaviour classes.
    /// Created once by Plugin and stays alive through scene changes.
    /// </summary>
    public class CoroutineRunner : MonoBehaviour
    {
        public static CoroutineRunner Instance { get; private set; }

        public static void Initialize()
        {
            if (Instance != null) return;
            var go = new GameObject("OAO_CoroutineRunner");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<CoroutineRunner>();
        }

        /// <summary>
        /// Starts a coroutine and returns it for yield-chaining.
        /// </summary>
        public Coroutine StartTrackedCoroutine(IEnumerator routine)
        {
            return StartCoroutine(routine);
        }
    }
}
