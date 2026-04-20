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
        public  float ElapsedTime { get; internal set; }
        public int Coins { get; internal set; }
        public  int Deaths { get; internal set; }
        public  int MaxCoins { get; internal set; }

        public LevelData(float time, int coins, int deaths, int maxCoins)
        {
            ElapsedTime = time;
            Coins = coins;
            Deaths = deaths;
            MaxCoins = maxCoins;
        }

        /// <summary>
        /// eventually to be used to calculate a score at the end of the game
        /// </summary>
        /// <returns></returns>
        public float ComputeScore()
        {
            float coinScore = (float)Coins / MaxCoins;
            float deathScore = 1f / (1 + Deaths);
            float timeScore = 1f / (1 + ElapsedTime);
            
            return coinScore + deathScore * timeScore;
        }
    }
}