using UnityEngine;

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
            int coinValue = CoinValue(Coins, MaxCoins);
            int deathValue = DeathValue(Deaths);
            int timeValue = TimeValue(ElapsedTime);
            
            float average = (coinValue + deathValue + timeValue) / 3f;
            
            return Mathf.FloorToInt(average);
        }
        
        private static int TimeValue(float seconds)
        {
            return seconds switch
            {
                // less 2 mins
                < 120f => 5,
                // less 3 mins
                < 180f => 4,
                // less 4 mins
                < 240f => 3,
                // less 5 mins
                < 300f => 2,
                // 5 mins +
                _ => 1
            };
        }
        
        private static int CoinValue(int coins, int maxCoins)
        {
            if (maxCoins <= 0) return 1;

            float ratio = (float)coins / maxCoins;
            // round down to ensure 5 value is only achieved by getting all coins
            int value = Mathf.FloorToInt(ratio * 5f);

            return Mathf.Clamp(value, 1, 5);
        }
        
        private static int DeathValue(int deaths)
        {
            return deaths switch
            {
                // 2 or less
                <= 2 => 5,
                // 5 or less
                <= 5 => 4,
                // 8 or less
                <= 8 => 3,
                // 11 or less
                <= 11 => 2,
                // 12 or more
                _ => 1
            };
        }
    }
}