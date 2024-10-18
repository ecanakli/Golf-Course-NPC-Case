using Golf_Course.Scripts.Managers;
using UnityEngine.Device;

namespace Golf_Course.Scripts.UI.UIPanels
{
    public class MainMenuPanel : UIPanel
    {
        public void StartButtonOnClick()
        {
            ClosePanel();
            GameHandler.Instance.OnGameStarted?.Invoke();
        }
        
        public void SettingsButtonOnClick()
        {
            UIManager.Instance.OpenUIPanel("SettingsPanel");
        }

        public void ExitButtonOnClick()
        {
            Application.Quit();
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