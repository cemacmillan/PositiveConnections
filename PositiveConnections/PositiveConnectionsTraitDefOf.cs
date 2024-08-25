using RimWorld;
using Verse;

namespace DIL_PositiveConnections
{
    [DefOf]
    public static class TraitDefOf
    {
        
        public static TraitDef Kind;
       
        static TraitDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(TraitDefOf));
        }
    }
}