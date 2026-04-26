using GameState;
using GameState.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TransitionAndResults
{
    public class LevelCompletedUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI headerLabel;
        
        [SerializeField] private LevelResultReportUI oldResultPanel;
        [SerializeField] private LevelResultReportUI newResultPanel;
        
        [SerializeField] private Button chooseOldButton;
        [SerializeField] private Button chooseNewButton;

        [SerializeField] private Button replayButton;
        [SerializeField] private Button nextButton;
        
        private string _levelName;
        private LevelData _oldData;
        private LevelData _newData;
        
        private bool? _useNewResult;

        private void Start()
        {
            GameData data = GameStateManager.Instance.Data;
            
            _levelName = data.CurrentLevel;
            _newData = data.CurrentLevelData;
            
            headerLabel.text = $"Completed: {_levelName}";
            
            bool hasPrevResults = data.CompletedLevelData.TryGetValue(_levelName, out _oldData);
            
            if (!hasPrevResults)
            {
                SetupFirstTimeUI();
            }
            else
            {
                SetupComparisonUI();
            }
            
            replayButton.onClick.RemoveAllListeners();
            nextButton.onClick.RemoveAllListeners();
            
            replayButton.onClick.AddListener(OnReplayPressed);
            nextButton.onClick.AddListener(OnNextPressed);
        }
        
        private void SetupFirstTimeUI()
        {
            oldResultPanel.gameObject.SetActive(false);

            newResultPanel.gameObject.SetActive(true);
            newResultPanel.SetHeader($"{_levelName} Score");
            newResultPanel.Initialize(_newData);
            
            CenterPanel(newResultPanel);

            chooseOldButton.gameObject.SetActive(false);
            chooseNewButton.gameObject.SetActive(false);

            replayButton.gameObject.SetActive(true);
            nextButton.gameObject.SetActive(true);
            UpdateNextButtonText();
        }
        
        private void SetupComparisonUI()
        {
            oldResultPanel.gameObject.SetActive(true);
            newResultPanel.gameObject.SetActive(true);
            
            oldResultPanel.SetHeader("Old Results");
            newResultPanel.SetHeader("New Results");

            oldResultPanel.Initialize(_oldData);
            newResultPanel.Initialize(_newData);

            chooseOldButton.gameObject.SetActive(true);
            chooseNewButton.gameObject.SetActive(true);

            replayButton.gameObject.SetActive(false);
            nextButton.gameObject.SetActive(false);
            
            chooseOldButton.onClick.RemoveAllListeners();
            chooseNewButton.onClick.RemoveAllListeners();

            chooseOldButton.onClick.AddListener(() => ChooseResult(false));
            chooseNewButton.onClick.AddListener(() => ChooseResult(true));
        }
        
        private void ChooseResult(bool useNew)
        {
            _useNewResult = useNew;
            
            if (useNew)
                oldResultPanel.gameObject.SetActive(false);
            else
                newResultPanel.gameObject.SetActive(false);
            
            chooseOldButton.gameObject.SetActive(false);
            chooseNewButton.gameObject.SetActive(false);
            
            LevelResultReportUI chosen = useNew ? newResultPanel : oldResultPanel;
            chosen.SetHeader($"{_levelName} Score");
            CenterPanel(chosen);
            
            replayButton.gameObject.SetActive(true);
            nextButton.gameObject.SetActive(true);
            UpdateNextButtonText();
        }

        private void OnReplayPressed()
        {
            FinalizeData();
            GameFlowManager.Instance.NewLevel(_levelName);
        }

        private void OnNextPressed()
        {
            GameData data = GameStateManager.Instance.Data;

            if (data.BeatGame)
            {
                FinalizeData();
                GameFlowManager.Instance.CompleteGame();
                return;
            }

            if (!TryGetNextLevel(data.CurrentLevel, out string nextLevel))
            {
                Debug.LogError($"[Inquire with Nico] Current level '{data.CurrentLevel}' not found in levelOrder list, no next level to be found.");
                return;
            }

            // TryGetNextLevel was true at this point, null value means last level was completed
            if (nextLevel == null)
            {
                data.BeatGame = true;
                
                FinalizeData();
                GameFlowManager.Instance.CompleteGame();
                return;
            }

            // nextLevel has a value, meaning start that level
            FinalizeData();
            GameFlowManager.Instance.NewLevel(nextLevel);
        }

        private void FinalizeData()
        {
            GameData data = GameStateManager.Instance.Data;
            
            if (!data.CompletedLevelData.ContainsKey(_levelName))
            {
                GameFlowManager.Instance.FinalizeLevelResult(_levelName, _newData);
                return;
            }
            
            if (_useNewResult == false)
                return;

            GameFlowManager.Instance.FinalizeLevelResult(_levelName, _newData);
        }

        private static void CenterPanel(LevelResultReportUI panel)
        {
            RectTransform rt = panel.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
        }
        
        private void UpdateNextButtonText()
        {
            bool beatGame = GameStateManager.Instance.Data.BeatGame;

            TextMeshProUGUI label = nextButton.GetComponentInChildren<TextMeshProUGUI>();
            label.text = beatGame ? "Game Scores" : "Next Level";
        }

        private static bool TryGetNextLevel(string currentLevel, out string nextLevel)
        {
            string[] order = GameStateManager.Instance.levelOrder;

            int index = System.Array.IndexOf(order, currentLevel);
            if (index < 0)
            {
                // current level was not in the level order array
                nextLevel = null;
                return false;
            }

            if (index + 1 >= order.Length)
            {
                nextLevel = null;
                return true;
            }
            
            nextLevel = order[index + 1];
            return true;
        }
    }
}