using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace PositiveConnectionsNmSpc;

public class InteractionWorker_GiveComfort : InteractionWorker
{
    public static event Action<Pawn, float, string, int> OnPositiveInteraction;

    private PositiveConnectionsModSettings _modSettings = PositiveConnections.Instance.GetSettings<PositiveConnectionsModSettings>();
    
    // Encapsulated property for the last computed selection weight
    public float CurrentRandomSelectionWeight { get; private set; } = 0f;

    public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
    {
        // 1. Ensure the initiator and recipient are relevant to the player's colony
        if (!PositiveConnectionsUtility.ArePawnsRelevant(initiator, recipient))
        {
            return 0f;
        }
        
        // 1.5. Check if the recipient already has the associated Thought
        ThoughtDef comfortReceivedThought = ThoughtDef.Named("DIL_ComfortReceived");
        if (PositiveConnectionsUtility.HasLimitedMemories(recipient, comfortReceivedThought,
                PositiveConnectionsTuning.MaxMemoryStack_GiveComfort))
        {
            return 0f;
        }
        
        // 2. Conditions specific to GiveComfort
        // Check if the initiator has the required social skill or the Kind trait
        if (initiator.skills.GetSkill(SkillDefOf.Social).Level < PositiveConnectionsTuning.MinSocialSkillRequired_GiveComfort
            && !initiator.story.traits.HasTrait(TraitDefOf.Kind))
        {
            return 0f;
        }

        // Check if the recipient's mood is low enough
        if (recipient.needs.mood.CurLevelPercentage * 100 >= PositiveConnectionsTuning.MinMoodForComfort)
        {
            return 0f;
        }

        // 3. Calculate the base weight using the utility method
        float baseWeight = PositiveConnectionsUtility.CalculateBaseWeight(
            initiator,
            PositiveConnectionsTuning.BaseSelectionWeight_GiveComfort,
            _modSettings,
            applyMoodModifier: false);

        // 4. Apply any modifiers specific to GiveComfort
        // (No additional modifiers in this case)

        // 5. Adjust final weight based on faction
        float finalWeight = initiator.Faction == recipient.Faction
            ? baseWeight
            : baseWeight * PositiveConnectionsTuning.NonColonyPawnFactor_GiveComfort;

        // 6. Log the final weight if logging is enabled
        if (_modSettings.EnableLogging)
        {
           // string logMessage = $"<color=#00FF7F>[Positive Connections]</color> GiveComfortInteraction - Weight: {finalWeight} - Initiator: {initiator.Name.ToStringShort}, Recipient: {recipient.Name.ToStringShort}";
           // Log.Message(logMessage);
        }

        // 7. Return the final weight
        CurrentRandomSelectionWeight = finalWeight;
        return finalWeight;
    }

    public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks,
        out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
    {
        // Initialize output parameters to null
        letterText = null;
        letterLabel = null;
        letterDef = null;
        lookTargets = null;

        // Create a memory of the comforting interaction
        ThoughtDef thoughtDef = ThoughtDef.Named("DIL_ComfortReceived");
        recipient.needs.mood.thoughts.memories.TryGainMemory(thoughtDef, initiator);

        // Generate the comfort message using the utility method
        string comfortMessage = PositiveConnectionsUtility.GenerateComfortMessage(initiator, recipient);

        if (!_modSettings.DisableAllMessages)
        {
            Messages.Message(comfortMessage, new LookTargets(new Pawn[] { initiator, recipient }), MessageTypeDefOf.PositiveEvent);
        }

        OnPositiveInteraction?.Invoke(initiator, 0.1f, "PositiveInteraction", (int)ExperienceValency.Positive);

        // Enhanced logging without calling RandomSelectionWeight
        if (_modSettings.EnableLogging)
        {
            string logMessage = $"<color=#00FF7F>[Positive Connections]</color> GiveComfortInteraction - Weight: {CurrentRandomSelectionWeight} - Initiator: {initiator.Name.ToStringShort}, Recipient: {recipient.Name.ToStringShort}";
            Log.Message(logMessage);
        }
    }
}