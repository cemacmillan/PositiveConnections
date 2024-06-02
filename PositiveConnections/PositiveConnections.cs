using UnityEngine;
using Verse;

namespace DIL_PositiveConnections
{
    public class PositiveConnections : Mod
    {
        public static PositiveConnections Instance;
        public static bool IsMindMattersActive;

        public PositiveConnectionsModSettings settings; // Update the type here

        public PositiveConnections(ModContentPack content) : base(content)
        {
            Instance = this;
            settings = GetSettings<PositiveConnectionsModSettings>(); // Update the type here
            Log.Message("Positive connections v1.5.0");
            IsMindMattersActive = ModsConfig.IsActive("cem.mindmatters");

            if(IsMindMattersActive)
            {
                Log.Message("Mind Matters detected!");
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
            var settingsWindow = new PositiveConnectionsModSettingsWindow(settings);
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
