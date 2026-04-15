using UnityEditor;
using UnityEngine;

namespace PlayerRespawnSystem.Editor
{
    /// <summary>
    /// An editor tool attached to the PlayerRespawnManager
    /// Calls the RespawnPlayer operation from the editor
    /// </summary>
    [CustomEditor(typeof(PlayerRespawnManager))]
    public class PlayerRespawnManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            EditorGUILayout.Space();
            
            PlayerRespawnManager manager = (PlayerRespawnManager)target;
            
            if (GUILayout.Button("Respawn Player"))
            {
                if (Application.isPlaying)
                {
                    manager.RespawnPlayer();
                }
                else
                {
                    Debug.LogWarning("RespawnPlayer can only be triggered in Play Mode.");
                }
            }
        }
    }
}