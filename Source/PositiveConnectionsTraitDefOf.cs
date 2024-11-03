using RimWorld;
using Verse;

namespace PositiveConnectionsNmSpc
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