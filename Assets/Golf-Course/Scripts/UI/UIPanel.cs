using UnityEngine;

namespace Golf_Course.Scripts.UI
{
    public abstract class UIPanel : MonoBehaviour
    {
        [SerializeField]
        private UIPanelMode panelMode;

        public UIPanelMode PanelMode => panelMode;

        [SerializeField]
        private SlideMode anchorMode;

        public SlideMode AnchorMode => anchorMode;

        [SerializeField]
        private RectTransform container;

        public RectTransform Container => container;

        [SerializeField]
        private CanvasGroup canvasGroup;

        public CanvasGroup CanvasGroup => canvasGroup;

        public bool CanBeClosed { get; set; } = true;

        public void ClosePanel()
        {
            UIManager.Instance.ClosePanel();
        }

        public abstract void PreOpen();
        public abstract void OnOpen();

        public abstract void PreClose();
        public abstract void OnClose();
    }
}