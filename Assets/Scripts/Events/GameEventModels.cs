using System;
using System.Collections.Generic;

namespace AirshipsAndAirIslands.Events
{
    /// <summary>
    /// Describes a change to the shared game state produced by an event outcome.
    /// </summary>
    [Serializable]
    public struct ResourceDelta
    {
        public ResourceType Type;
        public int Amount;

        public ResourceDelta(ResourceType type, int amount)
        {
            Type = type;
            Amount = amount;
        }
    }

    /// <summary>
    /// High-level categories of quests generated or completed by events.
    /// </summary>
    [Serializable]
    public enum QuestType
    {
        Delivery,
        Escort
    }

    /// <summary>
    /// Lightweight quest descriptor used by the event system.
    /// </summary>
    [Serializable]
    public class QuestInfo
    {
        public string QuestId { get; }
        public QuestType Type { get; }
        public string Description { get; }
        public IReadOnlyList<ResourceDelta> Rewards { get; }
        public int DurationDays { get; }

        public QuestInfo(string questId, QuestType type, string description, IReadOnlyList<ResourceDelta> rewards, int durationDays)
        {
            QuestId = questId;
            Type = type;
            Description = description;
            Rewards = rewards;
            DurationDays = durationDays;
        }
    }

    /// <summary>
    /// Result of selecting a particular choice in an event.
    /// </summary>
    [Serializable]
    public class EventResult
    {
        public string ResultText { get; }
        public IReadOnlyList<ResourceDelta> ResourceChanges { get; }
        public QuestInfo QuestGranted { get; }
        public string QuestCompletedId { get; }
        public bool TriggersCombat { get; }
        public bool TriggersAmbush { get; }

        public EventResult(
            string resultText,
            IReadOnlyList<ResourceDelta> resourceChanges,
            QuestInfo questGranted = null,
            string questCompletedId = null,
            bool triggersCombat = false,
            bool triggersAmbush = false)
        {
            ResultText = resultText;
            ResourceChanges = resourceChanges ?? Array.Empty<ResourceDelta>();
            QuestGranted = questGranted;
            QuestCompletedId = questCompletedId;
            TriggersCombat = triggersCombat;
            TriggersAmbush = triggersAmbush;
        }
    }

    /// <summary>
    /// A selectable option within an event.
    /// </summary>
    [Serializable]
    public class EventChoice
    {
        public string ChoiceId { get; }
        public string Description { get; }
        private readonly Func<GameState, bool> _availabilityRule;
        private readonly Func<GameState, EventResult> _resolution;

        public EventChoice(string choiceId, string description, Func<GameState, EventResult> resolution, Func<GameState, bool> availabilityRule = null)
        {
            ChoiceId = choiceId;
            Description = description;
            _resolution = resolution ?? throw new ArgumentNullException(nameof(resolution));
            _availabilityRule = availabilityRule;
        }

        public bool IsAvailable(GameState state)
        {
            return _availabilityRule?.Invoke(state) ?? true;
        }

        public EventResult Resolve(GameState state)
        {
            return _resolution(state);
        }
    }

    /// <summary>
    /// Describes a narrative event with a set of player-facing choices.
    /// </summary>
    [Serializable]
    public class GameEvent
    {
        public string EventId { get; }
        public string Title { get; }
        public string Description { get; }
        public IReadOnlyList<EventChoice> Choices { get; }

        public GameEvent(string eventId, string title, string description, IReadOnlyList<EventChoice> choices)
        {
            EventId = eventId;
            Title = title;
            Description = description;
            Choices = choices;
        }
    }

    /// <summary>
    /// Logged record of a resolved event so that UI or persistence layers can inspect history.
    /// </summary>
    [Serializable]
    public class EventLogEntry
    {
        public string EventId { get; }
        public string ChoiceId { get; }
        public EventResult Result { get; }
        public DateTime OccurredAt { get; }

        public EventLogEntry(string eventId, string choiceId, EventResult result, DateTime occurredAt)
        {
            EventId = eventId;
            ChoiceId = choiceId;
            Result = result;
            OccurredAt = occurredAt;
        }
    }
}
