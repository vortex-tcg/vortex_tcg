using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;

namespace VortexTCG.Scripts.Features.UI
{
    public class ButtonPressedBackgroundUI : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private string buttonName = "QuitButton";
        [SerializeField] private string[] buttonNames;
        [SerializeField] private float pressedVisibleDuration = 0.08f;
        [SerializeField] private bool debugLogs;

        [Header("Backgrounds")]
        [SerializeField] private Texture2D normalBackground;
        [SerializeField] private Texture2D pressedBackground;

        private VisualElement root;
        private readonly Dictionary<Button, BoundButtonState> boundButtons = new();
        private IVisualElementScheduledItem bindRetryTask;

        private sealed class BoundButtonState
        {
            public Button Button;
            public StyleBackground InitialBackground;
            public bool HasInitialBackground;
            public Action ClickHandler;
        }

        private void OnEnable()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();

            if (uiDocument == null)
            {
                Debug.LogError("[ButtonPressedBackgroundUI] UIDocument is missing.");
                return;
            }

            root = uiDocument.rootVisualElement;
            TryBindButton();

            if (boundButtons.Count == 0 && root != null)
            {
                // UXML can be created a few frames later in some setups.
                bindRetryTask = root.schedule.Execute(TryBindButton).Every(100);
            }
        }

        private void TryBindButton()
        {
            if (root == null)
                return;

            bool anyBound = false;
            bool allBound = true;

            foreach (string targetName in GetTargetButtonNames())
            {
                if (string.IsNullOrWhiteSpace(targetName))
                    continue;

                if (TryBindSingleButton(targetName))
                {
                    anyBound = true;
                    continue;
                }

                allBound = false;
                if (debugLogs)
                    Debug.LogWarning($"[ButtonPressedBackgroundUI] Button '{targetName}' not found yet.");
            }

            if (TryBindDeckButtons())
                anyBound = true;

            if (anyBound && allBound)
            {
                bindRetryTask?.Pause();
                bindRetryTask = null;
            }
            else if (bindRetryTask == null && root != null)
            {
                bindRetryTask = root.schedule.Execute(TryBindButton).Every(100);
            }
        }

        private bool TryBindSingleButton(string targetName)
        {
            foreach (KeyValuePair<Button, BoundButtonState> pair in boundButtons)
            {
                if (pair.Key != null && string.Equals(pair.Key.name, targetName, StringComparison.Ordinal))
                    return true;
            }

            return TryBindButton(root.Q<Button>(targetName), targetName);
        }

        private bool TryBindDeckButtons()
        {
            bool anyBound = false;

            root.Query<Button>().ForEach(button =>
            {
                if (button == null || string.IsNullOrWhiteSpace(button.name) || !button.name.StartsWith("DeckButton_", StringComparison.Ordinal))
                    return;

                if (TryBindButton(button, button.name))
                    anyBound = true;
            });

            return anyBound;
        }

        private bool TryBindButton(Button button, string targetName)
        {
            if (button == null)
                return false;

            foreach (KeyValuePair<Button, BoundButtonState> pair in boundButtons)
            {
                if (pair.Key == button)
                    return true;
            }

            BoundButtonState state = new BoundButtonState
            {
                Button = button,
                InitialBackground = button.style.backgroundImage,
                HasInitialBackground = true
            };

            if (normalBackground != null)
                ApplyBackground(state, normalBackground);

            state.ClickHandler = () => OnClicked(state);

            button.RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
            button.RegisterCallback<PointerUpEvent>(OnPointerUp, TrickleDown.TrickleDown);
            button.RegisterCallback<PointerLeaveEvent>(OnPointerLeave, TrickleDown.TrickleDown);
            button.RegisterCallback<PointerCancelEvent>(OnPointerCancel, TrickleDown.TrickleDown);
            button.RegisterCallback<MouseDownEvent>(OnMouseDown, TrickleDown.TrickleDown);
            button.RegisterCallback<MouseUpEvent>(OnMouseUp, TrickleDown.TrickleDown);
            button.clicked += state.ClickHandler;

            boundButtons[button] = state;

            if (debugLogs)
                Debug.Log($"[ButtonPressedBackgroundUI] Bound to button '{targetName}'.");

            return true;
        }

        private IEnumerable<string> GetTargetButtonNames()
        {
            if (buttonNames != null && buttonNames.Length > 0)
            {
                for (int i = 0; i < buttonNames.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(buttonNames[i]))
                        yield return buttonNames[i].Trim();
                }

                yield break;
            }

            if (!string.IsNullOrWhiteSpace(buttonName))
                yield return buttonName.Trim();
        }

        private void OnDisable()
        {
            CancelInvoke(nameof(RestoreBackground));

            bindRetryTask?.Pause();
            bindRetryTask = null;

            foreach (KeyValuePair<Button, BoundButtonState> pair in boundButtons)
            {
                Button button = pair.Key;
                BoundButtonState state = pair.Value;

                if (button == null || state == null)
                    continue;

                button.UnregisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
                button.UnregisterCallback<PointerUpEvent>(OnPointerUp, TrickleDown.TrickleDown);
                button.UnregisterCallback<PointerLeaveEvent>(OnPointerLeave, TrickleDown.TrickleDown);
                button.UnregisterCallback<PointerCancelEvent>(OnPointerCancel, TrickleDown.TrickleDown);
                button.UnregisterCallback<MouseDownEvent>(OnMouseDown, TrickleDown.TrickleDown);
                button.UnregisterCallback<MouseUpEvent>(OnMouseUp, TrickleDown.TrickleDown);

                if (state.ClickHandler != null)
                    button.clicked -= state.ClickHandler;

                RestoreBackground(state);
            }

            boundButtons.Clear();
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            Button button = evt.currentTarget as Button;
            Debug.Log($"[ButtonPressedBackgroundUI] PointerDown on '{button?.name ?? buttonName}'");
            ShowPressedBackground(button);
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            Button button = evt.currentTarget as Button;
            Debug.Log($"[ButtonPressedBackgroundUI] PointerUp on '{button?.name ?? buttonName}'");
            RestoreBackgroundAfterDelay(button);
        }

        private void OnPointerLeave(PointerLeaveEvent evt)
        {
            Button button = evt.currentTarget as Button;
            Debug.Log($"[ButtonPressedBackgroundUI] PointerLeave on '{button?.name ?? buttonName}'");
            RestoreBackground(button);
        }

        private void OnPointerCancel(PointerCancelEvent evt)
        {
            Button button = evt.currentTarget as Button;
            Debug.Log($"[ButtonPressedBackgroundUI] PointerCancel on '{button?.name ?? buttonName}'");
            RestoreBackground(button);
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            Button button = evt.currentTarget as Button;
            Debug.Log($"[ButtonPressedBackgroundUI] MouseDown on '{button?.name ?? buttonName}'");
            ShowPressedBackground(button);
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            Button button = evt.currentTarget as Button;
            Debug.Log($"[ButtonPressedBackgroundUI] MouseUp on '{button?.name ?? buttonName}'");
            RestoreBackgroundAfterDelay(button);
        }

        private void OnClicked(BoundButtonState state)
        {
            Debug.Log($"[ButtonPressedBackgroundUI] clicked event fired on '{state?.Button?.name ?? buttonName}'");
        }

        private void ShowPressedBackground(Button button)
        {
            CancelInvoke(nameof(RestoreBackground));

            if (button == null)
                return;

            if (pressedBackground != null)
                ApplyBackground(GetState(button), pressedBackground);
        }

        private void RestoreBackgroundAfterDelay(Button button)
        {
            CancelInvoke(nameof(RestoreBackground));
            if (pressedVisibleDuration <= 0f)
            {
                RestoreBackground(button);
                return;
            }

            Invoke(nameof(RestoreBackground), pressedVisibleDuration);
        }

        private void ApplyBackground(BoundButtonState state, Texture2D texture)
        {
            if (state == null || state.Button == null)
                return;

            state.Button.style.backgroundImage = new StyleBackground(texture);
        }

        private void RestoreBackground()
        {
            foreach (BoundButtonState state in boundButtons.Values)
                RestoreBackground(state);
        }

        private void RestoreBackground(Button button)
        {
            RestoreBackground(GetState(button));
        }

        private void RestoreBackground(BoundButtonState state)
        {
            if (state == null || state.Button == null)
                return;

            if (normalBackground != null)
            {
                ApplyBackground(state, normalBackground);
                return;
            }

            if (state.HasInitialBackground)
                state.Button.style.backgroundImage = state.InitialBackground;
        }

        private BoundButtonState GetState(Button button)
        {
            if (button == null)
                return null;

            return boundButtons.TryGetValue(button, out BoundButtonState state) ? state : null;
        }
    }
}
