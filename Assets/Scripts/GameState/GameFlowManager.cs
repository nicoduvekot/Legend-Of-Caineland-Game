using CoinManagement;
using GameState.Core;
using GameState.SaveLoad;
using PlayerRespawnSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities;

namespace GameState
{
    /// <summary>
    /// Central Flow Manager
    ///
    /// This is in place of events being published, as a particular sequence of events must happen.
    /// </summary>
    public class GameFlowManager : PersistentSingleton<GameFlowManager>
    {
        protected override void Awake()
        {
            base.Awake();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        /// <summary>
        /// This is called when SceneManager triggers the event for a SceneLoading event.
        /// It skips any scenes that are not a level
        /// GameFlowManager then initiates the level data start up sequence for the level
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="sceneMode"></param>
        private void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
        {
            if (!scene.name.StartsWith("Level"))
                return;
            
            OnLevelStarted(scene.name);
        }

        private void OnLevelStarted(string sceneName)
        {
            // counts the total number of coin objects in the scene at the start of the scene
            int maxCoins = FindObjectsByType<Coin>(FindObjectsSortMode.None).Length;
            
            GameStateManager.Instance.BeginLevel(sceneName, maxCoins);
        }

        /// <summary>
        /// Call This when a checkpoint is reached,
        /// GameFlowManager handles the operation order that must occur
        /// </summary>
        /// <param name="checkpointIndex"></param>
        /// <param name="checkpointPosition"></param>
        public static void OnCheckpointReached(int checkpointIndex, Vector3 checkpointPosition)
        {
            GameStateManager.Instance.SetCheckpoint(checkpointIndex);
            
            PlayerRespawnManager.Instance.SetCheckpoint(checkpointPosition);
            
            CoinManager.Instance.ReachedCheckpoint();
            
            SaveLoadSystem.Instance.SaveGame();
        }

        /// <summary>
        /// Call this when the player has died: FlowManager handles the operation order that must occur to keep data safe
        /// </summary>
        public void OnPlayerDeath()
        {
            GameStateManager.Instance.AddLevelDeath();
            GameStateManager.Instance.AddDeath();
            
            CoinManager.Instance.RespawnCoins();
            
            PlayerRespawnManager.Instance.RespawnPlayer();
            
            SaveLoadSystem.Instance.SaveGame();
        }
    }
}