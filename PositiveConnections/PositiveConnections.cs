using UnityEngine;
using Verse;

namespace dIl_PositiveConnections
{
    public class Mod_PositiveConnections : Mod
    {
        private ModSettings settings;

        public Mod_PositiveConnections(ModContentPack content) : base(content)
        {
            settings = GetSettings<ModSettings>();
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
    }
}
