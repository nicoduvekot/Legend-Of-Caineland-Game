using System.Collections.Generic;
using GameState.Core;
using Utilities;

namespace CoinManagement
{
    /// <summary>
    /// This singleton handles tracking Coin Management. When a Coin is collected,
    /// it is added to a list of collected since last checkpoint,
    /// if they player should not reach another checkpoint prior to death,
    /// Coin Manager handles refreshing these coins, and removing them from data.
    /// Should the player reach a checkpoint, Coin Manager refreshes the list,
    /// keeping those coins in a permanently collected state.
    /// </summary>
    public class CoinManager : PersistentSingleton<CoinManager>
    {
        private readonly List<Coin> _collectedSinceCheckpoint = new();
        private int _coinTotalValueSinceCheckpoint;

        /// <summary>
        /// This to be called by the coin when it is collected
        /// </summary>
        /// <param name="coin"></param>
        public void CollectCoin(Coin coin)
        {
            GameStateManager.Instance.AddCoin(coin.Value);
            
            _collectedSinceCheckpoint.Add(coin);
            _coinTotalValueSinceCheckpoint += coin.Value;
            
            coin.Collect();
        }

        /// <summary>
        /// This is called by GameFlowManager when the coins are to be refreshed upon a death
        /// </summary>
        public void RespawnCoins()
        {
            GameStateManager.Instance.AddCoin(-_coinTotalValueSinceCheckpoint);
            
            foreach (Coin coin in _collectedSinceCheckpoint)
                coin.Respawn();
            
            _collectedSinceCheckpoint.Clear();
            _coinTotalValueSinceCheckpoint = 0;
        }

        /// <summary>
        /// This to be called by GameFlowManager, when a Checkpoint has been reached
        /// </summary>
        public void ReachedCheckpoint()
        {
            _collectedSinceCheckpoint.Clear();
            _coinTotalValueSinceCheckpoint = 0;
        }
    }
}