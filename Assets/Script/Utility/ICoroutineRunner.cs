using UnityEngine;
using System.Collections;

namespace Game.Utility
{
    public interface ICoroutineRunner
    {
        Coroutine RunCoroutine(IEnumerator routine);
    }
}