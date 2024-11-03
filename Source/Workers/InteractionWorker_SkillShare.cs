using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace PositiveConnectionsNmSpc;

public class InteractionWorker_SkillShare : InteractionWorker
{
    public static event Action<Pawn, float, string, int> OnPositiveInteraction;

    private PositiveConnectionsModSettings _modSettings =
        PositiveConnections.Instance.GetSettings<PositiveConnectionsModSettings>();

    // Encapsulated property for the last computed selection weight
    public float CurrentRandomSelectionWeight { get; private set; } = 0f;

public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
{
    // Ensure the initiator and recipient are relevant to the player's colony
    if (!PositiveConnectionsUtility.ArePawnsRelevant(initiator, recipient))
    {
        //Log.Message("SkillShare: Initiator or recipient is not relevant to the player's colony.");
        return 0f;
    }

    // Check if the recipient already has the associated Thought
    ThoughtDef receivedTeachingThought = ThoughtDef.Named("DIL_ReceivedTeaching");
    if (PositiveConnectionsUtility.HasMemory(recipient, receivedTeachingThought))
    {
        //Log.Message("SkillShare: Recipient already has the associated 'ReceivedTeaching' thought.");
        return 0f;
    }

    // Get the highest skill of the initiator
    var highestSkill = initiator.skills.skills.MaxBy(s => s.Level);
    if (highestSkill.Level < PositiveConnectionsTuning.MinSkillLevel_SkillShare)
    {
        // Log.Message($"SkillShare: Initiator's skill level in {highestSkill.def.defName} is below the minimum required ({PositiveConnectionsTuning.MinSkillLevel_SkillShare}).");
        return 0f;
    }

    // Get the recipient's skill in the same area
    var recipientSkill = recipient.skills.GetSkill(highestSkill.def);

    // If the recipient cannot profit from the skill, don't bother
    if (!PositiveConnectionsUtility.CanReceiveSkillShare(recipient, recipientSkill.def, PositiveConnectionsTuning.MinSkillLevel_SkillShareRecipient))
    {
        //Log.Message($"SkillShare: Recipient's skill level in {recipientSkill.def.defName} is below the minimum level to benefit from skill sharing ({PositiveConnectionsTuning.MinSkillLevel_SkillShareRecipient}).");
        return 0f;
    }

    // If the recipient's skill level is too close to the initiator's, skip interaction
    if (recipientSkill.Level > highestSkill.Level - 1)
    {
        // Log.Message($"SkillShare: Recipient's skill level in {recipientSkill.def.defName} is too close to the initiator's skill level.");
        return 0f;
    }

    // Calculate the base weight with the adjusted divisor
    float baseWeight = PositiveConnectionsUtility.CalculateBaseWeight(
        initiator,
        PositiveConnectionsTuning.BaseSelectionWeight_SkillShare,
        _modSettings);

    // Determine skill difference and apply the divisor
    var skillDifference = highestSkill.Level - recipientSkill.Level;
    float weight = (skillDifference / PositiveConnectionsTuning.SkillDifferenceDivisor_SkillShare) * baseWeight;

    // Log the calculated weight and skill difference
    // Log.Message($"SkillShare: Calculated weight before conditional adjustments is {weight}. Skill difference: {skillDifference}, Divisor: {PositiveConnectionsTuning.SkillDifferenceDivisor_SkillShare}");

    // Apply conditional boosts and dampening
    if (highestSkill.Level >= 8 && recipientSkill.Level < 6)
    {
        weight *= 1.5f;
        // Log.Message("SkillShare: Applied boost for significant skill gap (initiator >= 8 and recipient < 6).");
    }
    else if (recipientSkill.Level >= highestSkill.Level - 2)
    {
        weight *= 0.5f;
        // Log.Message("SkillShare: Applied dampening for near-peer skill levels (recipient within 2 levels of initiator).");
    }

    // Ensure weight is positive
    if (weight <= 0f)
    {
        // Log.Message("SkillShare: Final weight is zero or negative after adjustments.");
        return 0f;
    }

    // Final weight based on faction
    float finalWeight = initiator.Faction == recipient.Faction
        ? weight
        : weight * PositiveConnectionsTuning.NonColonyPawnFactor_SkillShare;

    // Log the final weight
    // Log.Message($"SkillShare: Final calculated weight is {finalWeight}. Initiator: {initiator.Name.ToStringShort}, Recipient: {recipient.Name.ToStringShort}");

    // Store the final weight for logging or later use
    CurrentRandomSelectionWeight = finalWeight;
    return finalWeight;
}

    public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks,
        out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
    {
        var highestSkill = initiator.skills.skills.MaxBy(s => s.Level);

        if (highestSkill != null)
        {
            recipient.skills.GetSkill(highestSkill.def).Learn(200f);

            // Use the new utility method to generate the message
            var teachingMessage =
                PositiveConnectionsUtility.GenerateSkillShareMessage(initiator, recipient, highestSkill.def);

            if (!_modSettings.DisableAllMessages)
                Messages.Message(teachingMessage, recipient, MessageTypeDefOf.PositiveEvent);

            // Apply the thought
            recipient.needs.mood.thoughts.memories.TryGainMemory(PositiveConnectionsThoughtDefOf.DIL_ReceivedTeaching,
                initiator);

            OnPositiveInteraction?.Invoke(initiator, 0.2f, "PositiveInteraction", (int)ExperienceValency.Positive);

            // New logging
            if (_modSettings.EnableLogging)
            {
                var logMessage =
                    $"<color=#00FF7F>[Positive Connections]</color> SkillSharingInteraction - Weight: {CurrentRandomSelectionWeight} - Initiator: {initiator.Name.ToStringShort}, Recipient: {recipient.Name.ToStringShort}, Skill: {highestSkill.def.defName}";
                Log.Message(logMessage);
            }
        }

        letterText = null;
        letterLabel = null;
        letterDef = null;
        lookTargets = null;
    }
}