using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace DIL_PositiveConnections
{
    public class InteractionWorker_Compliment : InteractionWorker
    {
        public static event Action<Pawn, float, string, int> OnPositiveInteraction;

        // Reduced the base selection weight by about 1/3
        private const float BaseSelectionWeight = 0.085f; // Previously 0.125f

        PositiveConnectionsModSettings modSettings = PositiveConnections.Instance.GetSettings<PositiveConnectionsModSettings>();

  
          public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
          {
              // Check if the pawns are relevant (player-controlled)
              if (!PositiveConnectionsUtility.ArePawnsRelevant(initiator, recipient))
              {
                  return 0f;
              }

              // Additional condition specific to Compliment: Opinion > -15
              if (initiator.relations.OpinionOf(recipient) < -15 || recipient.relations.OpinionOf(initiator) < -15)
              {
                  return 0f;
              }

              // Calculate the base weight
              float baseWeight = PositiveConnectionsUtility.CalculateBaseWeight(initiator, PositiveConnectionsTuning.BaseSelectionWeight_Compliment);

              // Adjust for same-faction interactions
              float finalWeight = initiator.Faction == recipient.Faction ? baseWeight : baseWeight * PositiveConnectionsTuning.NonColonyPawnFactor;

              return finalWeight;
          }
  
        public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
        {
            string complimentMessage = PositiveConnectionsUtility.GenerateComplimentMessage(initiator, recipient);

            if (!modSettings.DisableAllMessages)
            {
                Messages.Message(complimentMessage, recipient, MessageTypeDefOf.PositiveEvent);
            }

            Thought_Memory memory = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDef.Named("DIL_ReceivedCompliment"));
            memory.moodPowerFactor = 1f;
            recipient.needs.mood.thoughts.memories.TryGainMemory(memory, initiator);

            letterText = null;
            letterLabel = null;
            letterDef = null;
            lookTargets = null;

            // Update relationship between initiator and recipient
            Faction factionA = initiator.Faction;
            Faction factionB = recipient.Faction;

            if (factionA != null && factionB != null && factionA != factionB)
            {
                PositiveConnectionsUtility.ChangeFactionRelations(factionA, factionB, 10);
            }

            OnPositiveInteraction?.Invoke(initiator, 0.05f, "PositiveInteraction", (int)ExperienceValency.Positive);

            // New logging
            if (modSettings.EnableLogging)
            {
                string logMessage = $"<color=#00FF7F>[Positive Connections]</color> ComplimentInteraction - Weight: {RandomSelectionWeight(initiator, recipient)} - Initiator: {initiator.Name.ToStringShort}, Recipient: {recipient.Name.ToStringShort}";
                Log.Message(logMessage);
            }
        }
    }
}