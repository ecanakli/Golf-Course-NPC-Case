using TMPro;
using UnityEngine;

namespace Golf_Course.Scripts.UI.UIPanels
{
    public class EndGamePanel : UIPanel
    {
        [SerializeField]
        private TextMeshProUGUI resultText;

        [SerializeField]
        private TextMeshProUGUI pointText;

        private int _currentPoint;

        public void ReturnToMainMenuOnClick()
        {
            ClosePanel();
            UIManager.Instance.OpenUIPanel("MainMenuPanel");
        }

        public void SetUI(int point, string result)
        {
            _currentPoint = point;
            UpdateUI(result);
        }

        private void UpdateUI(string result)
        {
            pointText.text = $"Points:<br>{_currentPoint}";
            resultText.text = result;
        }

        private void ResetUI()
        {
            _currentPoint = 0;
            resultText.text = string.Empty;
            pointText.text = $"Points:<br>{_currentPoint}";
        }

        public override void PreOpen()
        {
        }

        public override void OnOpen()
        {
        }

        public override void PreClose()
        {
        }

        public override void OnClose()
        {
            ResetUI();
        }
    }
}