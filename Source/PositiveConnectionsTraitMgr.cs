using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using RimWorld;
using Verse;

namespace PositiveConnectionsNmSpc;

public static class PositiveConnectionsTraitMgr
{
    private static Dictionary<string, TraitDef> traitCache = new Dictionary<string, TraitDef>();
    private static Dictionary<string, bool> modStatusCache = new Dictionary<string, bool>();

    // Initialize trait-mod mappings, specifying mod dependencies only once here
    private static Dictionary<string, string> traitModMap = new Dictionary<string, string>
    {
        { "Kind", null }, // Core RimWorld trait
        { "MM_Empathetic", "cem.mindmatters" },
        { "SP_Caring", "SimplePersonalities" },
        { "SP_Empathetic", "SimplePersonalities" },
        { "MM_SelfCentered", "cem.mindmatters" },
        { "MM_Desensitized", "cem.mindmatters" },
        { "Psychopath", null },
        { "VTE_WorldWeary", "OskarPotocki.VanillaTraitsExpanded" }
    };

    static PositiveConnectionsTraitMgr()
    {
        // Preload mod activity statuses to avoid repeated checks
        foreach (var modId in new HashSet<string>(traitModMap.Values))
        {
            if (modId != null)
            {
                modStatusCache[modId] = ModsConfig.IsActive(modId);
            }
        }
    }

    // Returns whether a pawn has the specified trait, checking mod status automatically
    public static bool PawnHasTrait(Pawn pawn, string traitName)
    {
        if (!TryGetTraitDef(traitName, out TraitDef traitDef))
        {
            return false;
        }
        return pawn.story?.traits?.HasTrait(traitDef) ?? false;
    }

    // Try to retrieve the TraitDef, accounting for mod dependencies
    private static bool TryGetTraitDef(string traitName, out TraitDef traitDef)
    {
        // Return cached trait if it exists
        if (traitCache.TryGetValue(traitName, out traitDef))
        {
            return traitDef != null;
        }

        // Check if the trait has a mod dependency and if that mod is active
        if (traitModMap.TryGetValue(traitName, out string modId) && modId != null && !modStatusCache[modId])
        {
            traitCache[traitName] = null; // Cache absence if mod is inactive
            traitDef = null;
            return false;
        }

        // Attempt to retrieve and cache the trait if mod dependency is met
        traitDef = DefDatabase<TraitDef>.GetNamedSilentFail(traitName);
        traitCache[traitName] = traitDef; // Cache result (even if null for absent traits)
        return traitDef != null;
    }

    // Checks for Rare Interactions by requiring specific traits
    public static bool CanInitiateRareInteraction(Pawn initiator, List<string> requiredTraits)
    {
        foreach (string traitName in requiredTraits)
        {
            if (!PawnHasTrait(initiator, traitName))
            {
                return false;
            }
        }
        return true;
    }

    // Example helper to check multiple positive or negative traits at once
    public static bool PawnHasAnyTrait(Pawn pawn, List<string> traits)
    {
        foreach (string trait in traits)
        {
            if (PawnHasTrait(pawn, trait))
            {
                return true;
            }
        }
        return false;
    }
}