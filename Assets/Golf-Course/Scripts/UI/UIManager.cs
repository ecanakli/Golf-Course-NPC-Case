using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Golf_Course.Scripts.Managers;
using UnityEngine;

namespace Golf_Course.Scripts.UI
{
    public class UIManager : Singleton<UIManager>
    {
        [SerializeField]
        private List<UIPanel> uiPanels;

        private readonly Dictionary<string, UIPanel> _uiPanels = new();
        private readonly Dictionary<string, CancellationTokenSource> _cancellationTokenSources = new();
        private readonly Stack<string> _openPanels = new();

        public Action<string> OnPanelOpen;
        public Action<string> OnPanelClose;

        private void Awake()
        {
            foreach (var uiPanel in uiPanels)
            {
                if (_uiPanels.ContainsKey(uiPanel.GetType().Name))
                {
                    Debug.LogError($"There is already a panel of the type {uiPanel.GetType().Name}");

                    continue;
                }

                _uiPanels.Add(uiPanel.GetType().Name, uiPanel);
            }
        }

        public UIPanel GetUIPanel(string panelName)
        {
            if (!_uiPanels.ContainsKey(panelName))
            {
                throw new Exception($"Panel with name {panelName} not found");
            }

            return _uiPanels[panelName];
        }

        public void OpenUIPanel(string panelName)
        {
            if (!_uiPanels.ContainsKey(panelName))
            {
                Debug.LogError($"There are no panels with name {panelName}");

                return;
            }

            var panel = _uiPanels[panelName];

            if (_openPanels.Count > 0 && _openPanels.Peek() == panelName)
            {
                panel.PreOpen();
                panel.OnOpen();

                return;
            }

            _openPanels.Push(panelName);
            OnPanelOpen?.Invoke(panelName);

            if (_cancellationTokenSources.TryGetValue(panelName, out var cancellationTokenSource))
            {
                cancellationTokenSource.Cancel();
            }

            _cancellationTokenSources[panelName] = new CancellationTokenSource();

            panel.GetComponent<Canvas>().sortingOrder = _openPanels.Count * 2;
            panel.gameObject.SetActive(true);
            panel.PreOpen();

            switch (panel.PanelMode)
            {
                case UIPanelMode.Slide:
                    panel.Container.position = GetEndPosition(panel);
                    break;
                case UIPanelMode.Scale:
                    panel.Container.localScale = new Vector3(0, 0, 0);
                    break;
                case UIPanelMode.Appear:
                    panel.Container.position = GetEndPosition(panel);
                    break;
                case UIPanelMode.SlideAndFade:
                case UIPanelMode.Instant:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            AnimatePanelOpening(panel).Forget();
        }

        public void ClosePanel(string panelName)
        {
            if (_openPanels.Count == 0)
            {
                return;
            }

            var secondStack = new Stack<string>();

            while (true)
            {
                var latestPanel = _openPanels.Pop();

                if (latestPanel == panelName)
                {
                    while (secondStack.Count > 0)
                    {
                        _openPanels.Push(secondStack.Pop());
                    }

                    OnPanelClose?.Invoke(latestPanel);
                    CloseUIPanel(latestPanel);

                    break;
                }

                secondStack.Push(latestPanel);
            }
        }

        public void ClosePanel()
        {
            if (_openPanels.Count <= 0 || !_uiPanels[_openPanels.Peek()].CanBeClosed)
            {
                return;
            }

            var panel = _openPanels.Pop();
            OnPanelClose?.Invoke(panel);
            CloseUIPanel(panel);
        }

        private void CloseUIPanel(string panelName)
        {
            if (!_uiPanels.ContainsKey(panelName))
            {
                Debug.LogError($"There are no panels with name {panelName}");

                return;
            }

            if (_cancellationTokenSources.TryGetValue(panelName, out var cancellationTokenSource))
            {
                cancellationTokenSource.Cancel();
            }

            _cancellationTokenSources[panelName] = new CancellationTokenSource();

            var panel = _uiPanels[panelName];

            panel.PreClose();

            AnimatePanelClosing(panel).Forget();
        }

        private Vector3 GetInitialPosition(UIPanel uiPanel)
        {
            var width = uiPanel.Container.rect.width * uiPanel.GetComponent<RectTransform>().localScale.x;
            var height = uiPanel.Container.rect.height * uiPanel.GetComponent<RectTransform>().localScale.y;

            float posX;
            float posY;

            switch (uiPanel.AnchorMode)
            {
                case SlideMode.Top:
                    posX = uiPanel.Container.position.x;
                    posY = Screen.height - (height / 2f);
                    break;
                case SlideMode.Left:
                    posX = width / 2f;
                    posY = uiPanel.Container.position.y;
                    break;
                case SlideMode.Bottom:
                    posX = uiPanel.Container.position.x;
                    posY = height / 2f;
                    break;
                case SlideMode.Right:
                    posX = Screen.width - (width / 2f);
                    posY = uiPanel.Container.position.y;
                    break;
                case SlideMode.Mid:
                    posX = uiPanel.Container.position.x;
                    posY = 0f;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return new Vector3(posX, posY, 0);
        }

        private Vector3 GetEndPosition(UIPanel uiPanel)
        {
            var width = uiPanel.Container.rect.width * uiPanel.GetComponent<RectTransform>().localScale.x;
            var height = uiPanel.Container.rect.height * uiPanel.GetComponent<RectTransform>().localScale.y;

            float posX;
            float posY;

            switch (uiPanel.AnchorMode)
            {
                case SlideMode.Top:
                    posX = uiPanel.Container.position.x;
                    posY = Screen.height + (height / 2f);
                    break;
                case SlideMode.Left:
                    posX = -width / 2f;
                    posY = uiPanel.Container.position.y;
                    break;
                case SlideMode.Bottom:
                    posX = uiPanel.Container.position.x;
                    posY = -height / 2f;
                    break;
                case SlideMode.Right:
                    posX = Screen.width + (width / 2f);
                    posY = uiPanel.Container.position.y;
                    break;
                case SlideMode.Mid:
                    var position = uiPanel.Container.position;
                    posX = position.x;
                    posY = position.y - 20f;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return new Vector3(posX, posY, 0);
        }

        private async UniTask AnimatePanelOpening(UIPanel uiPanel)
        {
            await PlayPanelOpening(uiPanel);
            uiPanel.OnOpen();
        }

        private async UniTask PlayPanelOpening(UIPanel uiPanel)
        {
            switch (uiPanel.PanelMode)
            {
                case UIPanelMode.Scale:
                    await uiPanel.Container.DOScale(new Vector3(1, 1, 1), 0.3f).SetEase(Ease.InCubic)
                        .AsyncWaitForCompletion().AsUniTask()
                        .AttachExternalCancellation(_cancellationTokenSources[uiPanel.GetType().Name].Token);
                    break;
                case UIPanelMode.Slide:
                    await UniTask.DelayFrame(5);
                    await uiPanel.Container.DOMove(GetInitialPosition(uiPanel), 0.3f).SetEase(Ease.InCubic)
                        .AsyncWaitForCompletion().AsUniTask()
                        .AttachExternalCancellation(_cancellationTokenSources[uiPanel.GetType().Name].Token);
                    break;
                case UIPanelMode.Appear:
                    uiPanel.CanvasGroup.alpha = 0f;
                    await UniTask.DelayFrame(5);
                    uiPanel.CanvasGroup.DOFade(1f, 0.2f);
                    var position = uiPanel.Container.position;
                    await uiPanel.Container.DOMove(new Vector3(position.x, 20f, position.z), 0.1f)
                        .SetEase(Ease.InOutQuart)
                        .AsyncWaitForCompletion().AsUniTask()
                        .AttachExternalCancellation(_cancellationTokenSources[uiPanel.GetType().Name].Token);
                    await uiPanel.Container.DOMove(GetEndPosition(uiPanel), 0.1f).SetEase(Ease.InOutQuart)
                        .AsyncWaitForCompletion().AsUniTask()
                        .AttachExternalCancellation(_cancellationTokenSources[uiPanel.GetType().Name].Token);
                    break;

                case UIPanelMode.SlideAndFade:
                    await UniTask.DelayFrame(5);
                    await uiPanel.Container.DOMove(GetInitialPosition(uiPanel), 0.3f).SetEase(Ease.InCubic)
                        .OnPlay(() => uiPanel.CanvasGroup.DOFade(1, 0.2f))
                        .AsyncWaitForCompletion().AsUniTask()
                        .AttachExternalCancellation(_cancellationTokenSources[uiPanel.GetType().Name].Token);
                    break;
                case UIPanelMode.Instant:
                    break;
            }
        }

        private async UniTask AnimatePanelClosing(UIPanel uiPanel)
        {
            await PlayPanelClosing(uiPanel);
            uiPanel.OnClose();
            uiPanel.gameObject.SetActive(false);
        }

        private async UniTask PlayPanelClosing(UIPanel uiPanel)
        {
            switch (uiPanel.PanelMode)
            {
                case UIPanelMode.Scale:
                    await uiPanel.Container.DOScale(new Vector3(0, 0, 0), 0.3f).SetEase(Ease.InCubic)
                        .AsyncWaitForCompletion().AsUniTask()
                        .AttachExternalCancellation(_cancellationTokenSources[uiPanel.GetType().Name].Token);
                    break;
                case UIPanelMode.Slide:
                    await uiPanel.Container.DOMove(GetEndPosition(uiPanel), 0.3f).SetEase(Ease.InCubic)
                        .AsyncWaitForCompletion().AsUniTask()
                        .AttachExternalCancellation(_cancellationTokenSources[uiPanel.GetType().Name].Token);
                    break;
                case UIPanelMode.Appear:
                    uiPanel.CanvasGroup.alpha = 1f;
                    var position = uiPanel.Container.position;
                    await uiPanel.Container.DOMove(new Vector3(position.x, 20f, position.z), 0.1f)
                        .SetEase(Ease.InOutQuart)
                        .AsyncWaitForCompletion().AsUniTask()
                        .AttachExternalCancellation(_cancellationTokenSources[uiPanel.GetType().Name].Token);
                    uiPanel.CanvasGroup.DOFade(0f, 0.2f);
                    await uiPanel.Container.DOMove(GetEndPosition(uiPanel), 0.1f).SetEase(Ease.InOutQuart)
                        .AsyncWaitForCompletion().AsUniTask()
                        .AttachExternalCancellation(_cancellationTokenSources[uiPanel.GetType().Name].Token);
                    break;
                case UIPanelMode.SlideAndFade:
                    await UniTask.DelayFrame(5);
                    await uiPanel.Container.DOMove(GetInitialPosition(uiPanel), 0.3f).SetEase(Ease.InCubic)
                        .OnPlay(() => uiPanel.CanvasGroup.DOFade(0, 0.2f))
                        .AsyncWaitForCompletion().AsUniTask()
                        .AttachExternalCancellation(_cancellationTokenSources[uiPanel.GetType().Name].Token);
                    break;
                case UIPanelMode.Instant:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}