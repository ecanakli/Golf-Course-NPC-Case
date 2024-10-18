using Golf_Course.Scripts.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Golf_Course.Scripts.UI.UIPanels
{
    public class SettingsPanel : UIPanel
    {
        [Header("Sliders")]
        [SerializeField]
        private Slider maxHealthSlider;

        [SerializeField]
        private Slider healthDecreaseRateSlider;

        [SerializeField]
        private Slider maxBallSlider;

        [Header("Texts")]
        [SerializeField]
        private TextMeshProUGUI maxHealthText;

        [SerializeField]
        private TextMeshProUGUI healthDecreaseRateText;

        [SerializeField]
        private TextMeshProUGUI maxBallText;

        [SerializeField]
        private Button applyButton;

        private void Start()
        {
            InitializeSliders();
            AddListeners();
        }

        private void InitializeSliders()
        {
            maxHealthSlider.minValue = 50f;
            maxHealthSlider.maxValue = 500f;

            healthDecreaseRateSlider.minValue = 0.1f;
            healthDecreaseRateSlider.maxValue = 5f;
            
            maxBallSlider.minValue = 10f;
            maxBallSlider.maxValue = 200f;
            
            if (NPCController.Instance != null)
            {
                maxHealthSlider.value = NPCController.Instance.MaxHealth;
                maxHealthText.text = NPCController.Instance.MaxHealth.ToString("F0");

                healthDecreaseRateSlider.value = NPCController.Instance.HealthDecreaseRate;
                healthDecreaseRateText.text = NPCController.Instance.HealthDecreaseRate.ToString("F1");
            }
            
            if (BallManager.Instance != null)
            {
                maxBallSlider.value = BallManager.Instance.MaxBalls;
                maxBallText.text = BallManager.Instance.MaxBalls.ToString("F0");
            }
        }

        private void AddListeners()
        {
            maxHealthSlider.onValueChanged.AddListener(OnMaxHealthSliderChanged);
            healthDecreaseRateSlider.onValueChanged.AddListener(OnHealthDecreaseRateSliderChanged);
            maxBallSlider.onValueChanged.AddListener(OnMaxBallSliderChanged);
            applyButton.onClick.AddListener(OnApplyButtonClicked);
        }

        private void OnMaxHealthSliderChanged(float value)
        {
            maxHealthText.text = value.ToString("F0");
        }

        private void OnHealthDecreaseRateSliderChanged(float value)
        {
            healthDecreaseRateText.text = value.ToString("F1");
        }

        private void OnMaxBallSliderChanged(float value)
        {
            maxBallText.text = value.ToString("F0");
        }

        private void OnApplyButtonClicked()
        {
            if (NPCController.Instance != null)
            {
                NPCController.Instance.MaxHealth = maxHealthSlider.value;
                NPCController.Instance.HealthDecreaseRate = healthDecreaseRateSlider.value;
            }

            if (BallManager.Instance != null)
            {
                BallManager.Instance.MaxBalls = (int) maxBallSlider.value;
            }
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
        }
    }
}