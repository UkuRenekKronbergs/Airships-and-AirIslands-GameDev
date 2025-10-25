using AirshipsAndAirIslands.Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace AirshipsAndAirIslands.Locations
{
    /// <summary>
    /// Represents a handmade city node where the player can heal the ship and trade other resources for gold.
    /// Hook the public methods up to UI buttons for interactable city services.
    /// </summary>
    public class CityLocation : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameState gameState;
        [SerializeField] private GameObject cityUiRoot;
        [SerializeField] private TMP_Text headerText;
        [SerializeField] private TMP_Text statsText;
        [SerializeField] private TMP_Text feedbackText;

        [Header("UI Buttons")]
        [SerializeField] private Button healButton;
        [SerializeField] private Button tradeButton;
        [SerializeField] private Button closeButton;

        [Header("City Settings")]
        [SerializeField] private string cityName = "Nimbus Gate";
        [SerializeField, Min(1)] private int healCost = 6;
        [SerializeField, Min(1)] private int healAmount = 8;
        [SerializeField] private ResourceType tradeInputType = ResourceType.Food;
        [SerializeField, Min(1)] private int tradeInputAmount = 4;
        [SerializeField, Min(1)] private int tradeGoldReward = 2;
    [SerializeField] private bool loadSceneOnExit;
    [SerializeField] private string exitSceneName = "Map";

        private void Awake()
        {
            if (cityUiRoot != null)
            {
                cityUiRoot.SetActive(false);
            }

            WireButtonListeners();
        }

        private void Reset()
        {
            if (gameState == null)
            {
                gameState = FindFirstObjectByType<GameState>();
            }
        }

        private void OnMouseDown()
        {
            OpenCity();
        }

        public void OpenCity()
        {
            if (!EnsureGameState())
            {
                Debug.LogWarning("CityLocation could not find GameState in the scene.");
                return;
            }

            if (cityUiRoot != null)
            {
                cityUiRoot.SetActive(true);
            }

            if (headerText != null)
            {
                headerText.text = cityName;
            }

            UpdateStats();
            SetFeedback(string.Empty);
        }

        public void CloseCity()
        {
            if (cityUiRoot != null)
            {
                cityUiRoot.SetActive(false);
            }
        }

        public void HealShip()
        {
            if (!EnsureGameState())
            {
                return;
            }

            if (gameState.GetResource(ResourceType.Hull) >= gameState.MaxHull)
            {
                SetFeedback("Hull already at maximum strength.");
                return;
            }

            if (!gameState.HasResource(ResourceType.Gold, healCost))
            {
                SetFeedback($"Need {healCost} gold for repairs.");
                return;
            }

            var preHull = gameState.GetResource(ResourceType.Hull);
            gameState.ModifyResource(ResourceType.Gold, -healCost);
            var newHull = gameState.ModifyResource(ResourceType.Hull, healAmount);
            var restored = Mathf.Max(0, newHull - preHull);
            SetFeedback(restored > 0
                ? $"Repaired {restored} hull for {healCost} gold."
                : "Hull repairs provided no additional benefit.");
            UpdateStats();
        }

        public void TradeForGold()
        {
            if (!EnsureGameState())
            {
                return;
            }

            if (tradeInputAmount <= 0)
            {
                SetFeedback("Trade amount must be greater than zero.");
                return;
            }

            if (!gameState.HasResource(tradeInputType, tradeInputAmount))
            {
                SetFeedback($"Need {tradeInputAmount} {tradeInputType} to trade for gold.");
                return;
            }

            gameState.ModifyResource(tradeInputType, -tradeInputAmount);
            gameState.ModifyResource(ResourceType.Gold, tradeGoldReward);
            SetFeedback($"Received {tradeGoldReward} gold for {tradeInputAmount} {tradeInputType}.");
            UpdateStats();
        }

        public void OnCloseButtonPressed()
        {
            if (loadSceneOnExit && !string.IsNullOrWhiteSpace(exitSceneName))
            {
                if (Application.CanStreamedLevelBeLoaded(exitSceneName))
                {
                    SceneManager.LoadScene(exitSceneName);
                }
                else
                {
                    Debug.LogWarning($"CityLocation could not load scene '{exitSceneName}'. Check build settings.");
                }
            }
            else
            {
                CloseCity();
            }
        }

        private void WireButtonListeners()
        {
            if (healButton != null)
            {
                healButton.onClick.RemoveListener(HealShip);
                healButton.onClick.AddListener(HealShip);
            }

            if (tradeButton != null)
            {
                tradeButton.onClick.RemoveListener(TradeForGold);
                tradeButton.onClick.AddListener(TradeForGold);
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(OnCloseButtonPressed);
                closeButton.onClick.AddListener(OnCloseButtonPressed);
            }
        }

        private bool EnsureGameState()
        {
            if (gameState != null)
            {
                return true;
            }

            gameState = FindFirstObjectByType<GameState>();
            return gameState != null;
        }

        private void UpdateStats()
        {
            if (statsText == null || gameState == null)
            {
                return;
            }

            statsText.text =
                $"Hull: {gameState.GetResource(ResourceType.Hull)}/{gameState.MaxHull}\n" +
                $"Gold: {gameState.GetResource(ResourceType.Gold)}";
        }

        private void SetFeedback(string message)
        {
            if (feedbackText == null)
            {
                return;
            }

            feedbackText.text = message;
        }
    }
}
