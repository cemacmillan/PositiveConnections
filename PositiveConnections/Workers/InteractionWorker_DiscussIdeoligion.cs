using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace DIL_PositiveConnections
{
    public class InteractionWorker_DiscussIdeoligion : InteractionWorker
    {
        PositiveConnectionsModSettings modSettings = PositiveConnections.Instance.GetSettings<PositiveConnectionsModSettings>();

        public static event Action<Pawn, float, string, int> OnPositiveInteraction;

        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            // Ensure the initiator and recipient are relevant to the player's colony
            if (!PositiveConnectionsUtility.ArePawnsRelevant(initiator, recipient))
            {
                return 0f;
            }

            // Check for null ideology
            if (initiator.Ideo == null || recipient.Ideo == null)
            {
                return 0f;
            }

            // Calculate the base weight using the utility method and tuning class
            float baseWeight = PositiveConnectionsUtility.CalculateBaseWeight(initiator, PositiveConnectionsTuning.BaseSelectionWeight_DiscussIdeoligion, modSettings);

            // Adjust based on ideology match
            float finalWeight = initiator.Ideo == recipient.Ideo ? baseWeight : baseWeight * 0.2f;

            return finalWeight;
        }

        public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
        {
            // Define the ThoughtDef for a positive ideoligion discussion
            ThoughtDef positiveDiscussion = ThoughtDef.Named("DIL_IdeologicalDiscussion");

            // Add the memory to both pawns
            initiator.needs?.mood?.thoughts?.memories?.TryGainMemory((Thought_Memory)ThoughtMaker.MakeThought(positiveDiscussion), recipient);
            recipient.needs?.mood?.thoughts?.memories?.TryGainMemory((Thought_Memory)ThoughtMaker.MakeThought(positiveDiscussion), initiator);

            // Create a narrative message for the player
            string message = $"{initiator.LabelShort} and {recipient.LabelShort} had a positive discussion about ideoligions.";

            if (!modSettings.DisableAllMessages)
            {
                // Show the message to the recipient
                Messages.Message(message, recipient, MessageTypeDefOf.PositiveEvent);
            }

            // Set the other output variables to null or default values
            letterText = null;
            letterLabel = null;
            letterDef = null;
            lookTargets = null;

            OnPositiveInteraction?.Invoke(initiator, 0.1f, "PositiveInteraction", (int)ExperienceValency.Positive);

            // New logging
            if (modSettings.EnableLogging)
            {
                string logMessage = $"<color=#00FF7F>[Positive Connections]</color> DiscussIdeoligion - Weight: {RandomSelectionWeight(initiator, recipient)} - Initiator: {initiator.Name.ToStringShort}, Recipient: {recipient.Name.ToStringShort}";
                Log.Message(logMessage);
            }
        }
    }
}