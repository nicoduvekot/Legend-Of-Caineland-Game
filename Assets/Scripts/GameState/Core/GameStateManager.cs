using UnityEngine;
using Utilities;

namespace GameState.Core
{
    /// <summary>
    /// Runtime State Management Layer
    ///
    /// Responsibilities:
    ///     Provide safe mutation operations for gameplay systems
    ///     Enforce some domain invariants (like maxHealth >= health >= 0)
    /// </summary>
    public class GameStateManager : PersistentSingleton<GameStateManager>
    {
        public GameData Data { get; private set; }
        
        protected override void Awake()
        {
            base.Awake();
        }
        
        public void SetActiveData(GameData data)
        {
            Data = data;
        }
        
        // GameState mutation operations
        
        public void AddCoin(int amount = 1)
        {
            Data.Coins += amount;
        }

        public void TakeDamage(int amount)
        {
            Data.PlayerHealth = Mathf.Max(0, Data.PlayerHealth - amount);
        }

        public void UnlockLevel(LevelId levelId)
        {
            Data.LevelsUnlocked.Add(levelId);
        }

        public void CompleteLevel(LevelId levelId)
        {
            Data.LevelsCompleted.Add(levelId);
        }

        public void SetCurrentLevel(LevelId levelId)
        {
            Data.CurrentLevel = levelId;
        }

        public void SetCurrentPlayer(PlayerId playerId)
        {
            Data.CurrentPlayer = playerId;
        }

        public void SetCheckpoint(int checkpointIndex)
        {
            Data.CurrentCheckpoint = checkpointIndex;
        }
    }
}