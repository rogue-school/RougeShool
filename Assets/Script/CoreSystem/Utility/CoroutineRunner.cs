using UnityEngine;
using System.Collections;

namespace Game.CoreSystem.Utility
{
    public class CoroutineRunner : MonoBehaviour, ICoroutineRunner
    {
        public Coroutine RunCoroutine(IEnumerator routine)
        {
            return StartCoroutine(routine);
        }
    }
}