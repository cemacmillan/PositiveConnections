using System;
using System.Collections.Generic;
using dIl_PositiveConnections;
using RimWorld;
using Verse;

namespace PositiveConnections
{
    public class InteractionWorker_Compliment : InteractionWorker
    {
        private const float BaseSelectionWeight = 0.020f;

        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            if (initiator.relations.OpinionOf(recipient) < -15 || recipient.relations.OpinionOf(initiator) < -15)
            {
                return 0f;
            }

            float baseWeight = initiator.needs.mood.CurLevel * BaseSelectionWeight;

            if (initiator.Faction == recipient.Faction)
            {
                return baseWeight;
            }
            else
            {
                return baseWeight * 0.2f; // 1/5 as likely
            }
        }


        public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
        {
            string complimentMessage = GenerateComplimentMessage(initiator, recipient);

            Messages.Message(complimentMessage, recipient, MessageTypeDefOf.PositiveEvent);

            Thought_Memory memory = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDef.Named("ReceivedCompliment"));
            memory.moodPowerFactor = 1f;
            recipient.needs.mood.thoughts.memories.TryGainMemory(memory);

            letterText = null;
            letterLabel = null;
            letterDef = null;
            lookTargets = null;

            // Update relationship between initiator and recipient
            Faction factionA = initiator.Faction;
            Faction factionB = recipient.Faction;

            if (factionA != null && factionB != null && factionA != factionB)
            {
                FactionRelationsChanger.ChangeFactionRelations(factionA, factionB, 10);
            }
        }

        private string GenerateComplimentMessage(Pawn initiator, Pawn recipient)
        {
            string[] compliments = { "social skills", "hard work", "bravery", "intelligence", "grace", "patience" };
            string randomCompliment = compliments.RandomElement();

            return $"{initiator.Name} compliments {recipient.Name} on their {randomCompliment}!";
        }
    }
}
