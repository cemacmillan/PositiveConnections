using System.Collections.Generic;
using DIL_PositiveConnections;
using RimWorld;
using UnityEngine;
using Verse;

namespace DIL_PositiveConnnections
{
    public class InteractionWorker_DiscussIdeoligion : InteractionWorker
    {
        PositiveConnectionsModSettings modSettings = Mod_PositiveConnections.Instance.GetSettings<PositiveConnectionsModSettings>();

        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            // If both pawns have the same ideoligion, return the base selection weight
            if (initiator.Ideo == recipient.Ideo)
            {
                return 0.025f; // Adjust this weight according to your needs
            }

            // If they have different ideoligions, return zero to prevent this interaction
            return 0.0f;
        }

        public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
        {
            // Define the ThoughtDef for a positive ideoligion discussion
            ThoughtDef positiveDiscussion = ThoughtDef.Named("DIL_PositiveIdeoligionDiscussion");

            // Add the memory to both pawns
            initiator.needs?.mood?.thoughts?.memories?.TryGainMemory((Thought_Memory)ThoughtMaker.MakeThought(positiveDiscussion), recipient);
            recipient.needs?.mood?.thoughts?.memories?.TryGainMemory((Thought_Memory)ThoughtMaker.MakeThought(positiveDiscussion), initiator);

            // Set the message text
            string message = "A positive discussion about ideoligion has taken place.";

            if(!modSettings.DisableAllMessages) {

                // Show the message to the recipient
                Messages.Message(message, recipient, MessageTypeDefOf.PositiveEvent);
            }
           

            // Set the other output variables to null or default values
            letterText = null;
            letterLabel = null;
            letterDef = null;
            lookTargets = null;
        }
    }
}
