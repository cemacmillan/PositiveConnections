using System;
using System.Collections.Generic;
using Verse;
using RimWorld;

namespace DIL_PositiveConnections
{
   public class InteractionWorker_Gift : InteractionWorker
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

           // condition specific to Gift: Opinion > -5
           if (initiator.relations.OpinionOf(recipient) < -5 || recipient.relations.OpinionOf(initiator) < -5)
           {
               return 0f;
           }

           // Calculate the base weight
           float baseWeight = PositiveConnectionsUtility.CalculateBaseWeight(initiator, PositiveConnectionsTuning.BaseSelectionWeight_Gift, modSettings, applyMoodModifier: true);

           // Adjust to reflect the fact that prettier people get more free stuff statistically
           float prettiness = recipient.GetStatValue(StatDefOf.PawnBeauty);
           if (prettiness < 0f)
           {
               baseWeight *= 0.5f;  // Reduced unattractive factor
           }
           else if (prettiness > 0f)
           {
               baseWeight *= 1.5f;  // Increased attractive factor
           }

         
           float finalWeight = initiator.Faction == recipient.Faction ? baseWeight : baseWeight * 0.2f; // 1/5 as likely for non-colony interactions

           return finalWeight;
       }

       public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
       {
           string giftDescription = PositiveConnectionsUtility.GenerateGiftDescription(initiator, recipient);

           if (!modSettings.DisableAllMessages) 
           {
               Messages.Message(giftDescription, recipient, MessageTypeDefOf.PositiveEvent);
           }

           Thought_Memory memory = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDef.Named("DIL_ReceivedGift"));
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

           OnPositiveInteraction?.Invoke(initiator, 0.25f, "PositiveInteraction", (int)ExperienceValency.Positive);

           // New logging
           if (modSettings.EnableLogging)
           {
               string logMessage = $"<color=#00FF7F>[Positive Connections]</color> GiftInteraction - Weight: {RandomSelectionWeight(initiator, recipient)} - Initiator: {initiator.Name.ToStringShort}, Recipient: {recipient.Name.ToStringShort}";
               Log.Message(logMessage);
           }
       }
   }
}