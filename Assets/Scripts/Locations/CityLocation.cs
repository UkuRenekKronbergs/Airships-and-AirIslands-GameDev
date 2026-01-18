using System.Text;
using AirshipsAndAirIslands.Events;
using AirshipsAndAirIslands.Audio;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
        [SerializeField] private Button roomsButton;

        [Header("Purchase Buttons")]
        [SerializeField] private Button buyFuelButton;
        [SerializeField] private Button visitTavernButton;
        [SerializeField] private Button upgradeDamageButton;

        [Header("City Settings")]
        [SerializeField] private string cityName = "Nimbus Gate";
        [SerializeField, Min(1)] private int healCost = 6;
        [SerializeField, Min(1)] private int healAmount = 8;
        [SerializeField] private ResourceType tradeInputType = ResourceType.Food;
        [SerializeField, Min(1)] private int tradeInputAmount = 4;
        [SerializeField, Min(1)] private int tradeGoldReward = 2;

        [Header("Fuel Station")]
        [SerializeField, Min(0.1f)] private float fuelCostPerUnit = 0.5f;
        [SerializeField, Min(1)] private int maxFuelPurchase = 20;

        [Header("Tavern (Morale & Fatigue)")]
        [SerializeField, Min(1)] private int tavernCost = 4;
        [SerializeField, Min(1)] private int moraleRestore = 15;
        [SerializeField, Min(1)] private int fatigueReduction = 10;

        [Header("Upgrade Shop")]
        [SerializeField, Min(1)] private int damageUpgradeCost = 5;
        [SerializeField, Min(0)] private int damageUpgradeBonus = 2;
        [SerializeField, Min(1)] private int maxDamageUpgrades = 5;

        [SerializeField] private bool loadSceneOnExit;
        [SerializeField] private string exitSceneName = "Map";
        [SerializeField] private string roomsSceneName = "Rooms";

        private void Awake()
        {
            WireButtonListeners();
        }

        private void Start()
        {
            EnsureGameState();
            UpdateHeader();
            UpdateStats();
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

            UpdateHeader();
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
            AudioManager.Instance?.PlayHeal();
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
            AudioManager.Instance?.PlayPurchase();
            UpdateStats();
        }

        public void BuyFuel(int amount = 5)
        {
            if (!EnsureGameState())
            {
                return;
            }

            amount = Mathf.Clamp(amount, 1, maxFuelPurchase);
            int totalCost = Mathf.RoundToInt(amount * fuelCostPerUnit);

            if (!gameState.HasResource(ResourceType.Gold, totalCost))
            {
                SetFeedback($"Need {totalCost} gold to buy {amount} fuel.");
                return;
            }

            gameState.ModifyResource(ResourceType.Gold, -totalCost);
            gameState.ModifyResource(ResourceType.Fuel, amount);
            SetFeedback($"Purchased {amount} fuel for {totalCost} gold.");
            AudioManager.Instance?.PlayPurchase();
            UpdateStats();
        }

        public void RestoreCrewMorale()
        {
            if (!EnsureGameState())
            {
                return;
            }

            int currentMorale = gameState.GetResource(ResourceType.CrewMorale);
            int currentFatigue = gameState.GetResource(ResourceType.CrewFatigue);

            if (currentMorale >= 100 && currentFatigue <= 0)
            {
                SetFeedback("Crew is already in peak condition.");
                return;
            }

            if (!gameState.HasResource(ResourceType.Gold, tavernCost))
            {
                SetFeedback($"Need {tavernCost} gold for a tavern visit.");
                return;
            }

            gameState.ModifyResource(ResourceType.Gold, -tavernCost);
            gameState.ModifyResource(ResourceType.CrewMorale, moraleRestore);
            gameState.ModifyResource(ResourceType.CrewFatigue, -fatigueReduction);
            SetFeedback($"Tavern visit for {tavernCost} gold: +{moraleRestore} morale, -{fatigueReduction} fatigue.");
            AudioManager.Instance?.PlayPurchase();
            UpdateStats();
        }

        public void ReduceCrewFatigue()
        {
            // This method is deprecated - use RestoreCrewMorale() instead for tavern visits
            RestoreCrewMorale();
        }

        public void UpgradeDamage()
        {
            if (!EnsureGameState())
            {
                return;
            }

            if (gameState.DamageUpgrades >= maxDamageUpgrades)
            {
                SetFeedback($"Damage upgrades maxed out ({maxDamageUpgrades}).");
                return;
            }

            if (!gameState.HasResource(ResourceType.Gold, damageUpgradeCost))
            {
                SetFeedback($"Need {damageUpgradeCost} gold for a damage upgrade.");
                return;
            }

            gameState.ModifyResource(ResourceType.Gold, -damageUpgradeCost);
            gameState.IncrementDamageUpgrades();
            SetFeedback($"Weapons upgraded (+{damageUpgradeBonus} damage) for {damageUpgradeCost} gold.");
            AudioManager.Instance?.PlayPurchase();
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
                // This button plays its own heal SFX via code, mark it so AudioManager doesn't add the default click SFX
                if (healButton.GetComponent<AirshipsAndAirIslands.Audio.UIButtonHasCustomSound>() == null)
                {
                    healButton.gameObject.AddComponent<AirshipsAndAirIslands.Audio.UIButtonHasCustomSound>();
                }
            }

            if (tradeButton != null)
            {
                tradeButton.onClick.RemoveListener(TradeForGold);
                tradeButton.onClick.AddListener(TradeForGold);
                // This button plays its own purchase SFX via code, mark it so AudioManager doesn't add the default click SFX
                if (tradeButton.GetComponent<AirshipsAndAirIslands.Audio.UIButtonHasCustomSound>() == null)
                {
                    tradeButton.gameObject.AddComponent<AirshipsAndAirIslands.Audio.UIButtonHasCustomSound>();
                }
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(OnCloseButtonPressed);
                closeButton.onClick.AddListener(OnCloseButtonPressed);
            }

            if (roomsButton != null)
            {
                roomsButton.onClick.RemoveListener(OnRoomsButtonPressed);
                roomsButton.onClick.AddListener(OnRoomsButtonPressed);
            }

            // Wire purchase buttons
            if (buyFuelButton != null)
            {
                buyFuelButton.onClick.RemoveListener(() => BuyFuel());
                buyFuelButton.onClick.AddListener(() => BuyFuel());
                if (buyFuelButton.GetComponent<AirshipsAndAirIslands.Audio.UIButtonHasCustomSound>() == null)
                {
                    buyFuelButton.gameObject.AddComponent<AirshipsAndAirIslands.Audio.UIButtonHasCustomSound>();
                }
            }

            if (visitTavernButton != null)
            {
                visitTavernButton.onClick.RemoveListener(RestoreCrewMorale);
                visitTavernButton.onClick.AddListener(RestoreCrewMorale);
                if (visitTavernButton.GetComponent<AirshipsAndAirIslands.Audio.UIButtonHasCustomSound>() == null)
                {
                    visitTavernButton.gameObject.AddComponent<AirshipsAndAirIslands.Audio.UIButtonHasCustomSound>();
                }
            }

            if (upgradeDamageButton != null)
            {
                upgradeDamageButton.onClick.RemoveListener(UpgradeDamage);
                upgradeDamageButton.onClick.AddListener(UpgradeDamage);
                if (upgradeDamageButton.GetComponent<AirshipsAndAirIslands.Audio.UIButtonHasCustomSound>() == null)
                {
                    upgradeDamageButton.gameObject.AddComponent<AirshipsAndAirIslands.Audio.UIButtonHasCustomSound>();
                }
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

            var builder = new StringBuilder();

            AppendStat("Hull", $"{gameState.GetResource(ResourceType.Hull)}/{gameState.MaxHull}");
            AppendStat("Gold", gameState.GetResource(ResourceType.Gold).ToString());
            AppendStat("Fuel", gameState.GetResource(ResourceType.Fuel).ToString());
            AppendStat("Food", gameState.GetResource(ResourceType.Food).ToString());
            AppendStat("Ammo", gameState.GetResource(ResourceType.Ammo).ToString());
            AppendStat("Crew Morale", gameState.GetResource(ResourceType.CrewMorale).ToString());
            AppendStat("Crew Fatigue", gameState.GetResource(ResourceType.CrewFatigue).ToString());

            statsText.text = builder.ToString().TrimEnd('\r', '\n');

            void AppendStat(string label, string value)
            {
                builder.Append(label);
                builder.Append(": ");
                builder.AppendLine(value);
            }
        }

        private void SetFeedback(string message)
        {
            if (feedbackText == null)
            {
                return;
            }

            feedbackText.text = message;
        }

        public void OnRoomsButtonPressed()
        {
            if (string.IsNullOrWhiteSpace(roomsSceneName))
            {
                Debug.LogWarning("CityLocation rooms scene name is empty.");
                return;
            }

            if (Application.CanStreamedLevelBeLoaded(roomsSceneName))
            {
                SceneManager.LoadScene(roomsSceneName);
            }
            else
            {
                Debug.LogWarning($"CityLocation could not load rooms scene '{roomsSceneName}'. Check build settings.");
            }
        }

        private void UpdateHeader()
        {
            if (headerText == null)
            {
                return;
            }

            var displayName = string.IsNullOrWhiteSpace(cityName) ? transform.root.name : cityName;
            headerText.text = displayName;
        }
    }
}
