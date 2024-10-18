using System;
using Golf_Course.Scripts.UI;
using Golf_Course.Scripts.UI.UIPanels;

namespace Golf_Course.Scripts.Managers
{
    public class GameHandler : Singleton<GameHandler>
    {
        public Action OnGameStarted;
        private int _currentScore;

        private void Start()
        {
            StartGame();
        }
        
        private void OnEnable()
        {
            if (NPCController.Instance == null)
            {
                return;
            }

            NPCController.Instance.OnSuccessful += OnGameSuccess;
            NPCController.Instance.OnFail += OnGameFail;
            NPCController.Instance.OnPointsEarned += UpdateScore;
        }
        
        private void OnDisable()
        {
            if (NPCController.Instance == null)
            {
                return;
            }

            NPCController.Instance.OnSuccessful -= OnGameSuccess;
            NPCController.Instance.OnFail -= OnGameFail;
            NPCController.Instance.OnPointsEarned -= UpdateScore;
        }

        private void StartGame()
        {
            _currentScore = 0;
            UIManager.Instance.OpenUIPanel("MainMenuPanel");
        }
        
        private void UpdateScore(int points)
        {
            _currentScore += points;
        }

        private void OnGameFail()
        {
            OpenEndGamePanel("Game Over");
        }

        private void OnGameSuccess()
        {
            OpenEndGamePanel("Congratulations");
        }

        private void OpenEndGamePanel(string message)
        {
            if (UIManager.Instance.GetUIPanel("EndGamePanel") is not EndGamePanel endGamePanel)
            {
                return;
            }

            endGamePanel.SetUI(_currentScore, message);
            UIManager.Instance.OpenUIPanel("EndGamePanel");
            _currentScore = 0;
        }
    }
}