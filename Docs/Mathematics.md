Sure, let's outline the mathematical basis for the probabilities of different interactions in Positive Connections. Here is a detailed explanation along with the markdown document as requested.

## The Mathematics of Positive Connections

### Overview

Positive Connections is designed to enhance social interactions among pawns in RimWorld by introducing various positive interactions. Each interaction type has a probability of occurring, influenced by factors such as pawn traits, mood, and social skill levels. This document explains the mathematical basis for these probabilities to help users and modders understand how the mod influences the game.

### Interaction Types

Positive Connections includes the following interaction types:
1. Compliments
2. Small Virtual Gifts
3. Sharing of Passion
4. Discuss Ideology
5. Skill Sharing
6. Mediation
7. Comfort Giving
8. Storytelling

### General Probability Calculation

Each interaction has a `RandomSelectionWeight` method that determines its likelihood of occurring. The base probability is modified by various factors, including:
- Base Selection Weight
- Mood Levels
- Social Skills
- Specific Conditions for Each Interaction

The formula for the selection weight of each interaction can be generally described as:

\[ \text{Weight} = \text{Base Selection Weight} \times \text{Mood Factor} \times \text{Skill Factor} \times \text{Interaction Frequency} \]

### Interaction Probabilities

#### 1. Compliments

**RandomSelectionWeight Method:**
```csharp
public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
{
    if (initiator.relations.OpinionOf(recipient) < -15 || recipient.relations.OpinionOf(initiator) < -15)
    {
        return 0f;
    }

    if (initiator.Faction != Faction.OfPlayer && recipient.Faction != Faction.OfPlayer)
    {
        return 0f;
    }

    float baseWeight = initiator.needs.mood.CurLevel * BaseSelectionWeight * modSettings.BaseInteractionFrequency;

    if (initiator.Faction == recipient.Faction)
    {
        return baseWeight;
    }
    else
    {
        return baseWeight * 0.2f; // 1/5 as likely
    }
}
```
- **Base Selection Weight:** 0.0125
- **Mood Factor:** Current mood level of the initiator
- **Faction Adjustment:** 1.0 if same faction, 0.2 if different faction

#### 2. Small Virtual Gifts

**RandomSelectionWeight Method:**
```csharp
public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
{
    if (initiator.Faction != Faction.OfPlayer && recipient.Faction != Faction.OfPlayer)
    {
        return 0f;
    }

    if (initiator.relations.OpinionOf(recipient) < -5 || recipient.relations.OpinionOf(initiator) < -5)
    {
        return 0f;
    }

    float baseWeight = initiator.needs.mood.CurLevel * BaseSelectionWeight * modSettings.BaseInteractionFrequency;
    float prettiness = recipient.GetStatValue(StatDefOf.PawnBeauty);

    if (prettiness < 0f)
    {
        baseWeight *= 0.3f; // found unattractive factor
    }
    else if (prettiness > 0f)
    {
        baseWeight *= 2.3f; // found attractive factor
    }
    else
    {
        baseWeight *= 1f; // average factor
    }

    if (initiator.Faction == recipient.Faction)
    {
        return baseWeight;
    }
    else
    {
        return baseWeight * 0.2f; // 1/5 as likely
    }
}
```
- **Base Selection Weight:** 0.0050
- **Mood Factor:** Current mood level of the initiator
- **Beauty Adjustment:** 0.3 if unattractive, 2.3 if attractive, 1.0 if average
- **Faction Adjustment:** 1.0 if same faction, 0.2 if different faction

#### 3. Sharing of Passion

**RandomSelectionWeight Method:**
```csharp
public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
{
    if (initiator.Faction != Faction.OfPlayer && recipient.Faction != Faction.OfPlayer)
    {
        return 0f;
    }

    var sharedPassion = initiator.skills.skills
        .FirstOrDefault(s => recipient.skills.skills.Any(rs => rs.def == s.def && (int)rs.passion > 0 && (int)s.passion > 0));

    if (sharedPassion == null)
    {
        return 0f;
    }

    float baseWeight = (initiator.needs.mood.CurLevel + recipient.needs.mood.CurLevel) / 2 * BaseSelectionWeight;

    if (initiator.Faction == recipient.Faction)
    {
        return baseWeight * ((int)sharedPassion.passion * PassionLevelFactor) * modSettings.BaseInteractionFrequency;
    }
    else
    {
        return baseWeight * NonColonyPawnFactor * modSettings.BaseInteractionFrequency;
    }
}
```
- **Base Selection Weight:** 0.0025
- **Mood Factor:** Average mood level of the initiator and recipient
- **Passion Level Factor:** Multiplied by the passion level
- **Faction Adjustment:** 1.0 if same faction, 0.05 if different faction

#### 4. Discuss Ideology

**RandomSelectionWeight Method:**
```csharp
public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
{
    if (initiator.Faction != Faction.OfPlayer && recipient.Faction != Faction.OfPlayer)
    {
        return 0f;
    }

    if (initiator.Ideo == null || recipient.Ideo == null)
    {
        return 0f;
    }

    float baseSelectionWeight = 0.025f * modSettings.BaseInteractionFrequency;

    if (initiator.Ideo == recipient.Ideo)
    {
        return baseSelectionWeight;
    }
    else
    {
        return baseSelectionWeight * 0.2f;
    }
}
```
- **Base Selection Weight:** 0.025
- **Faction Adjustment:** 1.0 if same ideology, 0.2 if different ideology

#### 5. Skill Sharing

**RandomSelectionWeight Method:**
```csharp
public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
{
    if (initiator.Faction != Faction.OfPlayer && recipient.Faction != Faction.OfPlayer)
    {
        return 0f;
    }

    SkillRecord highestSkill = Verse.GenCollection.MaxBy(initiator.skills.skills, s => s.Level);

    if (highestSkill != null && recipient.skills.GetSkill(highestSkill.def).Level < highestSkill.Level)
    {
        float weight = (highestSkill.Level - recipient.skills.GetSkill(highestSkill.def).Level) / SkillDifferenceDivisor * initiator.needs.mood.CurLevel * BaseSelectionWeight;

        if (initiator.Faction == recipient.Faction)
        {
            return weight * modSettings.BaseInteractionFrequency;
        }
        else
        {
            return weight * NonColonyPawnFactor * modSettings.BaseInteractionFrequency;
        }
    }
    return 0f;
}
```
- **Base Selection Weight:** 0.01
- **Mood Factor:** Current mood level of the initiator
- **Skill Difference Factor:** Difference in skill levels divided by SkillDifferenceDivisor
- **Faction Adjustment:** 1.0 if same faction, NonColonyPawnFactor if different faction

### Summary

Each interaction's likelihood of occurring is influenced by multiple factors, primarily the mood of the pawns involved, their social skills, and specific conditions that apply to each interaction. The `BaseInteractionFrequency` setting in Positive Connections Mod Settings further scales these probabilities, allowing players to adjust the frequency of interactions.

By understanding these mathematical foundations, users can better appreciate how Positive Connections enhances the social dynamics of RimWorld, and modders can leverage this knowledge to create their own interactions or modify existing ones.