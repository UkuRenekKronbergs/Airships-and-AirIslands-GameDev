using System;
using System.Collections.Generic;
using UnityEngine;

namespace AirshipsAndAirIslands.Events
{
    /// <summary>
    /// Handles selection and resolution of random events, including book-keeping for history and quest state.
    /// </summary>
    public class GameEventManager : MonoBehaviour
    {
        [SerializeField] private GameState gameState;
        [SerializeField] private int seed = 0;

        private System.Random _rng;
        private readonly List<GameEvent> _events = new();
        private readonly List<EventLogEntry> _history = new();
        public IReadOnlyList<EventLogEntry> History => _history;
    public IReadOnlyList<GameEvent> Events => _events;

        private void Awake()
        {
            _rng = seed == 0 ? new System.Random() : new System.Random(seed);
            BuildEventDatabase();
        }

        private void Reset()
        {
            if (gameState == null)
            {
                // Prefer the persistent singleton instance if available
                gameState = GameState.Instance ?? FindFirstObjectByType<GameState>();
            }
        }

        public GameEvent GetRandomEvent()
        {
            if (_events.Count == 0)
            {
                throw new InvalidOperationException("Event database is empty. Ensure BuildEventDatabase populated events.");
            }

            var index = _rng.Next(_events.Count);
            return _events[index];
        }
        public EventResult ResolveChoice(GameEvent gameEvent, string choiceId)
        {
            if (gameEvent == null)
            {
                throw new ArgumentNullException(nameof(gameEvent));
            }

            if (string.IsNullOrWhiteSpace(choiceId))
            {
                throw new ArgumentException("Choice id must be provided", nameof(choiceId));
            }

            var choice = FindChoice(gameEvent, choiceId);
            if (choice == null)
            {
                throw new ArgumentException($"Choice '{choiceId}' not found for event '{gameEvent.EventId}'", nameof(choiceId));
            }

            EnsureGameStateReference();

            if (!choice.IsAvailable(gameState))
            {
                throw new InvalidOperationException($"Choice '{choiceId}' is not currently available.");
            }

            var result = choice.Resolve(gameState);

            if (result.ResourceChanges != null)
            {
                gameState.ApplyResourceChanges(result.ResourceChanges);
            }

            if (!string.IsNullOrEmpty(result.QuestCompletedId))
            {
                if (gameState.CompleteQuest(result.QuestCompletedId, out _))
                {
                    // completed quest rewards already granted by CompleteQuest
                }
            }

            if (result.QuestGranted != null)
            {
                gameState.TryAddQuest(result.QuestGranted);
            }

            _history.Add(new EventLogEntry(gameEvent.EventId, choiceId, result, DateTime.UtcNow));
            return result;
        }

        private static EventChoice FindChoice(GameEvent gameEvent, string choiceId)
        {
            foreach (var choice in gameEvent.Choices)
            {
                if (choice.ChoiceId.Equals(choiceId, StringComparison.OrdinalIgnoreCase))
                {
                    return choice;
                }
            }

            return null;
        }

        private void BuildEventDatabase()
        {
            _events.Clear();
            if (gameState == null)
            {
                // Prefer the persistent singleton instance when available
                gameState = GameState.Instance ?? FindFirstObjectByType<GameState>();
                if (gameState == null)
                {
                    Debug.LogWarning("GameState not found in scene. Event effects will still be generated but not applied.");
                }
            }

            _events.Add(CreateResourceGainEvent());
            _events.Add(CreateResourceLossEvent());
            _events.Add(CreateCrewMoraleEvent());
            _events.Add(CreateCrewNeedsEvent());
            _events.Add(CreateCombatTriggerEvent());
            _events.Add(CreateAmbushEvent());
            _events.Add(CreateTradeOpportunityEvent());
            _events.Add(CreateDeliveryQuestEvent());
            _events.Add(CreateEscortQuestEvent());
            _events.Add(CreateMysteryEvent());
        }

        #region Event Definitions

        private void EnsureGameStateReference()
        {
            if (gameState != null)
            {
                return;
            }

            gameState = FindFirstObjectByType<GameState>();
            if (gameState == null)
            {
                throw new InvalidOperationException("GameState component not found in scene. Attach one or assign it on the GameEventManager.");
            }
        }

        private GameEvent CreateResourceGainEvent()
        {
            const string id = "event_resource_gain";
            return new GameEvent(
                id,
                "Derelict Supply Barge",
                "A broken-down sky barge drifts nearby. Crates marked with guild seals still hang from its tethers.",
                new List<EventChoice>
                {
                    new EventChoice(
                        "salvage_carefully",
                        "Send engineers over in suits and salvage what you can.",
                        state => new EventResult(
                            "Careful planning pays off. You secure intact crates of supplies.",
                            new []
                            {
                                new ResourceDelta(ResourceType.Fuel, 3),
                                new ResourceDelta(ResourceType.Food, 5),
                                new ResourceDelta(ResourceType.Ammo, 2)
                            }
                        )
                    ),
                    new EventChoice(
                        "strip_fast",
                        "Rush the salvage to beat any scavengers, risking damage.",
                        state =>
                        {
                            // Base random gains
                            var fuelGain = UnityEngine.Random.Range(1, 5);
                            var foodGain = UnityEngine.Random.Range(2, 7);

                            // Adjust chance for extra haul based on crew morale vs fatigue
                            var morale = state.GetResource(ResourceType.CrewMorale);
                            var fatigueVal = state.GetResource(ResourceType.CrewFatigue);
                            var baseChance = 0.25f; // small chance to score extra
                            var successChance = Mathf.Clamp01(baseChance + (morale - fatigueVal) / 400f);
                            var roll = UnityEngine.Random.Range(0f, 1f);

                            var resultText = "Your crew rushes the job, scoring extra supplies but a crate ruptures, coating the deck.";
                            var deltas = new List<ResourceDelta>();

                            if (roll < successChance)
                            {
                                // bonus haul
                                var extraFuel = UnityEngine.Random.Range(1, 3);
                                var extraFood = UnityEngine.Random.Range(1, 4);
                                fuelGain += extraFuel;
                                foodGain += extraFood;
                                resultText = $"The rush paid off — you hauled extra supplies.";
                                // mention morale effect if any
                                var adjPct = Mathf.RoundToInt((successChance - baseChance) * 100f);
                                if (adjPct != 0)
                                {
                                    resultText += $" (Crew morale affected the chance by {(adjPct>0?"+":"")}{adjPct}%.)";
                                }
                            }
                            else
                            {
                                // mishap: small hull damage and more fatigue
                                resultText = "A crate ruptures during the rush, coating the deck and denting the hull.";
                                deltas.Add(new ResourceDelta(ResourceType.Hull, -1));
                            }

                            deltas.Add(new ResourceDelta(ResourceType.Fuel, fuelGain));
                            deltas.Add(new ResourceDelta(ResourceType.Food, foodGain));
                            var fatigue = new ResourceDelta(ResourceType.CrewFatigue, UnityEngine.Random.Range(4, 9));
                            deltas.Add(fatigue);
                            return new EventResult(resultText, deltas);
                        }
                    )
                }
            );
        }

        private GameEvent CreateResourceLossEvent()
        {
            const string id = "event_resource_loss";
            return new GameEvent(
                id,
                "Fuel Line Microfracture",
                "Engineering reports a thinning seam in the fuel main. It's bleeding vapors into the corridor.",
                new List<EventChoice>
                {
                    new EventChoice(
                        "seal_line",
                        "Shut down the line and patch it with reserve plating.",
                        state => new EventResult(
                            "The line is sealed but the crew vents a chunk of fuel before it catches.",
                            new []
                            {
                                new ResourceDelta(ResourceType.Fuel, -4),
                                new ResourceDelta(ResourceType.CrewFatigue, 6)
                            }
                        ),
                        state => state.HasResource(ResourceType.Fuel, 4)
                    ),
                    new EventChoice(
                        "ride_it_out",
                        "Keep pressure steady and hope it holds until the next dock.",
                        state => new EventResult(
                            "Luck isn't on your side. The seam bursts, spraying burning fuel before containment shutters close.",
                            new []
                            {
                                new ResourceDelta(ResourceType.Fuel, -6),
                                new ResourceDelta(ResourceType.Hull, -2),
                                new ResourceDelta(ResourceType.CrewMorale, -8)
                            }
                        )
                    )
                }
            );
        }

        private GameEvent CreateCrewMoraleEvent()
        {
            const string id = "event_crew_morale";
            return new GameEvent(
                id,
                "Festival in the Clouds",
                "A floating carnival ropes alongside, its performers offering a night of music for a modest fee.",
                new List<EventChoice>
                {
                    new EventChoice(
                        "grant_leave",
                        "Give the crew shore leave and pay for the show.",
                        state => new EventResult(
                            "Laughter echoes across the deck. Spirits soar as the crew returns reenergised.",
                            new []
                            {
                                new ResourceDelta(ResourceType.Gold, -5),
                                new ResourceDelta(ResourceType.CrewMorale, 12),
                                new ResourceDelta(ResourceType.CrewFatigue, -10)
                            }
                        ),
                        state => state.HasResource(ResourceType.Gold, 5)
                    ),
                    new EventChoice(
                        "decline",
                        "Decline politely and stay on course.",
                        state => new EventResult(
                            "You keep to schedule, though some crew grumble about missed fun.",
                            new []
                            {
                                new ResourceDelta(ResourceType.CrewMorale, -3)
                            }
                        )
                    )
                }
            );
        }

        private GameEvent CreateCrewNeedsEvent()
        {
            const string id = "event_crew_needs";
            return new GameEvent(
                id,
                "Sleepless Watch",
                "Two consecutive night raids have left the crew exhausted. The helmsman fights to keep eyes open.",
                new List<EventChoice>
                {
                    new EventChoice(
                        "stand_down",
                        "Rotate extra crew onto rest shifts and drift through the night.",
                        state => new EventResult(
                            "With the ship on low alert, everyone finally catches meaningful sleep.",
                            new []
                            {
                                new ResourceDelta(ResourceType.CrewFatigue, -20),
                                new ResourceDelta(ResourceType.Fuel, -2)
                            }
                        )
                    ),
                    new EventChoice(
                        "push_on",
                        "Caffeine rations for all. You can't afford to slow down.",
                        state => new EventResult(
                            "Bleary eyes stay fixed on instruments, but tempers flare in the mess.",
                            new []
                            {
                                new ResourceDelta(ResourceType.CrewFatigue, 12),
                                new ResourceDelta(ResourceType.CrewMorale, -5)
                            }
                        )
                    )
                }
            );
        }

        private GameEvent CreateCombatTriggerEvent()
        {
            const string id = "event_combat_trigger";
            return new GameEvent(
                id,
                "Sky Marauder Toll",
                "A squadron of marauders blocks the path, demanding tribute to pass through their claimed airspace.",
                new List<EventChoice>
                {
                    new EventChoice(
                        "pay_toll",
                        "Hand over supplies to avoid a fight.",
                        state =>
                        {
                            // High morale can negotiate a smaller toll; fatigue reduces bargaining effectiveness.
                            var morale = state.GetResource(ResourceType.CrewMorale);
                            var fatigue = state.GetResource(ResourceType.CrewFatigue);
                            var basePayment = 8;
                            var discountChance = Mathf.Clamp01((morale - fatigue) / 200f);
                            var roll = UnityEngine.Random.Range(0f, 1f);
                            var payment = basePayment;
                            var resultText = "The marauders sneer but wave you through after counting the goods.";
                            if (roll < discountChance)
                            {
                                payment = Math.Max(1, basePayment - 2);
                                resultText += $" Your negotiators found a better deal (saved {basePayment - payment} Gold).";
                            }

                            return new EventResult(resultText, new []
                            {
                                new ResourceDelta(ResourceType.Gold, -payment),
                                new ResourceDelta(ResourceType.Food, -3)
                            });
                        },
                        state => state.HasResource(ResourceType.Gold, 8) && state.HasResource(ResourceType.Food, 3)
                    ),
                    new EventChoice(
                        "open_fire",
                        "Arm the guns and blast a path.",
                        state => new EventResult(
                            "You prime the cannons. Combat is inevitable.",
                            new []
                            {
                                new ResourceDelta(ResourceType.Ammo, -2),
                                new ResourceDelta(ResourceType.CrewMorale, 4)
                            },
                            triggersCombat: true
                        )
                    )
                }
            );
        }

        private GameEvent CreateAmbushEvent()
        {
            const string id = "event_ambush";
            return new GameEvent(
                id,
                "Hidden Minefield",
                "An uncharted mine detonates off the bow, and more signatures light up the scope.",
                new List<EventChoice>
                {
                    new EventChoice(
                        "evasive",
                        "Burn hard to clear the field before more mines arm.",
                        state =>
                        {
                            var morale = state.GetResource(ResourceType.CrewMorale);
                            var fatigue = state.GetResource(ResourceType.CrewFatigue);
                            var baseAvoidDamageChance = 0.25f;
                            var avoidChance = Mathf.Clamp01(baseAvoidDamageChance + (morale - fatigue) / 400f);
                            var roll = UnityEngine.Random.Range(0f, 1f);
                            if (roll < avoidChance)
                            {
                                var resultText = "Engines roar as you thread through the pattern. Your crew performs expertly and you avoid major hits.";
                                var deltas = new []
                                {
                                    new ResourceDelta(ResourceType.Fuel, -5),
                                    new ResourceDelta(ResourceType.CrewFatigue, 6)
                                };
                                var adjPct = Mathf.RoundToInt((avoidChance - baseAvoidDamageChance) * 100f);
                                if (adjPct != 0)
                                {
                                    resultText += $" (Crew morale influenced this by {(adjPct>0?"+":"")}{adjPct}%).";
                                }
                                return new EventResult(resultText, deltas);
                            }

                            return new EventResult(
                                "Your crew pushes hard but a mine punches the hull as you clear the field.",
                                new []
                                {
                                    new ResourceDelta(ResourceType.Fuel, -5),
                                    new ResourceDelta(ResourceType.Hull, -4),
                                    new ResourceDelta(ResourceType.CrewFatigue, 8)
                                }
                            );
                        }
                    ),
                    new EventChoice(
                        "brace_for_impact",
                        "Cut engines, batten hatches, and let the mines spend themselves.",
                        state => new EventResult(
                            "Shockwaves hammer the ship. Batteries report incoming raider signatures amid the debris.",
                            new []
                            {
                                new ResourceDelta(ResourceType.Hull, -5),
                                new ResourceDelta(ResourceType.CrewMorale, -6)
                            },
                            triggersCombat: true,
                            triggersAmbush: true
                        )
                    )
                }
            );
        }

        private GameEvent CreateTradeOpportunityEvent()
        {
            const string id = "event_trade";
            return new GameEvent(
                id,
                "Sky Bazaar Convoy",
                "Merchants signal you with bright pennants, offering to barter goods in mid-air.",
                new List<EventChoice>
                {
                    new EventChoice(
                        "trade_food_for_ammo",
                        "Trade surplus food for ammunition.",
                        state => new EventResult(
                            "The barter goes smoothly; crates are winched across on glistening cables.",
                            new []
                            {
                                new ResourceDelta(ResourceType.Food, -4),
                                new ResourceDelta(ResourceType.Ammo, 4)
                            }
                        ),
                        state => state.HasResource(ResourceType.Food, 4)
                    ),
                    new EventChoice(
                        "sell_fuel",
                        "Sell a portion of your fuel reserves for gold.",
                        state =>
                        {
                            var morale = state.GetResource(ResourceType.CrewMorale);
                            var fatigue = state.GetResource(ResourceType.CrewFatigue);
                            var baseGold = 9;
                            var bonusChance = Mathf.Clamp01((morale - fatigue) / 300f);
                            var roll = UnityEngine.Random.Range(0f, 1f);
                            var gold = baseGold;
                            var resultText = "Guild factors stamp approval and credit your account.";
                            if (roll < bonusChance)
                            {
                                var bonus = UnityEngine.Random.Range(2, 6);
                                gold += bonus;
                                resultText += $" You negotiated a better price (+{bonus} Gold).";
                            }

                            return new EventResult(resultText, new []
                            {
                                new ResourceDelta(ResourceType.Fuel, -3),
                                new ResourceDelta(ResourceType.Gold, gold)
                            });
                        },
                        state => state.HasResource(ResourceType.Fuel, 3)
                    ),
                    new EventChoice(
                        "decline_trade",
                        "Decline and keep moving.",
                        state => new EventResult(
                            "You exchange pleasantries and continue on schedule.",
                            Array.Empty<ResourceDelta>()
                        )
                    )
                }
            );
        }

        private GameEvent CreateDeliveryQuestEvent()
        {
            const string id = "event_delivery_quest";
            return new GameEvent(
                id,
                "Courier Guild Contract",
                "A courier drone latches on, broadcasting a contract request for urgent cargo delivery to Nimbus Gate.",
                new List<EventChoice>
                {
                    new EventChoice(
                        "accept_delivery",
                        "Accept the sealed crate and promise delivery.",
                        state => new EventResult(
                            "You sign the manifest. The guild promises payment on arrival.",
                            Array.Empty<ResourceDelta>(),
                            new QuestInfo(
                                "quest_delivery_nimbus_gate",
                                QuestType.Delivery,
                                "Deliver the sealed courier crate to Nimbus Gate within three jumps.",
                                new []
                                {
                                    new ResourceDelta(ResourceType.Gold, 15),
                                    new ResourceDelta(ResourceType.CrewMorale, 4)
                                },
                                durationDays: 3
                            )
                        )
                    ),
                    new EventChoice(
                        "refuse_delivery",
                        "Refuse; no room in the hold.",
                        state => new EventResult(
                            "The drone detaches with a disappointed chirp.",
                            new []
                            {
                                new ResourceDelta(ResourceType.CrewMorale, -2)
                            }
                        )
                    )
                }
            );
        }

        private GameEvent CreateEscortQuestEvent()
        {
            const string id = "event_escort_quest";
            return new GameEvent(
                id,
                "Caravan Seeking Escort",
                "A civilian island-cluster wants safe passage through pirate territory.",
                new List<EventChoice>
                {
                    new EventChoice(
                        "accept_escort",
                        "Escort the caravan for a share of their cargo.",
                        state =>
                        {
                            var morale = state.GetResource(ResourceType.CrewMorale);
                            var fatigue = state.GetResource(ResourceType.CrewFatigue);
                            var goldReward = 10;
                            var fuelReward = 4;
                            var bonusFactor = Mathf.Clamp01((morale - fatigue) / 200f);
                            if (bonusFactor > 0.1f)
                            {
                                var extraGold = Mathf.RoundToInt(5 * bonusFactor);
                                goldReward += extraGold;
                            }

                            var quest = new QuestInfo(
                                "quest_escort_caravan",
                                QuestType.Escort,
                                "Protect the civilian caravan for two jumps without losing more than 2 hull.",
                                new []
                                {
                                    new ResourceDelta(ResourceType.Fuel, fuelReward),
                                    new ResourceDelta(ResourceType.Gold, goldReward),
                                },
                                durationDays: 2
                            );

                            var resultText = "You align speed with the caravan. They'll follow your signal beacon.";
                            if (bonusFactor > 0.1f)
                            {
                                resultText += " Your crew's high morale improves the promised payment.";
                            }

                            return new EventResult(resultText, Array.Empty<ResourceDelta>(), quest);
                        }
                    ),
                    new EventChoice(
                        "decline_escort",
                        "Decline; the route is too dangerous right now.",
                        state => new EventResult(
                            "You transmit regrets. The caravan drifts away in search of another protector.",
                            Array.Empty<ResourceDelta>()
                        )
                    )
                }
            );
        }

        private GameEvent CreateMysteryEvent()
        {
            const string id = "event_mystery";
            return new GameEvent(
                id,
                "Oracle's Beacon",
                "An ancient lighthouse pulses with an eerie frequency. Legends promise secrets to captains who answer.",
                new List<EventChoice>
                {
                    new EventChoice(
                        "tune_beacon",
                        "Match the signal using navigation crystals.",
                        state =>
                        {
                            var morale = state.GetResource(ResourceType.CrewMorale);
                            var fatigue = state.GetResource(ResourceType.CrewFatigue);
                            var baseSuccess = 0.4f;
                            var successChance = Mathf.Clamp01(baseSuccess + (morale - fatigue) / 400f);
                            var outcomeRoll = UnityEngine.Random.Range(0f, 1f);

                            if (outcomeRoll < successChance)
                            {
                                var adjPct = Mathf.RoundToInt((successChance - baseSuccess) * 100f);
                                var text = "The beacon flares, revealing a hidden cache nearby!";
                                if (adjPct != 0)
                                {
                                    text += $" (Crew morale affected the chance by {(adjPct>0?"+":"")}{adjPct}%.)";
                                }

                                return new EventResult(
                                    text,
                                    new []
                                    {
                                        new ResourceDelta(ResourceType.Fuel, 4),
                                        new ResourceDelta(ResourceType.Gold, 7)
                                    }
                                );
                            }

                            if (outcomeRoll < successChance + 0.4f)
                            {
                                return new EventResult(
                                    "Visions unsettle the crew. Sleepless nights follow.",
                                    new []
                                    {
                                        new ResourceDelta(ResourceType.CrewMorale, -6),
                                        new ResourceDelta(ResourceType.CrewFatigue, 10)
                                    }
                                );
                            }

                            return new EventResult(
                                "The beacon marks you as an intruder. Autonomous guardians emerge from the mist!",
                                new []
                                {
                                    new ResourceDelta(ResourceType.Hull, -3)
                                },
                                triggersCombat: true
                            );
                        }
                    ),
                    new EventChoice(
                        "ignore_beacon",
                        "Ignore the omen and sail on.",
                        state => new EventResult(
                            "The pulsing fades astern. Some crew breathe easier, others grumble about missed chance.",
                            new []
                            {
                                new ResourceDelta(ResourceType.CrewMorale, -1)
                            }
                        )
                    )
                }
            );
        }

        #endregion
    }
}
