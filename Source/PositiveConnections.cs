
using RimWorld;
using Verse;
using UnityEngine;

namespace PositiveConnectionsNmSpc
{
    public class PositiveConnections : Mod
    {
        internal static PositiveConnections Instance;
        // ReSharper disable once MemberCanBePrivate.Global - useful for debugging at present
        internal static bool IsMindMattersActive;

        internal static PositiveConnectionsModSettings settings;

        public PositiveConnections(ModContentPack content) : base(content)
        {
            Instance = this;
            settings = GetSettings<PositiveConnectionsModSettings>(); // Update the type here
            Log.Message("<color=#00FF7F>[Positive Connections]</color>v1.6.1.1 someday-we-relax-corrective");
            IsMindMattersActive = ModsConfig.IsActive("cem.mindmatterspr") || ModsConfig.IsActive("cem.mindmatters");

            if(IsMindMattersActive)
            {
                Log.Message("<color=#00FF7F>[Positive Connections]</color>Mind Matters detected!");
            }
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            if (settings == null)
            {
                Log.Error("ModSettings for PositiveConnections is null");
                return;
            }

            // Open the settings window
            PositiveConnectionsModSettingsWindow settingsWindow = new PositiveConnectionsModSettingsWindow(settings);
            settingsWindow.DoWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Positive Connections";
        }

        static void Mod_PositiveConnections_PostInit()
        {

        }

    }
}
