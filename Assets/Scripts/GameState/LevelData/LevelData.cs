namespace GameState
{
    /// <summary>
    /// Represents the Data within a particular level.
    /// Time (WIP)
    /// Coins, the amount of coins collected within this level
    /// Deaths, the amount of deaths within this level
    /// </summary>
    public class LevelData
    {
        public readonly float Time;
        public readonly int Coins;
        public readonly int Deaths;

        public LevelData(float time, int coins, int deaths)
        {
            Time = time;
            Coins = coins;
            Deaths = deaths;
        }

        /// <summary>
        /// eventually to be used to calculate a score at the end of the game
        /// </summary>
        /// <param name="maxCoins"></param>
        /// <returns></returns>
        public float ComputeScore(int maxCoins)
        {
            float coinScore = (float)Coins / maxCoins;
            float deathScore = 1f / (1 + Deaths);
            float timeScore = 1f / (1 + Time);
            
            return coinScore + deathScore * timeScore;
        }
    }
}