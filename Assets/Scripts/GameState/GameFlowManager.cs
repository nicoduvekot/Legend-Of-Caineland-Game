using CoinManagement;
using GameState.Core;
using GameState.SaveLoad;
using PlayerRespawnSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cutscenes;
using Esper.Freeloader;
using PlayerMovementSystem;
using TimeSystem;

namespace GameState
{
    /// <summary>
    /// Central Flow Manager
    ///
    /// This is in place of events being published, as a particular sequence of events must happen.
    /// </summary>
    public class GameFlowManager : PersistentSingleton<GameFlowManager>
    {
        private bool _isTransitioning = false;
        private const string FirstLevelSceneName = "Level_01";
        private const float RespawnTime = 1f;

        // ------------  PUBLIC API OPERATION CALLS ------------

        /// <summary>
        /// Call this when a NewGame is started. GFM will handle operation sequence.
        /// </summary>
        public void StartNewGame()
        {
            StartCoroutine(NewGameFlow());
        }

        /// <summary>
        /// Call this when a LoadGame Action must occur. GFM will handle operation sequence.
        /// </summary>
        public void LoadGame(string saveName)
        {
            StartCoroutine(LoadGameFlow(saveName));
        }

        /// <summary>
        /// Call this when the player has died and should play a death animation
        /// </summary>
        public void OnPlayerDeath()
        {
            PlayerControlManager.Instance.FreezeInput();
            LevelTimer.Instance.StopTimer();
            PlayerControlManager.Instance.PlayDeathAnimation();
        }

        /// <summary>
        /// Call this when the player has died but needs to respawn immediately
        /// </summary>
        public void RespawnPlayer()
        {
            StartCoroutine(PlayerDeathFlow());
        }

        /// <summary>
        /// Call this when a level is completed
        /// </summary>
        public void CompleteLevel()
        {
            if (_isTransitioning) return;
            _isTransitioning = true;
            StartCoroutine(CompleteLevelFlow());
        }

        public void NewLevel(string levelName)
        {
            StartCoroutine(NewLevelFlow(levelName));
        }

        /// <summary>
        /// Call this when a checkpoint is reached. GFM will handle operation sequence.
        /// </summary>
        public static void OnCheckpointReached(int checkpointIndex, Checkpoint checkpointPosition)
        {
            GameStateManager.Instance.SetCheckpoint(checkpointIndex);

            PlayerRespawnManager.Instance.SetCheckpoint(checkpointPosition);

            CoinManager.Instance.ReachedCheckpoint();

            float elapsedTime = LevelTimer.Instance.GetElapsedTime();
            GameStateManager.Instance.SetCurrentLevelElapsedTime(elapsedTime);

            SaveLoadSystem.Instance.SaveGame();
        }

        public void FinalizeLevelResult(string levelName, LevelData chosenData)
        {
            GameStateManager.Instance.Data.CompletedLevelData[levelName] = chosenData;

            SaveLoadSystem.Instance.SaveGame();
        }


        // ------------ Private Flows ------------

        private static IEnumerator NewGameFlow()
        {
            // 1. Create new GameData
            SaveLoadSystem.Instance.NewGame();

            // 2. Load Level_01 via Freeloader
            LoadingScreen.Instance.Load(FirstLevelSceneName);

            // 3. Wait until the scene is fully active
            yield return new WaitUntil(() =>
                SceneManager.GetActiveScene().name == FirstLevelSceneName);

            // 4. Wait for Freeloader to fully close
            yield return new WaitUntil(() =>
                !LoadingScreen.Instance.IsLoading && !LoadingScreen.Instance.IsOpen);

            // 5. New Game means the level has started fresh
            yield return LevelStartFlow(FirstLevelSceneName);
        }

        private static IEnumerator LoadGameFlow(string saveName)
        {
            // 1. Load GameData into GameStateManager
            SaveLoadSystem.Instance.LoadGame(saveName);

            // 2. Extract current level from GameData
            string sceneToLoad = GameStateManager.Instance.CurrentLevel;

            if (string.IsNullOrEmpty(sceneToLoad))
            {
                Debug.LogWarning("Save has no CurrentLevel stored, defaulting to Level_01");
                sceneToLoad = FirstLevelSceneName;
            }

            // 3. Load the scene via Freeloader
            LoadingScreen.Instance.Load(sceneToLoad);

            // 4. Wait until the scene is fully active
            yield return new WaitUntil(() =>
                SceneManager.GetActiveScene().name == sceneToLoad);

            // 5. Wait for Freeloader to fully close
            yield return new WaitUntil(() =>
                !LoadingScreen.Instance.IsLoading && !LoadingScreen.Instance.IsOpen);

            // 6. Resume level from saved state
            yield return LevelStartFlow(sceneToLoad);
        }

        private static IEnumerator NewLevelFlow(string levelName)
        {
            GameData data = GameStateManager.Instance.Data;

            Instance._isTransitioning = false;

            // Reset runtime state for the new level
            data.CurrentLevel = levelName;
            data.CurrentLevelData = null;
            data.CurrentCheckpoint = 0;

            // Load via Freeloader
            LoadingScreen.Instance.Load(levelName);

            // Wait until the scene is fully active
            yield return new WaitUntil(() =>
                SceneManager.GetActiveScene().name == levelName);

            // Wait for Freeloader to fully close
            yield return new WaitUntil(() =>
                !LoadingScreen.Instance.IsLoading && !LoadingScreen.Instance.IsOpen);

            yield return LevelStartFlow(levelName);
        }

        private static IEnumerator PlayerDeathFlow()
        {
            // Opt: Lose control of player
            PlayerControlManager.Instance.FreezeInput();
            LevelTimer.Instance.StopTimer();

            // 1. Add a death to both level death counter and total deaths counter
            GameStateManager.Instance.AddDeath();

            // 2. Coin manager to respawn coins operation
            CoinManager.Instance.RespawnCoins();

            // 3. Reset Player Health
            GameStateManager.Instance.Data.PlayerHealth = 3;

            // 4. Save the game state
            SaveLoadSystem.Instance.SaveGame();

            // 5. Respawn the player
            PlayerRespawnManager.Instance.RespawnPlayer();

            // Opt: Regain control of player
            PlayerControlManager.Instance.UnfreezeInput();
            LevelTimer.Instance.StartTimer();

            yield break;
        }

        private static IEnumerator LevelStartFlow(string sceneName)
        {
            // 1. Freeze control of player
            PlayerControlManager.Instance.FreezeInput();
            LevelTimer.Instance.HideTimer();
            LevelTimer.Instance.StopTimer();

            // 2. Begin level data
            if (GameStateManager.Instance.Data.CurrentLevelData == null)
            {
                int maxCoins = FindObjectsByType<Coin>(FindObjectsSortMode.None).Length;

                GameStateManager.Instance.BeginLevelFresh(sceneName, maxCoins);
                LevelTimer.Instance.ResetTimer();
            }
            else
            {
                GameStateManager.Instance.ContinueLevelFromLoad(sceneName);
                float savedElapsedTime = GameStateManager.Instance.Data.CurrentLevelData.ElapsedTime;
                LevelTimer.Instance.SetElapsedSeconds(savedElapsedTime);
            }

            // 3. Detect cutscenes and wait till they finish
            List<ICutscene> cutscenes = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                .OfType<ICutscene>()
                .Where(c => c.IsPlaying)
                .ToList();

            if (cutscenes.Count > 0)
            {
                bool waiting = true;

                foreach (ICutscene cs in cutscenes)
                    cs.OnFinish += () => waiting = false;

                while (waiting)
                    yield return null;
            }

            // 4. Spawn player at checkpoint
            int currentCheckpointIndex = GameStateManager.Instance.CurrentCheckpoint;

            IOrderedEnumerable<Checkpoint> allCheckpoints = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None)
                .OrderBy(checkpoint => checkpoint.CheckpointIndex);

            foreach (Checkpoint checkpoint in allCheckpoints)
            {
                if (checkpoint.CheckpointIndex < currentCheckpointIndex)
                {
                    checkpoint.ForceMarkAsReached();
                }
                else if (checkpoint.CheckpointIndex == currentCheckpointIndex)
                {
                    checkpoint.ForceMarkAsReached();
                    PlayerRespawnManager.Instance.SetCheckpoint(checkpoint);
                }
            }

            CoinManager.Instance.ReachedCheckpoint();
            SaveLoadSystem.Instance.SaveGame();
            PlayerRespawnManager.Instance.RespawnPlayer();

            // 5. Enable player control + start timer
            PlayerControlManager.Instance.UnfreezeInput();
            LevelTimer.Instance.ShowTimer();
            LevelTimer.Instance.StartTimer();
        }

        private IEnumerator CompleteLevelFlow()
        {
            PlayerControlManager.Instance.FreezeInput();
            LevelTimer.Instance.StopTimer();

            CoinManager.Instance.ReachedCheckpoint();

            SaveLoadSystem.Instance.SaveGame();

            // Allow Freeloader's internal 0.5s startup delay to initialize IsLoading correctly
            yield return new WaitForSeconds(0.6f);

            // Now wait for it to fully finish loading
            yield return new WaitUntil(() =>
                !LoadingScreen.Instance.IsLoading && !LoadingScreen.Instance.IsOpen);

            // Force close in case screen is still open (e.g. requireInputToContinue)
            LoadingScreen.Instance.Close();

            // Wait for close animation to finish
            yield return new WaitUntil(() => !LoadingScreen.Instance.IsOpen);

            // Load the results screen
            LoadingScreen.Instance.Load("GameResults");
        }
    }
}