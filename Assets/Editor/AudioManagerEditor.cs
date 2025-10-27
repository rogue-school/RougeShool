using UnityEngine;
using UnityEditor;
using Game.CoreSystem.Audio;
using Game.StageSystem.Data;

namespace Game.Editor
{
    /// <summary>
    /// AudioManager용 커스텀 Inspector
    /// StageData의 Enemy Audios를 인라인으로 표시하고 편집 가능하게 함
    /// </summary>
    [CustomEditor(typeof(AudioManager))]
    public class AudioManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            AudioManager audioManager = (AudioManager)target;
            
        }
    }
}

