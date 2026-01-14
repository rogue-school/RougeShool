using System.Collections;
using UnityEngine;

namespace Game.CoreSystem.Utility
{
    public interface ICoroutineRunner
    {
        Coroutine RunCoroutine(IEnumerator routine);
    }
}