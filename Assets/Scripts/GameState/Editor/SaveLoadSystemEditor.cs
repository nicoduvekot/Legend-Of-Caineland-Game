using System;
using System.Linq;
using GameState.Core;
using GameState.SaveLoad;
using UnityEditor;
using UnityEngine;

namespace GameState.Editor
{
    [CustomEditor(typeof(SaveLoadSystem))]
    public class SaveLoadSystemEditor : UnityEditor.Editor
    {
        private int _selectedIndex;
        private string [] _saveNames = Array.Empty<string>();
        
        public override void OnInspectorGUI()
        {
            SaveLoadSystem saveLoadSystem = (SaveLoadSystem)target;

            if (saveLoadSystem.GetDataService() != null)
            {
                _saveNames = saveLoadSystem
                    .GetDataService()
                    .ListSaves()
                    .ToArray();
            }
            
            DrawDefaultInspector();
            
            EditorGUILayout.Space();
            
            string activeSaveName = Application.isPlaying
                ? GameStateManager.Instance?.Data?.SaveName ?? "None"
                : "(Play Mode Only)";
            
            EditorGUILayout.LabelField("Active Save:", activeSaveName);
            
            EditorGUILayout.Space();

            if (Application.isPlaying)
            {
                if (GUILayout.Button("New Game"))
                {
                    saveLoadSystem.NewGame();
                }

                if (GUILayout.Button("Save Game"))
                {
                    saveLoadSystem.SaveGame();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Start New Game and Save Game require Play Mode.", MessageType.Info);
            }
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Saves List:");
            if (_saveNames.Length > 0)
            {
                _selectedIndex = EditorGUILayout.Popup("Select Save", _selectedIndex, _saveNames);
            }
            else
            {
                EditorGUILayout.HelpBox("No saves found.", MessageType.Info);
            }
            
            EditorGUILayout.Space();

            if (GUILayout.Button("Load Game") && _saveNames.Length > 0)
            {
                if (Application.isPlaying)
                {
                    saveLoadSystem.LoadGame(_saveNames[_selectedIndex]);
                }
                else
                {
                    Debug.LogWarning("Load Game requires Play Mode.");
                }
            }

            if (GUILayout.Button("Delete Game") && _saveNames.Length > 0)
            {
                saveLoadSystem.DeleteGame(_saveNames[_selectedIndex]);
            }
        }
    }
}