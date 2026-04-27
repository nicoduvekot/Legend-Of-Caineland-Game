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

            // this retrieves the current saves list
            if (saveLoadSystem.GetDataService() != null)
            {
                _saveNames = saveLoadSystem
                    .GetSaveNames()
                    .ToArray();
            }
            
            DrawDefaultInspector();
            
            EditorGUILayout.Space();
            
            string activeSaveName = Application.isPlaying
                ? GameStateManager.Instance?.Data?.SaveName ?? "None"
                : "(Play Mode Only)";
            
            // displays the active save
            EditorGUILayout.LabelField("Active Save:", activeSaveName);
            
            EditorGUILayout.Space();

            if (Application.isPlaying)
            {
                // makes a new save as if new game was pressed
                if (GUILayout.Button("New Game"))
                {
                    saveLoadSystem.NewGame();
                }

                // saves the current save state as if save game was pressed
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
            
            // gives a dropdown menu to choose a save from the saves list, if there are saves
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

            // loads the selection as if load game was pressed
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

            // deletes the selection as if delete game was pressed
            if (GUILayout.Button("Delete Game") && _saveNames.Length > 0)
            {
                saveLoadSystem.DeleteGame(_saveNames[_selectedIndex]);
            }
        }
    }
}