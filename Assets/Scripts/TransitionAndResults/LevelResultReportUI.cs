using UnityEngine;
using TMPro;
using GameState;

namespace TransitionAndResults
{
    public class LevelResultReportUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI headerLabel;
        [SerializeField] private TextMeshProUGUI timeLabel;
        [SerializeField] private TextMeshProUGUI coinsLabel;
        [SerializeField] private TextMeshProUGUI deathsLabel;
        [SerializeField] private TextMeshProUGUI scoreLabel;
        
        public void SetHeader(string text)
        {
            headerLabel.text = text;
        }
        
        public void Initialize(LevelData data)
        {
            timeLabel.text = $"Time : {FormatTime(data.ElapsedTime)}";
            coinsLabel.text = $"Coins : {data.Coins}/{data.MaxCoins}";
            deathsLabel.text = $"Deaths: {data.Deaths.ToString()}";
            scoreLabel.text = $"Total : {data.ComputeScore()} out of 5";
        }
        
        private static string FormatTime(float seconds)
        {
            int mins = (int)(seconds / 60f);
            int secs = (int)(seconds % 60f);
            return $"{mins:00}:{secs:00}";
        }
    }
}