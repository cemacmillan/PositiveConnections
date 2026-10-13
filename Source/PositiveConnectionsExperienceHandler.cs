using System;
using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using Verse;

namespace PositiveConnectionsNmSpc;

public static class PositiveConnectionsExperienceHandler
{
    private static PositiveConnectionsModSettings modSettings = PositiveConnections.settings;

    /// <summary>
    /// Valency is passed as string (e.g. "Positive", "Affirming") so we never load MindMattersInterface at compile time.
    /// When MindMatters is active we call MindMattersAPI.NotifyExperience via reflection.
    /// </summary>
    public static void HandlePositiveExperience(
        Pawn initiator,
        Pawn recipient,
        string eventType,
        float probability,
        string valencyName,
        HashSet<string> tags = null)
    {
        if (initiator == null || recipient == null || string.IsNullOrEmpty(eventType))
        {
            Log.Warning("[PositiveConnections] Invalid input to HandlePositiveExperience. Event skipped.");
            return;
        }

        if (!Rand.Chance(probability) || !PositiveConnections.IsMindMattersActive)
            return;

        HashSet<string> recipientTags = tags != null
            ? new HashSet<string>(tags) { $"{eventType}", "Recipient", "social" }
            : new HashSet<string> { $"{eventType}", "Recipient", "social" };
        HashSet<string> initiatorTags = tags != null
            ? new HashSet<string>(tags) { $"{eventType}_Initiator", "Initiator", "social" }
            : new HashSet<string> { $"{eventType}_Initiator", "Initiator", "social" };
        if (!TryNotifyMindMatters(recipient, eventType, valencyName, recipientTags, initiator))
            return;
        if (!TryNotifyMindMatters(initiator, $"{eventType}_Initiator", valencyName, initiatorTags, recipient))
            return;

        if (modSettings.EnableLogging)
            Log.Message($"[PositiveConnections] Experience '{eventType}' conveyed to recipient '{recipient.LabelShort}' and initiator '{initiator.LabelShort}'.");
    }

    /// <summary>
    /// Call MindMattersInterface.MindMattersAPI.NotifyExperience via reflection. Returns false if the API is not available (no crash).
    /// </summary>
    private static bool TryNotifyMindMatters(Pawn pawn, string eventType, string valencyName, HashSet<string> tags, Pawn relatedPawn)
    {
        try
        {
            Assembly asm = FindMindMattersInterfaceAssembly();
            if (asm == null)
                return false;

            Type apiType = asm.GetType("MindMattersInterface.MindMattersAPI");
            Type valencyType = asm.GetType("MindMattersInterface.ExperienceValency");
            if (apiType == null || valencyType == null)
                return false;

            object valencyEnum = Enum.Parse(valencyType, valencyName, ignoreCase: true);
            if (valencyEnum == null)
                return false;

            MethodInfo method = apiType.GetMethod("NotifyExperience",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new Type[] { typeof(Pawn), typeof(string), valencyType, typeof(HashSet<string>), typeof(int) },
                null);
            if (method != null)
            {
                int relatedId = relatedPawn?.thingIDNumber ?? -1;
                method.Invoke(null, new object[] { pawn, eventType, valencyEnum, tags, relatedId });
                return true;
            }

            method = apiType.GetMethod("NotifyExperience",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new Type[] { typeof(Pawn), typeof(string), valencyType, typeof(HashSet<string>) },
                null);
            if (method == null)
                return false;

            method.Invoke(null, new object[] { pawn, eventType, valencyEnum, tags });
            return true;
        }
        catch (Exception ex)
        {
            if (modSettings.EnableLogging)
                Log.Warning($"[PositiveConnections] MindMatters NotifyExperience failed: {ex.Message}");
            return false;
        }
    }

    private static Assembly FindMindMattersInterfaceAssembly()
    {
        string name = "MindMattersInterface";
        foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (asm.GetName().Name == name)
                return asm;
        }
        return null;
    }
}
