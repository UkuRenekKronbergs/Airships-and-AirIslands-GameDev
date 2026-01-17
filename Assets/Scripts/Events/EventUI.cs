using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AirshipsAndAirIslands.Events
{
    /// <summary>
    /// Minimal runtime UI for showing a GameEvent and letting the player choose an option.
    /// Creates a simple Canvas/modal on demand and cleans up when closed.
    /// </summary>
    public class EventUI : MonoBehaviour
    {
        [SerializeField] private GameEventManager gameEventManager;

        private Font _font;

        private void Reset()
        {
            if (gameEventManager == null)
            {
                gameEventManager = GameState.Instance?.GetComponent<GameEventManager>() ?? UnityEngine.Object.FindFirstObjectByType<GameEventManager>();
            }
        }

        private void Awake()
        {
            // Load a safe built-in font. Newer Unity versions expose "LegacyRuntime.ttf".
            _font = null;
            try
            {
                _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            catch {}

            if (_font == null)
            {
                try
                {
                    _font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                }
                catch {}
            }

            if (_font == null)
            {
                Debug.LogWarning("EventUI: built-in font not found. UI text may not display correctly.");
            }

            if (gameEventManager == null)
            {
                gameEventManager = GameState.Instance?.GetComponent<GameEventManager>() ?? UnityEngine.Object.FindFirstObjectByType<GameEventManager>();
            }

            Debug.Log($"EventUI Awake: font={( _font != null ? _font.name : "<null>")}, gameEventManager={(gameEventManager != null ? gameEventManager.name : "<null>")}");
        }

        /// <summary>
        /// Allows external wiring of the GameEventManager used by this EventUI.
        /// </summary>
        public void SetGameEventManager(GameEventManager manager)
        {
            gameEventManager = manager;
        }

        public IEnumerator ShowEventCoroutine(GameEvent gameEvent, Action<EventResult, bool> onComplete)
        {
            if (gameEvent == null)
            {
                onComplete?.Invoke(null, false);
                yield break;
            }

            // Build UI
            var canvasGO = new GameObject("EventUICanvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            var panelGO = new GameObject("Panel");
            panelGO.transform.SetParent(canvasGO.transform, false);
            var panelImage = panelGO.AddComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.75f);
            var panelRect = panelGO.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 0f);
            panelRect.anchorMax = new Vector2(1f, 1f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // Content container
            var contentGO = new GameObject("Content");
            contentGO.transform.SetParent(panelGO.transform, false);
            var contentRect = contentGO.AddComponent<RectTransform>();
            contentRect.sizeDelta = new Vector2(800, 400);
            contentRect.anchorMin = new Vector2(0.5f, 0.5f);
            contentRect.anchorMax = new Vector2(0.5f, 0.5f);
            contentRect.anchoredPosition = Vector2.zero;

            var contentImage = contentGO.AddComponent<Image>();
            contentImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);

            var layout = contentGO.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(12, 12, 12, 12);
            layout.spacing = 8;

            // Title
            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(contentGO.transform, false);
            var titleText = titleGO.AddComponent<Text>();
            titleText.font = _font;
            titleText.text = gameEvent.Title;
            titleText.fontSize = 26;
            titleText.color = Color.white;
            titleText.alignment = TextAnchor.MiddleCenter;

            // Description
            var descGO = new GameObject("Description");
            descGO.transform.SetParent(contentGO.transform, false);
            var descText = descGO.AddComponent<Text>();
            descText.font = _font;
            descText.text = gameEvent.Description;
            descText.fontSize = 18;
            descText.color = Color.white;
            descText.alignment = TextAnchor.UpperLeft;

            // Choices container
            var choicesGO = new GameObject("Choices");
            choicesGO.transform.SetParent(contentGO.transform, false);
            var choicesLayout = choicesGO.AddComponent<VerticalLayoutGroup>();
            choicesLayout.spacing = 6;

            // Track created buttons to disable later
            var createdButtons = new List<GameObject>();

            foreach (var choice in gameEvent.Choices)
            {
                var btnGO = CreateButton(choice.Description, _font);
                btnGO.transform.SetParent(choicesGO.transform, false);
                var btn = btnGO.GetComponent<Button>();
                if (!choice.IsAvailable(GameState.Instance))
                {
                    btn.interactable = false;
                }

                var capturedChoice = choice; // capture for lambda
                btn.onClick.AddListener(() =>
                {
                    // resolve via manager
                    EventResult result = null;
                    try
                    {
                        if (gameEventManager != null)
                        {
                            result = gameEventManager.ResolveChoice(gameEvent, capturedChoice.ChoiceId);
                        }
                        else
                        {
                            result = capturedChoice.Resolve(GameState.Instance);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"Error resolving event choice: {ex.Message}");
                    }

                    // show result text and wait for continue
                    StartCoroutine(ShowResultThenClose(result, canvasGO, onComplete));
                    // disable further input
                    foreach (var b in createdButtons)
                    {
                        var button = b.GetComponent<Button>();
                        if (button != null) button.interactable = false;
                    }
                });

                createdButtons.Add(btnGO);
            }

            // Skip / Leave button
            var skipBtn = CreateButton("Leave it", _font);
            skipBtn.transform.SetParent(choicesGO.transform, false);
            skipBtn.GetComponent<Button>().onClick.AddListener(() =>
            {
                onComplete?.Invoke(null, false);
                Destroy(canvasGO);
            });

            // Wait until the callback signals completion (onComplete invoked by ShowResultThenClose or skip)
            // We don't have a direct signal here; instead ShowResultThenClose will call onComplete and clean up.
            // To keep the coroutine alive until user acts, yield until the canvas is destroyed.
            while (canvasGO != null)
            {
                yield return null;
            }
        }

        private IEnumerator ShowResultThenClose(EventResult result, GameObject canvasGO, Action<EventResult, bool> onComplete)
        {
            // Find the content area so we can append a result text and a continue button
            var content = canvasGO.transform.Find("Panel/Content");
            if (content == null)
            {
                // Fallback: try to find any child named Content
                foreach (Transform t in canvasGO.transform)
                {
                    if (t.name == "Content")
                    {
                        content = t;
                        break;
                    }
                }
            }

            if (content == null)
            {
                onComplete?.Invoke(result, result != null);
                Destroy(canvasGO);
                yield break;
            }

            var resultGO = new GameObject("ResultText");
            resultGO.transform.SetParent(content, false);
            var rt = resultGO.AddComponent<Text>();
            rt.font = _font;
            rt.fontSize = 18;
            rt.color = Color.yellow;
            rt.alignment = TextAnchor.UpperLeft;

            // Build the result text including any resource changes so the player can see what changed.
            var display = result?.ResultText ?? "";
            var changes = result?.ResourceChanges;
            if (changes != null && changes.Count > 0)
            {
                display += "\n\n";
                foreach (var delta in changes)
                {
                    var sign = delta.Amount >= 0 ? "Gained" : "Lost";
                    var amt = Math.Abs(delta.Amount);
                    var label = GetResourceLabel(delta.Type);
                    display += $"{sign} {amt} {label}.\n";
                }
            }

            rt.text = display;

            // Add a Close button that the player must click to proceed. This avoids unexpected auto-navigation.
            var closeBtnGO = CreateButton("Continue", _font);
            closeBtnGO.transform.SetParent(content, false);
            var closeBtn = closeBtnGO.GetComponent<Button>();
            bool clicked = false;
            closeBtn.onClick.AddListener(() => { clicked = true; });

            // Wait for click
            while (!clicked)
            {
                yield return null;
            }

            onComplete?.Invoke(result, result != null);
            Destroy(canvasGO);
        }

        private static string GetResourceLabel(ResourceType type)
        {
            return type switch
            {
                ResourceType.Fuel => "Fuel",
                ResourceType.Food => "Food",
                ResourceType.Ammo => "Ammo",
                ResourceType.Gold => "Gold",
                ResourceType.Hull => "Hull",
                ResourceType.CrewMorale => "Crew Morale",
                ResourceType.CrewFatigue => "Crew Fatigue",
                _ => type.ToString()
            };
        }

        private GameObject CreateButton(string text, Font font)
        {
            var btnGO = new GameObject("Button");
            var image = btnGO.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            var btn = btnGO.AddComponent<Button>();

            var rect = btnGO.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(600, 40);

            var txtGO = new GameObject("Text");
            txtGO.transform.SetParent(btnGO.transform, false);
            var txt = txtGO.AddComponent<Text>();
            txt.font = font;
            txt.text = text;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.fontSize = 16;

            var txtRect = txtGO.GetComponent<RectTransform>();
            txtRect.anchorMin = new Vector2(0, 0);
            txtRect.anchorMax = new Vector2(1, 1);
            txtRect.offsetMin = Vector2.zero;
            txtRect.offsetMax = Vector2.zero;

            return btnGO;
        }
    }
}
