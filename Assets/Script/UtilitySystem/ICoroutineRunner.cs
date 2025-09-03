using System.Collections;
using UnityEngine;

namespace Game.Utility
{
    public interface ICoroutineRunner
    {
        Coroutine RunCoroutine(IEnumerator routine);
    }
}