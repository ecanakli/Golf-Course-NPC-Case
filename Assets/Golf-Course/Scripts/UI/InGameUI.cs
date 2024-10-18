using DG.Tweening;
using Golf_Course.Scripts.Managers;
using TMPro;
using UnityEngine;

namespace Golf_Course.Scripts.UI
{
    public class InGameUI : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI pointText;

        [SerializeField]
        private TextMeshProUGUI healthText;

        private int _currentPoint;

        private void OnEnable()
        {
            if (NPCController.Instance == null || GameHandler.Instance == null)
            {
                return;
            }
            
            NPCController.Instance.OnHealthChanged += UpdateHealthUI;
            NPCController.Instance.OnPointsEarned += UpdatePointUI;
            GameHandler.Instance.OnGameStarted += ResetUI;
        }

        private void OnDisable()
        {
            if (NPCController.Instance == null || GameHandler.Instance == null)
            {
                return;
            }
            
            NPCController.Instance.OnHealthChanged -= UpdateHealthUI;
            NPCController.Instance.OnPointsEarned -= UpdatePointUI;
            GameHandler.Instance.OnGameStarted -= ResetUI;
        }

        private void UpdateHealthUI(float currentHealth)
        {
            currentHealth = Mathf.Max(0, Mathf.FloorToInt(currentHealth));
            healthText.text = $"Health: {currentHealth}";
        }

        private void UpdatePointUI(int earnedPoints)
        {
            var targetPoint = _currentPoint + earnedPoints;
            IncreasePointsOverTime(_currentPoint, targetPoint, 0.3f);
            _currentPoint = targetPoint;
        }

        private void ResetUI()
        {
            _currentPoint = 0;
            pointText.text = $"Points: {_currentPoint}";
            healthText.text = "Health: ";
        }

        private void IncreasePointsOverTime(int startValue, int endValue, float duration)
        {
            DOVirtual.Int(startValue, endValue, duration, value => { pointText.text = $"Points: {value}"; })
                .OnComplete(() => { pointText.text = $"Points: {_currentPoint}"; });
        }
    }
}