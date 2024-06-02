using RimWorld;
using Verse;

namespace DIL_PositiveConnections
{
    [DefOf]
    public static class TraitDefOf
    {
        //public static TraitDef Beauty;
        public static TraitDef Kind;
        //public static TraitDef DefaultTrait;

        static TraitDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(TraitDefOf));
            // Check if Beauty is null and provide a fallback
            //if (Beauty == null)
            //{
            //    Log.Warning("Beauty trait not found. Defaulting to DefaultTrait.");
            //    Beauty = DefaultTrait;
            //}
        }
    }
}