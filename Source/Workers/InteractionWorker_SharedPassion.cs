using System;
using System.Linq;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PositiveConnectionsNmSpc;

public class InteractionWorker_SharedPassion : InteractionWorker
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
        
        // 1.5. Check if the recipient already has the applicable Thought
        ThoughtDef sharedPassionThought = ThoughtDef.Named("DIL_SharedPassionActivity");
        if (PositiveConnectionsUtility.HasMemory(recipient, sharedPassionThought))
        {
            return 0f;
        }

        // 2. Conditions specific to Shared Passion
        // Both pawns should have a shared skill with passion
        var sharedPassionSkill = initiator.skills.skills
            .FirstOrDefault(s =>
                (int)s.passion > 0 &&
                recipient.skills.GetSkill(s.def) != null &&
                (int)recipient.skills.GetSkill(s.def).passion > 0);

        if (sharedPassionSkill == null)
        {
            return 0f;
        }

        // 3. Calculate the base weight
        float baseWeight = PositiveConnectionsUtility.CalculateBaseWeight(
            initiator,
            PositiveConnectionsTuning.BaseSelectionWeight_SharedPassion,
            _modSettings,
            applyMoodModifier: true);

        // 4. Apply modifiers specific to Shared Passion
        // Increase the weight based on passion level
        int passionLevelInitiator = (int)sharedPassionSkill.passion;
        int passionLevelRecipient = (int)recipient.skills.GetSkill(sharedPassionSkill.def).passion;

        // Use the minimum passion level between the two pawns
        int sharedPassionLevel = Math.Min(passionLevelInitiator, passionLevelRecipient);

        float weight = baseWeight * (sharedPassionLevel * PositiveConnectionsTuning.PassionLevelFactor_SharedPassion);

        // 5. Adjust final weight based on faction
        float finalWeight = initiator.Faction == recipient.Faction
            ? weight
            : weight * PositiveConnectionsTuning.NonColonyPawnFactor_SharedPassion;
        
        // 6. Return the final weight
        CurrentRandomSelectionWeight = finalWeight;
        return finalWeight;
    }

    public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks,
        out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
    {
        // Find the shared passion skill
        var sharedPassionSkill = initiator.skills.skills
            .FirstOrDefault(s =>
                (int)s.passion > 0 &&
                recipient.skills.GetSkill(s.def) != null &&
                (int)recipient.skills.GetSkill(s.def).passion > 0);

        if (sharedPassionSkill != null)
        {
            // Generate a shared passion message using the utility method
            string passionMessage = PositiveConnectionsUtility.GenerateSharedPassionMessage(initiator, recipient, sharedPassionSkill);

            if (!_modSettings.DisableAllMessages)
            {
                Messages.Message(passionMessage, recipient, MessageTypeDefOf.PositiveEvent);
            }

            // Create and gain memory of the shared activity
            Thought_Memory memoryInitiator = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDef.Named("DIL_SharedPassionActivity"));
            Thought_Memory memoryRecipient = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDef.Named("DIL_SharedPassionActivity"));

            memoryInitiator.moodPowerFactor = 1f;
            memoryRecipient.moodPowerFactor = 1f;

            initiator.needs.mood.thoughts.memories.TryGainMemory(memoryInitiator, recipient);
            recipient.needs.mood.thoughts.memories.TryGainMemory(memoryRecipient, initiator);

            // Update relationship between initiator and recipient
            Faction factionA = initiator.Faction;
            Faction factionB = recipient.Faction;

            if (factionA != null && factionB != null && factionA != factionB)
            {
                // Adjust faction relations based on the memory's mood power factor
                int relationImpact = (int)(memoryInitiator.moodPowerFactor * 10);
                PositiveConnectionsUtility.ChangeFactionRelations(factionA, factionB, relationImpact);
            }

            OnPositiveInteraction?.Invoke(initiator, 0.2f, "PositiveInteraction", (int)ExperienceValency.Positive);

            // Enhanced logging
            if (_modSettings.EnableLogging)
            {
                string logMessage = $"<color=#00FF7F>[Positive Connections]</color> SharedPassionInteraction - Weight: {CurrentRandomSelectionWeight} - Initiator: {initiator.Name.ToStringShort}, Recipient: {recipient.Name.ToStringShort}, SharedSkill: {sharedPassionSkill.def.defName}";
                Log.Message(logMessage);
            }
        }

        letterText = null;
        letterLabel = null;
        letterDef = null;
        lookTargets = null;
    }
}