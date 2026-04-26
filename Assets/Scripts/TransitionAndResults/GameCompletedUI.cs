using Esper.Freeloader;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GameState;
using GameState.Core;

namespace TransitionAndResults
{
    public class GameCompletedUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI headerLabel;
        [SerializeField] private TextMeshProUGUI overallScoreLabel;
        
        [SerializeField] private LevelResultReportUI[] levelResultPanels;
        [SerializeField] private Button[] replayButtons;
        
        [SerializeField] private Button returnToMenuButton;

        private void Start()
        {
            GameData data = GameStateManager.Instance.Data;
            
            headerLabel.text = "Completed: The Game!";
            
            if (data.CompletedLevelData.Count != 4)
            {
                Debug.LogError("[GameCompletedUI] Expected 4 completed levels, found " +
                               data.CompletedLevelData.Count);
                return;
            }
            
            float totalScore = 0f;
            
            for (int i = 0; i < levelResultPanels.Length; i++)
            {
                string levelName = GameStateManager.Instance.levelOrder[i];

                if (!data.CompletedLevelData.TryGetValue(levelName, out LevelData levelData))
                {
                    Debug.LogError($"[GameCompletedUI] Missing LevelData for {levelName}");
                    continue;
                }

                levelResultPanels[i].SetHeader($"{levelName} Score");
                levelResultPanels[i].Initialize(levelData);
                
                replayButtons[i].onClick.RemoveAllListeners();
                replayButtons[i].onClick.AddListener(() =>
                {
                    Debug.Log($"Replay {levelName} pressed");
                    GameFlowManager.Instance.NewLevel(levelName);
                });

                totalScore += levelData.ComputeScore();
            }
            
            overallScoreLabel.text = $"Overall Score: {totalScore:0}";
            
            returnToMenuButton.onClick.RemoveAllListeners();
            returnToMenuButton.onClick.AddListener(() =>
            {
                Debug.Log("Return to menu pressed");
                LoadingScreen.Instance.Load("menu");
            });
        }
    }
}