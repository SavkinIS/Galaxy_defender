using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Core.Ad;

namespace Core
{
    public class ADController : MonoBehaviour
    {
        private sealed class AdRequest
        {
            public AdOfferPopup Popup;
            public string Message;
            public Action Granted;
            public Action Declined;
        }

        private const int PopupSortingOrder = 32760;
        private const string DefaultMessage = "Посмотреть рекламу?";

        public static ADController Instance { get; private set; }

        public bool IsShowing => _isShowing;

        private readonly Queue<AdRequest> _requests = new Queue<AdRequest>();
        private GameObject _popupRoot;
        private bool _isShowing;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void ShowRewardedAd(Action onRewardGranted, Action onDeclined = null)
        {
            ShowRewardedAd(null, DefaultMessage, onRewardGranted, onDeclined);
        }

        public void ShowRewardedAd(string message, Action onRewardGranted, Action onDeclined = null)
        {
            ShowRewardedAd(null, message, onRewardGranted, onDeclined);
        }

        public void ShowRewardedAd(AdOfferPopup popup, string message, Action onRewardGranted,
            Action onDeclined = null)
        {
            _requests.Enqueue(new AdRequest
            {
                Popup = popup,
                Message = message,
                Granted = onRewardGranted,
                Declined = onDeclined
            });

            if (!_isShowing)
                ShowNext();
        }

        private void ShowNext()
        {
            if (_requests.Count == 0)
            {
                _isShowing = false;
                return;
            }

            _isShowing = true;
            var request = _requests.Dequeue();
            if (request.Popup != null && request.Popup.IsConfigured)
            {
                request.Popup.Present(request.Message,
                    () => Resolve(request, true),
                    () => Resolve(request, false));
                return;
            }

            request.Declined?.Invoke();
            ShowNext();
        }

        private void BuildPopup(AdRequest request)
        {
            ClosePopup();

            var canvasObject = new GameObject("ADPopup", typeof(Canvas), typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            
            canvasObject.transform.SetParent(transform, false);

            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = PopupSortingOrder;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            EnsureEventSystem();

            var background = CreateUiObject("Background", canvasObject.transform);
            Stretch(background);
            var backgroundImage = background.AddComponent<Image>();
            backgroundImage.color = new Color(0f, 0f, 0f, 0.7f);

            var panel = CreateUiObject("Panel", canvasObject.transform);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(640f, 320f);
            var panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0.13f, 0.14f, 0.2f, 1f);

            var message = CreateUiObject("Message", panel.transform);
            var messageRect = message.GetComponent<RectTransform>();
            messageRect.anchorMin = new Vector2(0f, 0.35f);
            messageRect.anchorMax = new Vector2(1f, 0.9f);
            messageRect.offsetMin = new Vector2(40f, 0f);
            messageRect.offsetMax = new Vector2(-40f, 0f);
            var messageText = message.AddComponent<TextMeshProUGUI>();
            messageText.text = string.IsNullOrEmpty(request.Message) ? DefaultMessage : request.Message;
            messageText.fontSize = 32f;
            messageText.alignment = TextAlignmentOptions.Center;
            messageText.color = Color.white;
            messageText.enableWordWrapping = true;

            CreateButton(panel.transform, "AgreeButton", "Согласен",
                new Color(0.25f, 0.6f, 0.3f), new Vector2(-140f, 0f), () => Resolve(request, true));
            CreateButton(panel.transform, "DeclineButton", "Нет",
                new Color(0.6f, 0.25f, 0.25f), new Vector2(140f, 0f), () => Resolve(request, false));

            _popupRoot = canvasObject;
        }

        private void Resolve(AdRequest request, bool granted)
        {
            if (request.Popup != null && request.Popup.IsConfigured)
                request.Popup.Dismiss();
            else
                ClosePopup();

            if (granted)
                request.Granted?.Invoke();
            else
                request.Declined?.Invoke();

            ShowNext();
        }

        private void ClosePopup()
        {
            if (_popupRoot == null)
                return;

            Destroy(_popupRoot);
            _popupRoot = null;
        }

        private void CreateButton(Transform parent, string objectName, string label, Color color,
            Vector2 anchoredPosition, Action onClick)
        {
            var buttonObject = CreateUiObject(objectName, parent);
            var buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.15f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.15f);
            buttonRect.pivot = new Vector2(0.5f, 0.5f);
            buttonRect.sizeDelta = new Vector2(260f, 90f);
            buttonRect.anchoredPosition = anchoredPosition;

            var image = buttonObject.AddComponent<Image>();
            image.color = color;

            var button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => onClick?.Invoke());

            var labelObject = CreateUiObject("Label", buttonObject.transform);
            Stretch(labelObject);
            var labelText = labelObject.AddComponent<TextMeshProUGUI>();
            labelText.text = label;
            labelText.fontSize = 28f;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.color = Color.white;
        }

        private static GameObject CreateUiObject(string objectName, Transform parent)
        {
            var uiObject = new GameObject(objectName, typeof(RectTransform));
            uiObject.transform.SetParent(parent, false);
            return uiObject;
        }

        private static void Stretch(GameObject uiObject)
        {
            var rect = uiObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null)
                return;

            var eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }
    }
}
