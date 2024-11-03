using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace PositiveConnectionsNmSpc;

public class InteractionWorker_DiscussIdeoligion : InteractionWorker
{
    // Instance variable for mod settings
    private PositiveConnectionsModSettings _modSettings =
        PositiveConnections.Instance.GetSettings<PositiveConnectionsModSettings>();

    // Event for positive interaction
    public static event Action<Pawn, float, string, int> OnPositiveInteraction;

    // Encapsulated property for the last computed selection weight
    public float CurrentRandomSelectionWeight { get; private set; } = 0f;

    public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
    {
        // Ensure the initiator and recipient are relevant to the player's colony
        if (!PositiveConnectionsUtility.ArePawnsRelevant(initiator, recipient))
        {
            //Log.Message("DiscussIdeoligion: Initiator or recipient is not relevant to the player's colony.");
            return 0f;
        }

        // Check if the recipient already has the "DIL_IdeologicalDiscussion" Thought
        ThoughtDef ideologicalDiscussionThought = ThoughtDef.Named("DIL_IdeologicalDiscussion");
        if (PositiveConnectionsUtility.HasMemory(recipient, ideologicalDiscussionThought))
        {
            //Log.Message("DiscussIdeoligion: Recipient already has the associated 'DIL_IdeologicalDiscussion' thought.");
            return 0f;
        }

        // Ensure at least one pawn has sufficient Intellectual skill for the discussion
        if (initiator.skills.GetSkill(SkillDefOf.Intellectual).Level < PositiveConnectionsTuning.MinIntellectualSkillRequired_IdeologicalDiscussion &&
            recipient.skills.GetSkill(SkillDefOf.Intellectual).Level < PositiveConnectionsTuning.MinIntellectualSkillRequired_IdeologicalDiscussion)
        {
            //Log.Message(
              //  $"DiscussIdeoligion: Neither initiator nor recipient has Intellectual skill above the required minimum ({PositiveConnectionsTuning.MinIntellectualSkillRequired_IdeologicalDiscussion}).");
            return 0f;
        }

        // Ensure both pawns have Social capability
        if (initiator.skills.GetSkill(SkillDefOf.Social).TotallyDisabled ||
            recipient.skills.GetSkill(SkillDefOf.Social).TotallyDisabled)
        {
            // Log.Message("DiscussIdeoligion: Either the initiator or recipient has Social skill totally disabled.");
            return 0f;
        }

        // Calculate the base weight using the utility method
        float baseWeight = PositiveConnectionsUtility.CalculateBaseWeight(
            initiator,
            PositiveConnectionsTuning.BaseSelectionWeight_DiscussIdeoligion,
            _modSettings);

        // Apply modifiers specific to DiscussIdeoligion
        float weight = baseWeight;

        // Check if Ideology DLC is active and adjust weight accordingly
        if (ModsConfig.IdeologyActive)
        {
            Ideo initiatorIdeo = initiator.Ideo;
            Ideo recipientIdeo = recipient.Ideo;

            if (initiatorIdeo == null || recipientIdeo == null)
            {
                //Log.Message("DiscussIdeoligion: Either initiator or recipient has no Ideology defined.");
                return 0f;
            }

            // Apply weight adjustment based on Ideo match
            if (initiatorIdeo != recipientIdeo)
            {
                weight *= PositiveConnectionsTuning.DifferentIdeoFactor_DiscussIdeoligion;
                // Log.Message("DiscussIdeoligion: Initiator and recipient have different Ideologies. Adjusting weight.");
            }
        }
        else
        {
            // Apply weight adjustment if Ideology DLC is not active
            weight *= PositiveConnectionsTuning.NoIdeologyFactor_DiscussIdeoligion;
            //Log.Message("DiscussIdeoligion: Ideology DLC is inactive. Adjusting weight for non-ideology gameplay.");
        }

        // Final weight adjustment based on faction if applicable
        // Uncomment if non-colony pawn factor is required
        /*
        if (initiator.Faction != recipient.Faction)
        {
            weight *= PositiveConnectionsTuning.NonColonyPawnFactor_DiscussIdeoligion;
            Log.Message("DiscussIdeoligion: Adjusted weight for non-colony interaction.");
        }
        */

        // Log the final weight for transparency
        //Log.Message(
        //  $"DiscussIdeoligion: Final calculated weight is {weight}. Initiator: {initiator.Name.ToStringShort}, Recipient: {recipient.Name.ToStringShort}");

        // Store the final weight for logging or later use
        CurrentRandomSelectionWeight = weight;
        return weight;
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
        // Initialize output parameters
        letterText = null;
        letterLabel = null;
        letterDef = null;
        lookTargets = null;

        // Determine which thought to use
        ThoughtDef thoughtDef = ThoughtDef.Named("DIL_IdeologicalDiscussion");

        // Add the memory to both pawns
        initiator.needs?.mood?.thoughts?.memories?.TryGainMemory(thoughtDef, recipient);
        recipient.needs?.mood?.thoughts?.memories?.TryGainMemory(thoughtDef, initiator);

        // Generate the message
        string message = PositiveConnectionsUtility.GenerateIdeologicalDiscussionMessage(initiator, recipient);

        if (!_modSettings.DisableAllMessages)
        {
            Messages.Message(message, recipient, MessageTypeDefOf.PositiveEvent);
        }

        // Invoke the positive interaction event
        OnPositiveInteraction?.Invoke(initiator, 0.1f, "PositiveInteraction", (int)ExperienceValency.Positive);

        // Enhanced logging without calling RandomSelectionWeight
        if (_modSettings.EnableLogging)
        {
            string logMessage =
                $"<color=#00FF7F>[Positive Connections]</color> DiscussIdeoligionInteraction - Weight: {CurrentRandomSelectionWeight} - Initiator: {initiator.Name.ToStringShort}, Recipient: {recipient.Name.ToStringShort}";
            Log.Message(logMessage);
        }
    }
}