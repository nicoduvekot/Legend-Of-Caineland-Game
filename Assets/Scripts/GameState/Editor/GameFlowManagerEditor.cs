using UnityEditor;
using UnityEngine;

namespace GameState.Editor
{
    [CustomEditor(typeof(GameFlowManager))]
    public class GameFlowManagerEditor : UnityEditor.Editor
    {
        private string _saveName;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();
            
            GameFlowManager manager = (GameFlowManager)target;

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to use debug tools.", MessageType.Info);
                return;
            }
            
            if (GUILayout.Button("Start New Game"))
            {
                manager.StartNewGame();
            }
            
            _saveName = EditorGUILayout.TextField("Save Name", _saveName);

            if (GUILayout.Button("Load Game"))
            {
                manager.LoadGame(_saveName);
            }
            
            if (GUILayout.Button("Trigger Player Death"))
            {
                manager.OnPlayerDeath();
            }
        }
    }
}