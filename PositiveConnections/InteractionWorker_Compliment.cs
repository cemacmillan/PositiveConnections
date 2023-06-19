using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace PositiveConnections
{
    public class InteractionWorker_Compliment : InteractionWorker
    {


        private const float BaseSelectionWeight = 0.1f;

        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        { // If the recipient is not a colonist, return 0
            if (!recipient.IsColonist)
            {
                return 0f;
            }

            // If the pawns are rivals, return 0
            if (initiator.relations.OpinionOf(recipient) < -15 || recipient.relations.OpinionOf(initiator) < -15)
            {
                return 0f;
            }

            // If the worst opinion between the two pawns is less than -20, consider them as rivals and return 0
            if (initiator.relations.OpinionOf(recipient) < -20 || recipient.relations.OpinionOf(initiator) < -20)
            {
                return 0f;
            }

            
            return initiator.needs.mood.CurLevel * BaseSelectionWeight;
        }

        public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
        {
            // Perform the compliment interaction
            // This method is called when the compliment interaction is selected

            // Generate a compliment message based on the initiator's and recipient's characteristics
            string complimentMessage = GenerateComplimentMessage(initiator, recipient);

            // Add the compliment message to the conversation log or display it in a message window
            Messages.Message(complimentMessage, recipient, MessageTypeDefOf.PositiveEvent);

            // Increase the recipient's mood
            Thought_Memory memory = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDef.Named("ReceivedCompliment"));
            memory.moodPowerFactor = 1f;  // example of setting memory properties
            recipient.needs.mood.thoughts.memories.TryGainMemory(memory);

            // Set out parameters to null, they are not used in this interaction
            letterText = null;
            letterLabel = null;
            letterDef = null;
            lookTargets = null;

            // Update relationship between initiator and recipient, if necessary
            // For example, increase opinion or relationship stat
        }


        private string GenerateComplimentMessage(Pawn initiator, Pawn recipient)
        {
            // Generate a compliment message based on the initiator's and recipient's characteristics
            // You can use string interpolation or custom logic to create personalized compliments

            // Choose a random compliment
            string[] compliments = { "social skills", "hard work", "bravery", "intelligence", "grace",
            "patience"};
            string randomCompliment = compliments.RandomElement();

            return $"{initiator.Name} compliments {recipient.Name} on their {randomCompliment}!";
        }
    }
}
