using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace PositiveConnectionsNmSpc;

public class InteractionWorker_Mediation : InteractionWorker
{
    // Event for positive interaction
    public static event Action<Pawn, float, string, int> OnPositiveInteraction;

    // Instance variable for mod settings
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
        ThoughtDef successfulMediationThought = ThoughtDef.Named("DIL_SuccessfulMediation");
        if (PositiveConnectionsUtility.HasMemory(recipient, successfulMediationThought))
        {
            return 0f;
        }
            
        // 2. Conditions specific to Mediation
        // The initiator must have the required social skill
        /*if (initiator.skills.GetSkill(SkillDefOf.Social).Level < PositiveConnectionsTuning.MinSocialSkillRequired_Mediation)
        {
            return 0f;
        }*/
        
        // more complete test for mediation requirements including Social skill
        if (!PositiveConnectionsUtility.IsSuitedForMediation(initiator))
        {
            return 0f;
        }
        
        // The initiator and recipient must be in the same faction
        if (initiator.Faction != recipient.Faction)
        {
            return 0f;
        }

        // 3. Check for potential conflicts
        // Get the list of all colony pawns
        IEnumerable<Pawn> colonyPawns = recipient.Map.mapPawns.FreeColonists;

        // Look for potential conflicts that the initiator could mediate
        bool conflictExists = colonyPawns.Any(pawn =>
            recipient != pawn &&
            recipient.relations.OpinionOf(pawn) < PositiveConnectionsTuning.NegativeRelationshipThreshold_Mediation);

        if (!conflictExists)
        {
            // If there are no potential conflicts, return a weight of zero
            return 0f;
        }

        // 4. Calculate the base weight using the utility method
        float baseWeight = PositiveConnectionsUtility.CalculateBaseWeight(
            initiator,
            PositiveConnectionsTuning.BaseSelectionWeight_Mediation,
            _modSettings,
            applyMoodModifier: false);

        // 5. Apply any modifiers specific to Mediation
        // (No additional modifiers in this case)

        // 6. Adjust final weight based on faction (if applicable)
        // (Already ensured initiator and recipient are in the same faction)
        
        // 7. Return the final weight
        CurrentRandomSelectionWeight = baseWeight;
        return baseWeight;
    }

    public override void Interacted(
        Pawn initiator,
        Pawn recipient,
        List<RulePackDef> extraSentencePacks,
        out string letterText,
        out string letterLabel,
        out LetterDef letterDef,
        out LookTargets lookTargets)
    {
        // Initialize output parameters to null
        letterText = null;
        letterLabel = null;
        letterDef = null;
        lookTargets = null;

        // Locate a suitable conflict for the initiator to mediate
        Pawn conflictPawn = FindConflictPawn(recipient);

        // If no suitable conflict pawn is found, return early
        if (conflictPawn == null)
        {
            return;
        }

        // Calculate the outcome based on the initiator's social skill
        int mediationBonus = CalculateMediationBonus(initiator);

        // Apply the mediation bonus to the conflict pawn and recipient
        ApplyMediationOutcome(initiator, recipient, conflictPawn, mediationBonus);

        // Generate a mediation message using the utility method
        string mediationMessage = PositiveConnectionsUtility.GenerateMediationMessage(initiator, recipient, conflictPawn);

        if (!_modSettings.DisableAllMessages)
        {
            Messages.Message(mediationMessage, new LookTargets(new Pawn[] { initiator, recipient, conflictPawn }), MessageTypeDefOf.PositiveEvent);
        }

        OnPositiveInteraction?.Invoke(initiator, 0.25f, "PositiveInteraction", (int)ExperienceValency.Positive);

        // Enhanced logging without calling RandomSelectionWeight
        if (_modSettings.EnableLogging)
        {
            string logMessage = $"<color=#00FF7F>[Positive Connections]</color> MediationInteraction - Weight: {CurrentRandomSelectionWeight} - Initiator: {initiator.Name.ToStringShort}, Recipient: {recipient.Name.ToStringShort}, ConflictPawn: {conflictPawn.Name.ToStringShort}, MediationBonus: {mediationBonus}";
            Log.Message(logMessage);
        }
    }

    private Pawn FindConflictPawn(Pawn recipient)
    {
        IEnumerable<Pawn> colonyPawns = recipient.Map.mapPawns.FreeColonists;

        foreach (Pawn pawn in colonyPawns)
        {
            if (recipient != pawn && recipient.relations.OpinionOf(pawn) < PositiveConnectionsTuning.NegativeRelationshipThreshold_Mediation)
            {
                return pawn;
            }
        }

        return null;
    }

    private int CalculateMediationBonus(Pawn initiator)
    {
        int socialSkill = initiator.skills.GetSkill(SkillDefOf.Social).Level;
        float randomFactor = Rand.Value;

        int mediationBonus = (int)(
            PositiveConnectionsTuning.MinMediationBonus +
            (PositiveConnectionsTuning.MaxMediationBonus - PositiveConnectionsTuning.MinMediationBonus) *
            randomFactor * socialSkill / 20f);

        return mediationBonus;
    }

    private void ApplyMediationOutcome(Pawn initiator, Pawn recipient, Pawn conflictPawn, int mediationBonus)
    {
        int convertedBonus = (int)Mathf.Round(mediationBonus / (PositiveConnectionsTuning.MaxMediationBonus / 2f));
        convertedBonus = Mathf.Clamp(convertedBonus, 0, 2);

        ThoughtDef thoughtDef = ThoughtDef.Named("DIL_SuccessfulMediation");

        Thought_Memory memoryInitiator = (Thought_Memory)ThoughtMaker.MakeThought(thoughtDef);
        Thought_Memory memoryRecipient = (Thought_Memory)ThoughtMaker.MakeThought(thoughtDef);
        Thought_Memory memoryConflictPawn = (Thought_Memory)ThoughtMaker.MakeThought(thoughtDef);

        // Set the forced stage based on the converted bonus
        memoryInitiator.SetForcedStage(convertedBonus);
        memoryRecipient.SetForcedStage(convertedBonus);
        memoryConflictPawn.SetForcedStage(convertedBonus);

        // Add the memory to each pawn
        initiator.needs.mood.thoughts.memories.TryGainMemory(memoryInitiator);
        recipient.needs.mood.thoughts.memories.TryGainMemory(memoryRecipient);
        conflictPawn.needs.mood.thoughts.memories.TryGainMemory(memoryConflictPawn);

        // Create and add the social memory between recipient and conflictPawn
        Thought_MemorySocial memoryShared = (Thought_MemorySocial)ThoughtMaker.MakeThought(PositiveConnectionsThoughtDefOf.DIL_InMediationWith);
        recipient.needs.mood.thoughts.memories.TryGainMemory(memoryShared, conflictPawn);
        conflictPawn.needs.mood.thoughts.memories.TryGainMemory(memoryShared, recipient);
    }
}