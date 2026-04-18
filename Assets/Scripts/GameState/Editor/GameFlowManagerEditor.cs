using UnityEditor;
using UnityEngine;

namespace GameState.Editor
{
    [CustomEditor(typeof(GameFlowManager))]
    public class GameFlowManagerEditor : UnityEditor.Editor
    {
        private string _sceneName;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            EditorGUILayout.Space();
            
            _sceneName = EditorGUILayout.TextField("Scene Name", _sceneName);
            
            if (Application.isPlaying)
            {
                if (GUILayout.Button("Trigger OnLevelStarted"))
                {
                    GameFlowManager manager = (GameFlowManager)target;
                    manager.SendMessage("OnLevelStarted", _sceneName);
                }
                
                if (GUILayout.Button("Trigger Player Death"))
                {
                    GameFlowManager manager = (GameFlowManager)target;
                    manager.SendMessage("OnPlayerDeath");
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Enter Play Mode to use debug tools.", MessageType.Info);
            }
        }
    }
}